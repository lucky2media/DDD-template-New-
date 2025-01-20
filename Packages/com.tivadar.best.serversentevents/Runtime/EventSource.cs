using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

using Best.HTTP;
using Best.HTTP.Response;
using Best.HTTP.Shared;
using Best.HTTP.Shared.Extensions;
using Best.HTTP.Shared.Logger;
using Best.HTTP.Shared.PlatformSupport.Memory;

using static Best.HTTP.Hosts.Connections.HTTP1.Constants;

#if UNITY_WEBGL && !UNITY_EDITOR
    using System.Runtime.InteropServices;
#endif

namespace Best.ServerSentEvents
{
    
    public delegate void OnGeneralEventDelegate(EventSource eventSource);
    public delegate void OnMessageDelegate(EventSource eventSource, Best.ServerSentEvents.Message message);
    public delegate void OnErrorDelegate(EventSource eventSource, string error);
    public delegate bool OnRetryDelegate(EventSource eventSource);
    public delegate void OnEventDelegate(EventSource eventSource, Best.ServerSentEvents.Message message);
    public delegate void OnStateChangedDelegate(EventSource eventSource, States oldState, States newState);

#if !UNITY_WEBGL || UNITY_EDITOR
    public delegate void OnCommentDelegate(EventSource eventSource, string comment);
#endif

#if UNITY_WEBGL && !UNITY_EDITOR

    delegate void OnWebGLEventSourceOpenDelegate(uint id);
    delegate void OnWebGLEventSourceMessageDelegate(uint id, string eventStr, string data, string eventId, int retry);
    delegate void OnWebGLEventSourceErrorDelegate(uint id, string reason);
#endif

    /// <summary>
    /// Represents the possible states of an <see cref="EventSource"/> object.
    /// </summary>
    public enum States
    {
        /// <summary>
        /// Indicates the initial state of the <see cref="EventSource"/>, before any action has been taken.
        /// </summary>
        Initial,

        /// <summary>
        /// Represents the state when the <see cref="EventSource"/> is attempting to establish a connection.
        /// </summary>
        Connecting,

        /// <summary>
        /// Indicates that the <see cref="EventSource"/> has successfully established a connection.
        /// </summary>
        Open,

        /// <summary>
        /// Represents the state when the <see cref="EventSource"/> is attempting to reconnect after a connection loss.
        /// </summary>
        Retrying,

        /// <summary>
        /// Indicates that the <see cref="EventSource"/> is in the process of shutting down the connection.
        /// </summary>
        Closing,

        /// <summary>
        /// Represents the state when the <see cref="EventSource"/> has completely closed the connection.
        /// </summary>
        Closed
    }


    /// <summary>
    /// The EventSource class provides an implementation of the <see href="https://html.spec.whatwg.org/multipage/server-sent-events.html">Server-Sent Events (SSE) standard</see>.
    /// SSE is a simple and efficient protocol to receive real-time updates over HTTP. Instead of continually polling the server for changes,
    /// clients can open a single connection and receive updates as they happen, making it a powerful tool for building real-time applications.
    /// 
    /// Why Use EventSource:
    /// <list type="bullet">
    ///     <item><description>Reduces the amount of redundant data sent over the network, since there's no need to repeatedly poll the server.</description></item>
    ///     <item><description>Simplifies the client-side logic, as the protocol handles reconnects and manages the connection state.</description></item>
    ///     <item><description>Offers a standardized way (via <see href="https://html.spec.whatwg.org/multipage/server-sent-events.html">the W3C standard</see>) to achieve real-time communication in web applications.</description></item>
    /// </list>
    /// <para>This class encapsulates the complexity of the SSE protocol, offering an easy-to-use API for both sending and receiving real-time updates.</para>
    /// </summary>
    /// <remarks>
    /// How It Works:
    /// <list type="bullet">
    ///     <item><description>A client makes a request to an SSE-supporting server, specifying that it expects the server to keep the connection open.</description></item>
    ///     <item><description>The server holds the connection and sends data whenever a new event occurs. Each event is sent as a block of text terminated by a pair of newline characters.</description></item>
    ///     <item><description>Clients process these events as they arrive.</description></item>
    /// </list>
    /// </remarks>
    public class EventSource
#if !UNITY_WEBGL || UNITY_EDITOR
        : IHeartbeat
#endif

    {
        #region Public Properties

        /// <summary>
        /// Gets the URI of the remote endpoint.
        /// </summary>
        public Uri Uri { get; private set; }

        /// <summary>
        /// Gets the current state of the EventSource object.
        /// </summary>
        public States State
        {
            get
            {
                return _state;
            }
            private set
            {
                States oldState = _state;
                _state = value;

                if (OnStateChanged != null)
                {
                    try
                    {
                        OnStateChanged(this, oldState, _state);
                    }
                    catch(Exception ex)
                    {
                        HTTPManager.Logger.Exception("EventSource", "OnStateChanged", ex);
                    }
                }
            }
        }
        private States _state;

        /// <summary>
        /// Gets or sets the duration to wait before attempting a reconnection. Defaults to 2 seconds.
        /// </summary>
        /// <remarks>The server can override this setting.</remarks>
        public TimeSpan ReconnectionTime { get; set; }

        /// <summary>
        /// Gets the ID of the last successfully received event.
        /// </summary>
        public string LastEventId { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the EventSource is in the <see cref="States.Closed"/> state.
        /// </summary>
        public bool IsClosed { get { return this.State == States.Closed; } }

        /// <summary>
        /// Gets the logging context for the current EventSource instance.
        /// </summary>
        public LoggingContext LoggingContext { get; private set; }

#if !UNITY_WEBGL || UNITY_EDITOR
        /// <summary>
        /// Gets the internal <see cref="HTTPRequest"/> object used by the EventSource for communication.
        /// </summary>
        public HTTP.HTTPRequest InternalRequest { get; private set; }

#else
        /// <summary>
        /// Gets or sets a value indicating whether to send credentials like cookies and authorization headers with the request.
        /// </summary>
        public bool WithCredentials { get; set; }
#endif

        #endregion

        #region Public Events

        /// <summary>
        /// Called when successfully connected to the server.
        /// </summary>
        public event OnGeneralEventDelegate OnOpen;

        /// <summary>
        /// Called on every message received from the server.
        /// </summary>
        public event OnMessageDelegate OnMessage;

        /// <summary>
        /// Called when an error occurs.
        /// </summary>
        public event OnErrorDelegate OnError;

#if !UNITY_WEBGL || UNITY_EDITOR
        /// <summary>
        /// Called when the EventSource will try to do a retry attempt. If this function returns with false, it will cancel the attempt.
        /// </summary>
        public event OnRetryDelegate OnRetry;

        /// <summary>
        /// This event is called for comments received from the server.
        /// </summary>
        public event OnCommentDelegate OnComment;
#endif

        /// <summary>
        /// Called when the EventSource object closed.
        /// </summary>
        public event OnGeneralEventDelegate OnClosed;

        /// <summary>
        /// Called every time when the State property changed.
        /// </summary>
        public event OnStateChangedDelegate OnStateChanged;

#endregion

#region Privates

        /// <summary>
        /// A dictionary to store eventName => delegate mapping.
        /// </summary>
        private Dictionary<string, OnEventDelegate> EventTable;

#if !UNITY_WEBGL || UNITY_EDITOR
        /// <summary>
        /// Number of retry attempts made.
        /// </summary>
        private byte RetryCount;

        /// <summary>
        /// When we called the Retry function. We will delay the Open call from here.
        /// </summary>
        private DateTime RetryCalled;

        /// <summary>
        /// Buffer for the read data.
        /// </summary>
        private byte[] LineBuffer;

        /// <summary>
        /// Buffer position.
        /// </summary>
        private int LineBufferPos = 0;

        /// <summary>
        /// The currently receiving and parsing message
        /// </summary>
        private Best.ServerSentEvents.Message CurrentMessage;

        /// <summary>
        /// Completed messages that waiting to be dispatched
        /// </summary>
        //private List<Best.ServerSentEvents.Message> CompletedMessages = new List<Best.ServerSentEvents.Message>();
        private ConcurrentQueue<Best.ServerSentEvents.Message> CompletedMessages = new ConcurrentQueue<Message>();

#else
        private static Dictionary<uint, EventSource> EventSources = new Dictionary<uint, EventSource>();
        private uint Id;
#endif

#endregion

        public EventSource(Uri uri)
        {
            this.Uri = uri;
            this.LoggingContext = new LoggingContext(this);

            this.ReconnectionTime = TimeSpan.FromMilliseconds(2000);

#if !UNITY_WEBGL || UNITY_EDITOR
            CreateInternalRequest();
#else
            if (!ES_IsSupported())
              throw new NotSupportedException("This browser has no support for the EventSource protocol!");

            this.Id = ES_Create(this.Uri.ToString(), WithCredentials, OnOpenCallback, OnMessageCallback, OnErrorCallback);

            EventSources.Add(this.Id, this);
#endif
        }

#region Public Functions

        /// <summary>
        /// Start to connect to the remote server.
        /// </summary>
        public void Open()
        {
            if (this.State != States.Initial &&
                this.State != States.Retrying &&
                this.State != States.Closed)
                return;

            this.State = States.Connecting;

#if !UNITY_WEBGL || UNITY_EDITOR
            if (!string.IsNullOrEmpty(this.LastEventId))
                this.InternalRequest.SetHeader("Last-Event-ID", this.LastEventId);

            this.InternalRequest.Send();
#endif
        }

        /// <summary>
        /// Start to close the connection.
        /// </summary>
        public void Close()
        {
            if (this.State == States.Closing ||
                this.State == States.Closed)
                return;

            this.State = States.Closing;
#if !UNITY_WEBGL || UNITY_EDITOR
            if (this.InternalRequest != null)
                this.CancellationRequested();
            else
                this.State = States.Closed;
#else
            ES_Close(this.Id);

            SetClosed("Close");

            EventSources.Remove(this.Id);

            ES_Release(this.Id);
#endif
        }

        /// <summary>
        /// With this function an event handler can be subscribed for an event name.
        /// </summary>
        public void On(string eventName, OnEventDelegate action)
        {
            if (EventTable == null)
                EventTable = new Dictionary<string, OnEventDelegate>();

            EventTable[eventName] = action;
#if UNITY_WEBGL && !UNITY_EDITOR
            ES_AddEventHandler(this.Id, eventName);
#endif
        }

        /// <summary>
        /// With this function the event handler can be removed for the given event name.
        /// </summary>
        /// <remarks>The event is still will be sent by the server and processed by the client.</remarks>
        /// <param name="eventName">The name of the event to unsubscribe from.</param>
        public void Off(string eventName)
        {
            if (eventName == null || EventTable == null)
                return;

            EventTable.Remove(eventName);
        }

#endregion

#region Private Helper Functions

#if !UNITY_WEBGL || UNITY_EDITOR
        private HTTP.HTTPRequest CreateInternalRequest()
        {
            this.InternalRequest = new HTTPRequest(Uri, HTTPMethods.Get, OnRequestFinished);
            this.InternalRequest.DownloadSettings.DisableCache = true;

            // Set headers
            this.InternalRequest.SetHeader("Accept", "text/event-stream");
            this.InternalRequest.SetHeader("Cache-Control", "no-cache");
            this.InternalRequest.SetHeader("Accept-Encoding", "identity");

            // set up streaming
            this.InternalRequest.DownloadSettings.OnDownloadStarted = OnDownloadStarted;

            // Disable internal retry
            this.InternalRequest.RetrySettings.MaxRetries = 0;

            this.InternalRequest.Context.Add("EventSource", this.LoggingContext);

            return this.InternalRequest;
        }
#endif

        private void CallOnError(string error, string msg)
        {
            if (OnError != null)
            {
                try
                {
                    OnError(this, error);
                }
                catch (Exception ex)
                {
                    HTTPManager.Logger.Exception("EventSource", msg + " - OnError", ex, this.LoggingContext);
                }
            }
        }

#if !UNITY_WEBGL || UNITY_EDITOR
        private bool CallOnRetry()
        {
            if (OnRetry != null)
            {
                try
                {
                    return OnRetry(this);
                }
                catch(Exception ex)
                {
                    HTTPManager.Logger.Exception("EventSource", "CallOnRetry", ex, this.LoggingContext);
                }
            }

            return true;
        }
#endif

        private void SetClosed(string msg)
        {
            this.State = States.Closed;

            if (OnClosed != null)
            {
                try
                {
                    OnClosed(this);
                }
                catch (Exception ex)
                {
                    HTTPManager.Logger.Exception("EventSource", msg + " - OnClosed", ex, this.LoggingContext);
                }
            }
        }

#if !UNITY_WEBGL || UNITY_EDITOR
        private void Retry()
        {
            if (RetryCount > 0 ||
                !CallOnRetry())
            {
                SetClosed("Retry");
                return;
            }

            RetryCount++;
            RetryCalled = HTTPManager.CurrentFrameDateTime;

            HTTPManager.Heartbeats.Subscribe(this);

            this.State = States.Retrying;
        }
#endif

#endregion

#region HTTP Request Implementation
#if !UNITY_WEBGL || UNITY_EDITOR
        private void OnRequestFinished(HTTP.HTTPRequest req, HTTP.HTTPResponse resp)
        {
            HTTPManager.Logger.Information("EventSource", string.Format("OnRequestFinished - State: {0}, StatusCode: {1}", this.State, resp != null ? resp.StatusCode : 0), req.Context);

            if (this._downStream != null)
                return;

            if (this.State == States.Closed)
                return;

            if (this.State == States.Closing || req.IsCancellationRequested)
            {
                SetClosed("OnRequestFinished");

                return;
            }

            string reason = string.Empty;

            // In some cases retry is prohibited
            bool canRetry = true;

            switch (req.State)
            {
                // The request finished without any problem.
                case HTTP.HTTPRequestStates.Finished:
                    // HTTP 200 OK responses that have a Content-Type specifying an unsupported type, or that have no Content-Type at all, must cause the user agent to fail the connection.
                    bool hasEventStreamHeader = resp.HasHeaderWithValue("content-type", "text/event-stream");
                    if (resp.StatusCode == HTTPStatusCodes.OK && !hasEventStreamHeader)
                    {
                        reason = "No Content-Type header with value 'text/event-stream' present.";
                        canRetry = false;
                    }
                    else if (resp.StatusCode == HTTPStatusCodes.OK && hasEventStreamHeader)
                    {
                        reason = string.Empty;
                        canRetry = false;
                    }

                    // HTTP 500 Internal Server Error, 502 Bad Gateway, 503 Service Unavailable, and 504 Gateway Timeout responses, and any network error that prevents the connection
                    //  from being established in the first place (e.g. DNS errors), must cause the user agent to asynchronously reestablish the connection.
                    // Any other HTTP response code not listed here must cause the user agent to fail the connection.
                    if (canRetry &&
                        resp.StatusCode != HTTPStatusCodes.InternalServerError && // 500
                        resp.StatusCode != HTTPStatusCodes.BadGateway && // 502
                        resp.StatusCode != HTTPStatusCodes.ServiceUnavailable && // 503
                        resp.StatusCode != HTTPStatusCodes.GatewayTimeout) // 504
                    {
                        canRetry = false;

                        reason = string.Format("Request Finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2}",
                                                        resp.StatusCode,
                                                        resp.Message,
                                                        resp.DataAsText);
                    }
                    break;

                // The request finished with an unexpected error. The request's Exception property may contain more info about the error.
                case HTTP.HTTPRequestStates.Error:
                    reason = "Request Finished with Error! " + (req.Exception != null ? (req.Exception.Message + "\n" + req.Exception.StackTrace) : "No Exception");
                    break;

                // The request aborted, initiated by the user.
                case HTTP.HTTPRequestStates.Aborted:
                    // If the state is Closing, then it's a normal behaviour, and we close the EventSource
                    reason = "OnRequestFinished - Aborted without request. EventSource's State: " + this.State;
                    break;

                // Connecting to the server is timed out.
                case HTTP.HTTPRequestStates.ConnectionTimedOut:
                    reason = "Connection Timed Out!";
                    break;

                // The request didn't finished in the given time.
                case HTTP.HTTPRequestStates.TimedOut:
                    reason = "Processing the request Timed Out!";
                    break;
            }

            // If we are not closing the EventSource, then we will try to reconnect.
            if (this.State < States.Closing)
            {
                if (!string.IsNullOrEmpty(reason))
                    CallOnError(reason, "OnRequestFinished");

                if (canRetry)
                    Retry();
                else
                    SetClosed("OnRequestFinished");
            }
            else
                SetClosed("OnRequestFinished");
        }

        private void OnDownloadStarted(HTTPRequest req, HTTPResponse resp, DownloadContentStream stream)
        {
            HTTPManager.Logger.Information(nameof(EventSource), $"OnDownloadStarted({req}, {resp}, {stream})", this.LoggingContext);

            if (this.State == States.Connecting)
            {
                string contentType = resp.GetFirstHeaderValue("content-type");
                bool IsUpgraded = resp.StatusCode == 200 &&
                                  !string.IsNullOrEmpty(contentType) &&
                                  contentType.ToLowerInvariant().StartsWith("text/event-stream");

                if (IsUpgraded)
                {
                    if (this.OnOpen != null)
                    {
                        try
                        {
                            this.OnOpen(this);
                        }
                        catch (Exception ex)
                        {
                            HTTPManager.Logger.Exception("EventSource", "OnOpen", ex, this.LoggingContext);
                        }
                    }

                    this.RetryCount = 0;
                    this.State = States.Open;

                    // Start the coroutine to process downloaded chunks on unity's main thread.
                    HTTPUpdateDelegator.Instance.StartCoroutine(DataProcessor(req, resp, stream));
                }
                else
                {
                    this.State = States.Closing;
                }
            }
        }

        DownloadContentStream _downStream;
        IEnumerator DataProcessor(HTTPRequest req, HTTPResponse resp, DownloadContentStream stream)
        {
            try
            {
                this._downStream = stream;
                while (!this._downStream.IsCompleted)
                {
                    if (this.State == States.Closing)
                        yield break;

                    while (this._downStream.TryTake(out var buffer))
                        if (FeedData(buffer))
                            HandleEvents();

                    yield return null;
                }
            }
            finally
            {
                this._downStream.Dispose();
                this._downStream = null;

                OnRequestFinished(req, resp);
            }
        }

#region Data Parsing

        public bool FeedData(BufferSegment buffer)
        {
            if (buffer.Count == 0)
                return false;

            if (LineBuffer == null)
                LineBuffer = BufferPool.Get(1024, true);

            int newlineIdx;
            int pos = buffer.Offset;
            bool hasMessageToSend = false;

            do
            {

                newlineIdx = -1;
                int skipCount = 1; // to skip CR and/or LF

                for (int i = pos; i < buffer.Count && newlineIdx == -1; ++i)
                {
                    // Lines must be separated by either a U+000D CARRIAGE RETURN U+000A LINE FEED (CRLF) character pair, a single U+000A LINE FEED (LF) character, or a single U+000D CARRIAGE RETURN (CR) character.
                    if (buffer.Data[i] == CR)
                    {
                        if (i + 1 < buffer.Count && buffer.Data[i + 1] == LF)
                            skipCount = 2;
                        newlineIdx = i;
                    }
                    else if (buffer.Data[i] == LF)
                        newlineIdx = i;
                }

                int copyIndex = newlineIdx == -1 ? buffer.Count : newlineIdx;

                if (LineBuffer.Length < LineBufferPos + (copyIndex - pos))
                {
                    int newSize = LineBufferPos + (copyIndex - pos);
                    BufferPool.Resize(ref LineBuffer, newSize, true, false);
                }

                Array.Copy(buffer, pos, LineBuffer, LineBufferPos, copyIndex - pos);

                LineBufferPos += copyIndex - pos;

                if (newlineIdx == -1)
                    return hasMessageToSend;

                hasMessageToSend |= ParseLine(LineBuffer, LineBufferPos);

                LineBufferPos = 0;
                pos = newlineIdx + skipCount;

            } while (newlineIdx != -1 && pos < buffer.Count);

            return hasMessageToSend;
        }

        bool ParseLine(byte[] buffer, int count)
        {
            // If the line is empty (a blank line) => Dispatch the event
            if (count == 0)
            {
                if (CurrentMessage != null)
                {
                    CompletedMessages.Enqueue(CurrentMessage);
                    CurrentMessage = null;

                    return true;
                }

                return false;
            }

            // If the line starts with a U+003A COLON character (:) => Ignore the line.
            if (buffer[0] == 0x3A)
            {
                this.CompletedMessages.Enqueue(new Message() { IsComment = true, Data = Encoding.UTF8.GetString(buffer, 1, count - 1) });
                return true;
            }

            //If the line contains a U+003A COLON character (:)
            int colonIdx = -1;
            for (int i = 0; i < count && colonIdx == -1; ++i)
                if (buffer[i] == 0x3A)
                    colonIdx = i;

            string field;
            string value;

            if (colonIdx != -1)
            {
                // Collect the characters on the line before the first U+003A COLON character (:), and let field be that string.
                field = Encoding.UTF8.GetString(buffer, 0, colonIdx);

                //Collect the characters on the line after the first U+003A COLON character (:), and let value be that string. If value starts with a U+0020 SPACE character, remove it from value.
                if (colonIdx + 1 < count && buffer[colonIdx + 1] == 0x20)
                    colonIdx++;

                colonIdx++;

                // discarded because it is not followed by a blank line
                if (colonIdx >= count)
                    return false;

                value = Encoding.UTF8.GetString(buffer, colonIdx, count - colonIdx);
            }
            else
            {
                // Otherwise, the string is not empty but does not contain a U+003A COLON character (:) =>
                //      Process the field using the whole line as the field name, and the empty string as the field value.
                field = Encoding.UTF8.GetString(buffer, 0, count);
                value = string.Empty;
            }

            if (CurrentMessage == null)
                CurrentMessage = new Best.ServerSentEvents.Message();

            switch (field)
            {
                // If the field name is "id" => Set the last event ID buffer to the field value.
                case "id":
                    CurrentMessage.Id = value;
                    break;

                // If the field name is "event" => Set the event type buffer to field value.
                case "event":
                    CurrentMessage.Event = value;
                    break;

                // If the field name is "data" => Append the field value to the data buffer, then append a single U+000A LINE FEED (LF) character to the data buffer.
                case "data":
                    // Append a new line if we already have some data. This way we can skip step 3.) in the EventSource's OnMessageReceived.
                    // We do only null check, because empty string can be valid payload
                    if (CurrentMessage.Data != null)
                        CurrentMessage.Data += Environment.NewLine;

                    CurrentMessage.Data += value;
                    break;

                // If the field name is "retry" => If the field value consists of only ASCII digits, then interpret the field value as an integer in base ten,
                //  and set the event stream's reconnection time to that integer. Otherwise, ignore the field.
                case "retry":
                    int result;
                    if (int.TryParse(value, out result))
                        CurrentMessage.Retry = TimeSpan.FromMilliseconds(result);
                    break;

                // Otherwise: The field is ignored.
                default:
                    break;
            }

            return false;
        }

#endregion
#endif
#endregion

#region EventStreamResponse Event Handlers

        private void OnMessageReceived(Best.ServerSentEvents.Message message)
        {
            if (this.State >= States.Closing)
                return;

            // 1.) Set the last event ID string of the event source to value of the last event ID buffer.
            // The buffer does not get reset, so the last event ID string of the event source remains set to this value until the next time it is set by the server.
            // We check here only for null, because it can be a non-null but empty string.
            if (message.Id != null)
                this.LastEventId = message.Id;

            if (message.Retry.TotalMilliseconds > 0)
                this.ReconnectionTime = message.Retry;

            // 2.) If the data buffer is an empty string, set the data buffer and the event type buffer to the empty string and abort these steps.
            if (string.IsNullOrEmpty(message.Data))
                return;

            // 3.) If the data buffer's last character is a U+000A LINE FEED (LF) character, then remove the last character from the data buffer.
            // This step can be ignored. We constructed the string to be able to skip this step.

            if (OnMessage != null && !message.IsComment)
            {
                try
                {
                    OnMessage(this, message);
                }
                catch (Exception ex)
                {
                    HTTPManager.Logger.Exception("EventSource", "OnMessageReceived - OnMessage", ex, this.LoggingContext);
                }
            }
#if !UNITY_WEBGL || UNITY_EDITOR
            else if (message.IsComment && this.OnComment != null)
            {
                try
                {
                    this.OnComment(this, message.Data);
                }
                catch (Exception ex)
                {
                    HTTPManager.Logger.Exception("EventSource", "OnMessageReceived - OnComment", ex, this.LoggingContext);
                }
            }
#endif

            if (EventTable != null && !string.IsNullOrEmpty(message.Event))
            {
                OnEventDelegate action;
                if (EventTable.TryGetValue(message.Event, out action))
                {
                    if (action != null)
                    {
                        try
                        {
                            action(this, message);
                        }
                        catch(Exception ex)
                        {
                            HTTPManager.Logger.Exception("EventSource", "OnMessageReceived - action", ex, this.LoggingContext);
                        }
                    }
                }
            }
        }

        public void HandleEvents()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            if (this.State == States.Open)
            {
                Best.ServerSentEvents.Message message;
                while (this.CompletedMessages.TryDequeue(out message))
                    OnMessageReceived(message);
            }
#endif
        }

        public void CancellationRequested()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            if (this.InternalRequest != null)
                this.InternalRequest.Abort();
#else
            Close();
#endif
        }

        public void Dispose()
        {
            
        }

#endregion

#region IHeartbeat Implementation
#if !UNITY_WEBGL || UNITY_EDITOR

        void IHeartbeat.OnHeartbeatUpdate(DateTime now, TimeSpan dif)
        {
            if (this.State != States.Retrying)
            {
                HTTPManager.Heartbeats.Unsubscribe(this);

                return;
            }

            if (now - RetryCalled >= ReconnectionTime)
            {
                CreateInternalRequest();

                Open();

                if (this.State != States.Connecting)
                    SetClosed("OnHeartbeatUpdate");

                HTTPManager.Heartbeats.Unsubscribe(this);
            }
        }
#endif
        #endregion

        #region WebGL Static Callbacks
#if UNITY_WEBGL && !UNITY_EDITOR

        [AOT.MonoPInvokeCallback(typeof(OnWebGLEventSourceOpenDelegate))]
        static void OnOpenCallback(uint id)
        {
            EventSource es;
            if (EventSources.TryGetValue(id, out es))
            {
                if (es.OnOpen != null)
                {
                    try
                    {
                        es.OnOpen(es);
                    }
                    catch(Exception ex)
                    {
                        HTTPManager.Logger.Exception("EventSource", "OnOpen", ex, es.LoggingContext);
                    }
                }

                es.State = States.Open;
            }
            else
                HTTPManager.Logger.Warning("EventSource", "OnOpenCallback - No EventSource found for id: " + id.ToString());
        }

        [AOT.MonoPInvokeCallback(typeof(OnWebGLEventSourceMessageDelegate))]
        static void OnMessageCallback(uint id, string eventStr, string data, string eventId, int retry)
        {
            EventSource es;
            if (EventSources.TryGetValue(id, out es))
            {
                var msg = new Best.ServerSentEvents.Message();
                msg.Id = eventId;
                msg.Data = data;
                msg.Event = eventStr;
                msg.Retry = TimeSpan.FromSeconds(retry);

                es.OnMessageReceived(msg);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(OnWebGLEventSourceErrorDelegate))]
        static void OnErrorCallback(uint id, string reason)
        {
            EventSource es;
            if (EventSources.TryGetValue(id, out es))
            {
                es.CallOnError(reason, "OnErrorCallback");
                es.SetClosed("OnError");

                EventSources.Remove(id);
            }

            try
            {
                ES_Release(id);
            }
            catch (Exception ex)
            {
                HTTPManager.Logger.Exception("EventSource", "ES_Release", ex);
            }
        }

#endif
        #endregion

        #region WebGL Interface
#if UNITY_WEBGL && !UNITY_EDITOR

        [DllImport("__Internal")]
        static extern bool ES_IsSupported();

        [DllImport("__Internal")]
        static extern uint ES_Create(string url, bool withCred, OnWebGLEventSourceOpenDelegate onOpen, OnWebGLEventSourceMessageDelegate onMessage, OnWebGLEventSourceErrorDelegate onError);

        [DllImport("__Internal")]
        static extern void ES_AddEventHandler(uint id, string eventName);

        [DllImport("__Internal")]
        static extern void ES_Close(uint id);

        [DllImport("__Internal")]
        static extern void ES_Release(uint id);

#endif
        #endregion

    }
}
