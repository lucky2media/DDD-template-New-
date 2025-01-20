#if !UNITY_WEBGL || UNITY_EDITOR
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

using Best.HTTP.HostSetting;
using Best.HTTP.Shared.Extensions;
using Best.HTTP.Shared.PlatformSupport.Memory;
using Best.HTTP.Shared.PlatformSupport.Network.Tcp;
using Best.HTTP.Shared.Streams;

using Best.MQTT.Packets;

using static Best.HTTP.Shared.HTTPManager;

namespace Best.MQTT.Transports
{
    /// <summary>
    /// Transport to use raw TCP connections with optional secure (TLS) layer use.
    /// </summary>
    public sealed class SecureTCPTransport : Transport, IHeartbeat, INegotiationPeer, IContentConsumer
    {
        public PeekableContentProviderStream ContentProvider { get; private set; }

        /// <summary>
        /// Queue for sending packets
        /// </summary>
        private ConcurrentQueue<BufferSegment> _buffers = new ConcurrentQueue<BufferSegment>();

        /// <summary>
        /// Event to signal the sending thread.
        /// </summary>
        private AutoResetEvent _bufferAvailableEvent = new AutoResetEvent(false);

        private volatile bool closed;
        private volatile int runningThreadCount;

        private Negotiator _negotiator;

        public SecureTCPTransport(MQTTClient client)
            :base(client)
        {
        }

        internal override void BeginConnect(CancellationToken token = default)
        {
            base.BeginConnect(token);

            if (token.IsCancellationRequested)
                return;

            if (this.State != TransportStates.Initial)
                throw new Exception($"{nameof(SecureTCPTransport)} couldn't {nameof(BeginConnect)} as it's already in state {this.State}!");

            Logger.Information(nameof(SecureTCPTransport), $"{nameof(BeginConnect)}", this.Context);

            var options = this.Parent.Options;

            NegotiationParameters parameters = new NegotiationParameters();
            parameters.context = this.Context;
            //parameters.tryToKeepAlive = true;
            parameters.proxy = Proxy;
            parameters.createProxyTunel = true;
            parameters.targetUri = new Uri($"tcp://{options.Host}:{options.Port}");
            parameters.negotiateTLS = options.UseTLS;
            //parameters.optionalRequest = null;
            parameters.token = token;

            parameters.hostSettings = PerHostSettings.Get(HostKey.From(parameters.targetUri, new Best.HTTP.Request.Settings.ProxySettings() { Proxy = parameters.proxy }));

            this._negotiator = new Negotiator(this, parameters);
            this._negotiator.Start();
        }

        List<string> INegotiationPeer.GetSupportedProtocolNames(Negotiator negotiator) => new List<string> { Best.HTTP.Hosts.Connections.HTTPProtocolFactory.W3C_HTTP1 };

        bool INegotiationPeer.MustStopAdvancingToNextStep(Negotiator negotiator, NegotiationSteps finishedStep, NegotiationSteps nextStep, Exception error)
        {
            bool stop = negotiator.Parameters.token.IsCancellationRequested || error != null;

            if (stop)
                this.transportEvents.Enqueue(new TransportEvent(TransportEventTypes.StateChange, TransportStates.DisconnectedWithError, error?.ToString() ?? "IsCancellationRequested"));

            return stop;
        }

        void INegotiationPeer.EvaluateProxyNegotiationFailure(Negotiator negotiator, Exception error, bool resendForAuthentication)
        {
            this.transportEvents.Enqueue(new TransportEvent(TransportEventTypes.StateChange, TransportStates.DisconnectedWithError, error?.ToString() ?? "Proxy authentication failed"));
        }

        void INegotiationPeer.OnNegotiationFailed(Negotiator negotiator, Exception error)
        {
            this.transportEvents.Enqueue(new TransportEvent(TransportEventTypes.StateChange, TransportStates.DisconnectedWithError, error.ToString()));
        }

        void INegotiationPeer.OnNegotiationFinished(Negotiator negotiator, PeekableContentProviderStream stream, TCPStreamer streamer, string negotiatedProtocol)
        {
            this.transportEvents.Enqueue(new TransportEvent(TransportEventTypes.StateChange, TransportStates.Connected));

            //(stream as IPeekableContentProvider).Consumer = this;
            stream.SetTwoWayBinding(this);

            Best.HTTP.Shared.PlatformSupport.Threading.ThreadedRunner.RunLongLiving(SendThread);
        }

        internal override void Send(BufferSegment buffer)
        {
            if (this.State != TransportStates.Connected)
            {
                Logger.Warning(nameof(SecureTCPTransport), $"Send called while it's not in the Connected state! State: {this.State}", this.Context);
                return;
            }

            Logger.Information(nameof(SecureTCPTransport), $"{nameof(Send)}({buffer})", this.Context);
            this._buffers.Enqueue(buffer);
            this._bufferAvailableEvent.Set();
        }

        internal override void BeginDisconnect()
        {
            if (this.State >= TransportStates.Disconnecting)
                return;

            ChangeStateTo(TransportStates.Disconnecting, string.Empty);
            try
            {
                this.closed = true;
                this._bufferAvailableEvent.Set();
            }
            catch
            {
                ChangeStateTo(TransportStates.Disconnected, string.Empty);
            }
        }

        private void SendThread()
        {
            try
            {
                Interlocked.Increment(ref this.runningThreadCount);
                Logger.Information(nameof(SecureTCPTransport), $"{nameof(SendThread)} is up and running! runningThreadCount: {this.runningThreadCount}", this.Context);

                while (!closed)
                {
                    this._bufferAvailableEvent.WaitOne();
                    while (this._buffers.TryDequeue(out BufferSegment buff))
                    {
                        this._negotiator.Stream.Write(buff);
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.Exception(nameof(SecureTCPTransport), nameof(SendThread), ex, this.Context);
            }
            finally
            {
                Interlocked.Decrement(ref this.runningThreadCount);
                this.transportEvents.Enqueue(new TransportEvent(TransportEventTypes.StateChange, TransportStates.Disconnected));
            }
        }

        // Call it every time we received a Disconnected event. This means that CleanupAfterDisconnect going to be called 2-3 times.
        protected override void CleanupAfterDisconnect()
        {
            // Run cleanup logic only when both threads are closed
            if (this.runningThreadCount == 0 && this._bufferAvailableEvent != null)
            {
                base.CleanupAfterDisconnect();

                this._bufferAvailableEvent?.Dispose();
                this._bufferAvailableEvent = null;

                this._negotiator?.Stream?.Dispose();

                Logger.Information(nameof(SecureTCPTransport), $"{nameof(CleanupAfterDisconnect)} finished!", this.Context);
            }
        }

        public void SetBinding(PeekableContentProviderStream contentProvider) => this.ContentProvider = contentProvider;
        public void UnsetBinding() => this.ContentProvider = null;

        public void OnContent()
        {
            var available = this.ContentProvider.Length;
            var buffer = BufferPool.Get(available, true);
            int count = this.ContentProvider.Read(buffer, 0, (int)available);

            if (count == 0)
            {
                //this.closed = true;
                //this._bufferAvailableEvent.Set();
                //this.transportEvents.Enqueue(new TransportEvent(TransportEventTypes.StateChange, TransportStates.DisconnectedWithError, "TCP closed"));
                //return;
                //throw new Exception("TCP connection closed unexpectedly!");
            }
            
            this.ReceiveStream.Write(new BufferSegment(buffer, 0, count));

            Logger.Information(nameof(SecureTCPTransport), $"{nameof(OnContent)} - Received ({count})", this.Context);

            try
            {
                TryParseIncomingPackets();
            }
            catch (MQTTException ex)
            {
                this.transportEvents.Enqueue(new TransportEvent(TransportEventTypes.MQTTException, ex, nameof(TryParseIncomingPackets)));
            }
            catch (Exception ex)
            {
                Logger.Exception(nameof(SecureTCPTransport), $"{nameof(OnContent)}.TryParseIncomingPackets", ex, this.Context);
            }
        }

        public void OnConnectionClosed()
        {
            if (State == TransportStates.Disconnecting)
                this.transportEvents.Enqueue(new TransportEvent(TransportEventTypes.StateChange, TransportStates.Disconnected));
            else
                this.transportEvents.Enqueue(new TransportEvent(TransportEventTypes.StateChange, TransportStates.DisconnectedWithError, "TCP closed by remote peer."));
        }

        public void OnError(Exception ex)
        {
            this.transportEvents.Enqueue(new TransportEvent(TransportEventTypes.StateChange, TransportStates.DisconnectedWithError, ex.Message));
        }
    }
}

#endif
