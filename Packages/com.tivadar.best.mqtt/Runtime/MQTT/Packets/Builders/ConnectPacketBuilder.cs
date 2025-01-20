using Best.MQTT.Packets.Utils;

using System;

namespace Best.MQTT.Packets.Builders
{
    /// <summary>
    /// TODO: 
    /// </summary>
    public struct LastWillBuilder
    {
        private Data _topic;
        private Data _payload;
        private QoSLevels _qos;
        private bool _retain;
        private Properties _properties;

        internal void BuildFlags(ref BitField bitField)
        {
            if (this._retain)
                bitField[5] = true;

            // https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901041
            switch (this._qos)
            {
                case QoSLevels.AtMostOnceDelivery: break;
                case QoSLevels.AtLeastOnceDelivery: bitField[3] = true; break;
                case QoSLevels.ExactlyOnceDelivery: bitField[4] = true; break;
            }
        }

        internal void Build(ref Packet packet)
        {

            // Will Properties
            packet.AddPayload(Data.FromProperties(this._properties));

            // Will Topic
            if (this._topic.IsSet)
                packet.AddPayload(this._topic);
            else
                throw new ArgumentException("Will Topic required!");

            // Will Payload
            if (this._payload.IsSet)
                packet.AddPayload(this._payload);
            else
                throw new ArgumentException("Will Payload required!");
        }

        /// <summary>
        /// Set the topic the last-will will be published.
        /// </summary>
        public LastWillBuilder WithTopic(string topic)
        {
            this._topic = Data.FromString(topic);
            return this;
        }

        /// <summary>
        /// Binary payload of the last-will.
        /// </summary>
        public LastWillBuilder WithPayload(byte[] binary)
        {
            this._payload = Data.FromArray(binary);
            return this;
        }

        /// <summary>
        /// Textual payload of the last-will. It also sets the Payload Format Indicator to UTF8.
        /// </summary>
        public LastWillBuilder WithPayload(string payload)
        {
            return this.WithPayload(System.Text.Encoding.UTF8.GetBytes(payload))
                       /*.WithPayloadFormatIndicator(PayloadTypes.UTF8)*/;
        }

        /// <summary>
        /// QoS level of the last-will.
        /// </summary>
        public LastWillBuilder WithQoS(QoSLevels qos)
        {
            this._qos = qos;
            return this;
        }

        /// <summary>
        /// Retain flag.
        /// </summary>
        public LastWillBuilder WithRetain(bool retain = true)
        {
            this._retain = retain;
            return this;
        }

        /// <summary>
        /// Delay before the broker will publish the last-will
        /// </summary>
        public LastWillBuilder WithDelayInterval(UInt32 seconds)
        {
            this._properties.ThrowIfPresent(PacketProperties.WillDelayInterval);

            this._properties.AddProperty(new Property() { Type = PacketProperties.WillDelayInterval, Data = Data.FromFourByteInteger(seconds) });
            return this;
        }

        /// <summary>
        /// Type of the payload, binary or textual.
        /// </summary>
        public LastWillBuilder WithPayloadFormatIndicator(PayloadTypes payloadType)
        {
            this._properties.ThrowIfPresent(PacketProperties.PayloadFormatIndicator);

            this._properties.AddProperty(new Property { Type = PacketProperties.PayloadFormatIndicator, Data = Data.FromByte((byte)payloadType) });
            return this;
        }

        public LastWillBuilder WithMessageExpiryInterval(UInt32 seconds)
        {
            this._properties.ThrowIfPresent(PacketProperties.MessageExpiryInterval);

            this._properties.AddProperty(new Property { Type = PacketProperties.MessageExpiryInterval, Data = Data.FromFourByteInteger(seconds) });
            return this;
        }

        public LastWillBuilder WithContentType(string contentType)
        {
            this._properties.ThrowIfPresent(PacketProperties.ContentType);

            this._properties.AddProperty(new Property { Type = PacketProperties.ContentType, Data = Data.FromString(contentType) });

            return this;
        }

        public LastWillBuilder WithResponseTopic(string topic)
        {
            this._properties.ThrowIfPresent(PacketProperties.ResponseTopic);

            this._properties.AddProperty(new Property { Type = PacketProperties.ResponseTopic, Data = Data.FromString(topic) });

            return this;
        }

        public LastWillBuilder WithCorrelationData(byte[] binary)
        {
            this._properties.ThrowIfPresent(PacketProperties.ResponseTopic);

            this._properties.AddProperty(new Property { Type = PacketProperties.CorrelationData, Data = Data.FromArray(binary) });

            return this;
        }

        public LastWillBuilder WithUserData(string key, string value)
        {
            this._properties.AddProperty(new Property { Type = PacketProperties.UserProperty, Data = Data.FromStringPair(key, value) });
            return this;
        }
    }

    /// <summary>
    /// TODO: add detailed description
    /// </summary>
    /// <example>
    /// In this example the `ConnectPacketBuilderCallback` returns with the builder received as its second parameter without modifying it. 
    /// In the callback a new `ConnectPacketBuilder` can be created, but it's easier just to use the one already passed in the parameter.
    /// <code>
    /// var options = new ConnectionOptionsBuilder()
    ///         .WithTCP("test.mosquitto.org", 1883)
    ///         .Build();
    ///     client = new MQTTClient(options);
    ///     client.BeginConnect(ConnectPacketBuilderCallback);
    /// 
    /// ConnectPacketBuilder ConnectPacketBuilderCallback(MQTTClient client, ConnectPacketBuilder builder)
    /// {
    ///     return builder;
    /// }
    /// </code>
    /// </example>
    /// <example>
    /// Add UserName and Password
    /// <code>
    /// ConnectPacketBuilder ConnectPacketBuilderCallback(MQTTClient client, ConnectPacketBuilder builder)
    /// {
    ///     return builder.WithUserNameAndPassword("username", "password");
    /// }
    /// </code>
    /// </example>
    public struct ConnectPacketBuilder
    {
        private MQTTClient _client;
        
        // Variable Headers
        private BitField _connectFlags;
        private UInt16 _keepAlive;
        private LastWillBuilder _lastWillBuilder;

        // Properties
        private ConnectPropertyBuilder _connectPropertyBuilder;

        // Payload
        private string _clientId;
        private Session _session;
        private string _userName;
        private string _password;

        internal ConnectPacketBuilder(MQTTClient client)
        {
            this._client = client;

            this._connectFlags = new BitField(0);
            this._keepAlive = 60;
            this._lastWillBuilder = default(LastWillBuilder);

            this._connectPropertyBuilder = default(ConnectPropertyBuilder);

            this._clientId = null;
            this._session = null;
            this._userName = null;
            this._password = null;
        }

        /// <summary>
        /// This specifies whether the connection starts a new session or is a continuation of an existing session.
        /// When `WithCleanStart` is used, both the broker and client deletes theirs previously stored session data. 
        /// The client continues to use its client id.
        /// </summary>
        public ConnectPacketBuilder WithCleanStart()
        {
            this._connectFlags.Set(1, true);
            return this;
        }

        /// <summary>
        /// Maximum seconds that can be pass between sending two packets to the broker. If no other packets are sent, the plugin will send ping requests to check and keep the connection alive.
        /// <see href="https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901045"/>
        /// </summary>
        public ConnectPacketBuilder WithKeepAlive(ushort seconds)
        {
            this._keepAlive = seconds;
            return this;
        }

        /// <summary>
        /// A last will can be added to the connection. 
        /// The will message will be published by the broker after the network connection is subsequently closed and either the Will Delay Interval has elapsed or the session ends, 
        /// unless the will message has been deleted by the broker on receipt of a DISCONNECT packet with Reason Code `NormalDisconnection` or a new network connection for the ClientID is opened before the Will Delay Interval has elapsed.
        /// </summary>
        /// <remarks>
        /// Situations in which the will message is published include, but are not limited to:
        /// <list type="bullet">
        ///     <item><description>An I/O error or network failure detected by the broker.</description></item>
        ///     <item><description>The client fails to communicate within the Keep Alive time.</description></item>
        ///     <item><description>The client closes the network connection without first sending a DISCONNECT packet with a Reason Code `NormalDisconnection`.</description></item>
        ///     <item><description>The broker closes the network connection without first receiving a DISCONNECT packet with a Reason Code `NormalDisconnection`.</description></item>
        /// </list>
        /// </remarks>
        /// <param name="lastWillBuilder"></param>
        /// <returns></returns>
        public ConnectPacketBuilder WithLastWill(LastWillBuilder lastWillBuilder)
        {
            this._lastWillBuilder = lastWillBuilder;
            this._connectFlags[2] = true;
            return this;
        }

        /// <summary>
        /// With this call the plugin's automatic client id generation can be overwritten. If not exists the client creates a session to store its state. If a session is available for this clientId, it loads and uses it.
        /// </summary>
        /// <remarks>When neither the `WithClientID` or `WithSession` are used, first time connecting to the broker the plugin generates a unique id and will use it for consecutive connections.</remarks>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public ConnectPacketBuilder WithClientID(string clientId)
        {
            this._clientId = clientId;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>When neither the `WithClientID` or `WithSession` are used, first time connecting to the broker the plugin generates a unique id and will use it for consecutive connections.</remarks>
        /// <param name="session"></param>
        /// <returns></returns>
        public ConnectPacketBuilder WithSession(Session session)
        {
            this._session = session;
            return this;
        }

        /// <summary>
        /// Add a user name for authentication purposes.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ConnectPacketBuilder WithUserName(string userName)
        {
            this._userName = userName;
            this._connectFlags.Set(7, true);
            return this;
        }

        /// <summary>
        /// Add a password for authentication purposes.
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public ConnectPacketBuilder WithPassword(string password)
        {
            this._password = password;
            this._connectFlags.Set(6, true);
            return this;
        }

        /// <summary>
        /// Add both user name and password for authentication purposes.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public ConnectPacketBuilder WithUserNameAndPassword(string userName, string password)
        {
            this.WithUserName(userName);
            this.WithPassword(password);

            return this;
        }

        /// <summary>
        /// When the Session expires the client and broker need not process the deletion of state atomically.
        /// If the Session Expiry Interval is absent the value <c>0</c> is used. 
        /// If it is set to <c>0</c>, or is absent, the session ends when the network connection is closed. If the *Session Expiry Interval* is <c>0xFFFFFFFF</c> (<c>uint.MaxValue</c>), the session does not expire.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item><description>A client that only wants to process messages while connected will call <c>WithCleanStart</c> and set the *Session Expiry Interval* to <c>0</c>. It will not receive Application Messages published before it connected and has to subscribe afresh to any topics that it is interested in each time it connects.</description></item>
        /// <item><description>A client might be connecting to a broker using a network that provides intermittent connectivity. This client can use a short *Session Expiry Interval* so that it can reconnect when the network is available again and continue reliable message delivery. If the client does not reconnect, allowing the session to expire, then Application Messages will be lost.</description></item>
        /// <item><description>When a client connects with a long *Session Expiry Interval*, it is requesting that the broker maintain its MQTT session state after it disconnects for an extended period. Clients should only connect with a long *Session Expiry Interval* if they intend to reconnect to the broker at some later point in time. When a client has determined that it has no further use for the session it should disconnect with a *Session Expiry Interval* set to <c>0</c>.</description></item>
        /// </list>
        /// </remarks>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public ConnectPacketBuilder WithSessionExpiryInterval(UInt32 seconds)
        {
            ExceptionHelper.ThrowIfV311(this._client.Options.ProtocolVersion, $"{nameof(WithSessionExpiryInterval)} is available with MQTT v5.0 or newer.");

            this._connectPropertyBuilder.WithSessionExpiryInterval(seconds);
            return this;
        }

        /// <summary>
        /// The client uses this value to limit the number of <c>QoS 1</c> and <c>QoS 2</c> publications that it is willing to process concurrently. 
        /// There is no mechanism to limit the <c>QoS 0</c> publications that the broker might try to send.
        /// The value of Receive Maximum applies only to the current Network Connection. 
        /// If the Receive Maximum value is absent then its value defaults to <c>65,535</c>.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ConnectPacketBuilder WithReceiveMaximum(UInt16 value)
        {
            ExceptionHelper.ThrowIfV311(this._client.Options.ProtocolVersion, $"{nameof(WithReceiveMaximum)} is available with MQTT v5.0 or newer.");

            this._connectPropertyBuilder.WithReceiveMaximum(value);
            return this;
        }

        /// <summary>
        ///  The maximum packet size the client is willing to accept.
        ///  If the maximum packet size is not present, no limit on the packet size is imposed beyond the limitations in the protocol as a result of the remaining length encoding and the protocol header sizes.
        /// </summary>
        /// <param name="maximumPacketSize"></param>
        /// <returns></returns>
        public ConnectPacketBuilder WithMaximumPacketSize(UInt32 maximumPacketSize)
        {
            ExceptionHelper.ThrowIfV311(this._client.Options.ProtocolVersion, $"{nameof(WithMaximumPacketSize)} is available with MQTT v5.0 or newer.");

            this._connectPropertyBuilder.WithMaximumPacketSize(maximumPacketSize);
            return this;
        }

        /// <summary>
        /// This value indicates the highest value that the client will accept as a topic alias sent by the broker. The client uses this value to limit the number of topic aliases that it is willing to hold on this connection. If topic alias maximum is absent or zero, the broker will not send any topic aliases to the client.
        /// </summary>
        /// <remarks>If not called, the plugin will use `ushort.MaxValue`(65535). To disable receiving topic aliases from the broker call it with 0.</remarks>
        /// <param name="maximum"></param>
        /// <returns></returns>
        public ConnectPacketBuilder WithTopicAliasMaximum(UInt16 maximum)
        {
            ExceptionHelper.ThrowIfV311(this._client.Options.ProtocolVersion, $"{nameof(WithTopicAliasMaximum)} is available with MQTT v5.0 or newer.");

            this._connectPropertyBuilder.WithTopicAliasMaximum(maximum);
            return this;
        }

        /// <summary>
        /// When called with `true` the client request the broker to return Response Information in the ServerConnectAckMessage. 
        /// </summary>
        /// <remarks>The broker can choose not to include <c>Response Information</c> in the connect ack message, even if the client requested it!</remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        public ConnectPacketBuilder WithRequestResponseInformation(bool request)
        {
            ExceptionHelper.ThrowIfV311(this._client.Options.ProtocolVersion, $"{nameof(WithRequestResponseInformation)} is available with MQTT v5.0 or newer.");

            this._connectPropertyBuilder.WithRequestResponseInformation(request);
            return this;
        }

        /// <summary>
        /// The client can use this function to indicate whether the ReasonString or UserProperties are sent in the case of failures. 
        /// If the value of request problem information is `false`, the broker may return a `ReasonString` or `UserProperties` on a *connect acknowledgement* or* disconnect* packet,
        /// but must not send a `ReasonString` or `UserProperties` on any packet other than* publish*, *connect acknowledgement* or* disconnect*. 
        /// If this value is `true`, the broker may return a `ReasonString` or `UserProperties` on any packet where it is allowed.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ConnectPacketBuilder WithRequestProblemInformation(bool request)
        {
            ExceptionHelper.ThrowIfV311(this._client.Options.ProtocolVersion, $"{nameof(WithRequestProblemInformation)} is available with MQTT v5.0 or newer.");

            this._connectPropertyBuilder.WithRequestProblemInformation(request);
            return this;
        }

        /// <summary>
        /// User Properties on the connect packet can be used to send connection related properties from the client to the broker. 
        /// The meaning of these properties is not defined by this specification.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public ConnectPacketBuilder WithUserProperty(string key, string value)
        {
            ExceptionHelper.ThrowIfV311(this._client.Options.ProtocolVersion, $"{nameof(WithUserProperty)} is available with MQTT v5.0 or newer.");

            this._connectPropertyBuilder.WithUserProperty(key, value);
            return this;
        }

        /// <summary>
        /// Set the name of the authentication method used for extended authentication.
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public ConnectPacketBuilder WithExtendedAuthenticationMethod(string method)
        {
            ExceptionHelper.ThrowIfV311(this._client.Options.ProtocolVersion, $"{nameof(WithExtendedAuthenticationMethod)} is available with MQTT v5.0 or newer.");

            this._connectPropertyBuilder.WithExtendedAuthenticationMethod(method);
            return this;
        }

        /// <summary>
        /// Set the binary data containing authentication data for extended authentication. 
        /// The contents of this data are defined by the authentication method.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ConnectPacketBuilder WithExtendedAuthenticationData(byte[] data)
        {
            ExceptionHelper.ThrowIfV311(this._client.Options.ProtocolVersion, $"{nameof(WithExtendedAuthenticationData)} is available with MQTT v5.0 or newer.");

            this._connectPropertyBuilder.WithExtendedAuthenticationData(data);
            return this;
        }

        internal ConnectPacketBuilder WithProperties(ConnectPropertyBuilder builder)
        {
            this._connectPropertyBuilder = builder;
            return this;
        }

        internal (Packet packet, Session session, ushort clientKeepAlive, uint clientMaximumPacketSize, ushort clientReceiveMaximum) Build()
        {
            // will flag
            if (this._connectFlags[2])
                this._lastWillBuilder.BuildFlags(ref this._connectFlags);

            var packet = new Packet();

            packet.Type = PacketTypes.Connect;

            packet.AddVariableHeader(Data.FromString("MQTT")); // Protocol Name

            switch(this._client.Options.ProtocolVersion)
            {
                case SupportedProtocolVersions.MQTT_3_1_1: break;
                case SupportedProtocolVersions.MQTT_5_0: break;
                default:
                    throw new NotImplementedException($"Version '{this._client.Options.ProtocolVersion}' isn't supported!");
            }

            byte protocolLevel = 0;
            switch(this._client.Options.ProtocolVersion)
            {
                // Protocol Level (v3.1.1): http://docs.oasis-open.org/mqtt/mqtt/v3.1.1/os/mqtt-v3.1.1-os.html#_Toc398718030
                case SupportedProtocolVersions.MQTT_3_1_1: protocolLevel = 0x04; break;

                // Protocol Version (v5): https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901037
                case SupportedProtocolVersions.MQTT_5_0: protocolLevel = 0x05; break;

                default: throw new NotImplementedException($"Version '{this._client.Options.ProtocolVersion}' isn't supported!");
            };
            packet.AddVariableHeader(Data.FromByte(protocolLevel));

            packet.AddVariableHeader(this._connectFlags.AsData());
            packet.AddVariableHeader(Data.FromTwoByteInteger(this._keepAlive));

            var properties = this._connectPropertyBuilder.Build();
            if (this._client.Options.ProtocolVersion >= SupportedProtocolVersions.MQTT_5_0)
                packet.AddVariableHeader(Data.FromProperties(properties));

            // https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901058
            // These fields, if present, MUST appear in the order Client Identifier, Will Properties, Will Topic, Will Payload, User Name, Password

            // Client Identifier
            if (this._clientId != null)
                this._session = SessionHelper.Get(this._client.Options.Host, this._clientId);
            else if (this._session == null)
                this._session = SessionHelper.Get(this._client.Options.Host);

            if (this._session.IsNull)
            {
                // A Server MAY allow a Client to supply a ClientID that has a length of zero bytes, however if it does so the Server MUST treat this as a special case and assign a unique ClientID to that Client [MQTT-3.1.3-6].
                // It MUST then process the CONNECT packet as if the Client had provided that unique ClientID, and MUST return the Assigned Client Identifier in the CONNACK packet [MQTT-3.1.3-7].
                packet.SetPayload(Data.FromString(string.Empty));
            }
            else
                packet.SetPayload(Data.FromString(this._session.ClientId));

            // will flag
            if (this._connectFlags[2])
                this._lastWillBuilder.Build(ref packet);

            // User Name
            if (!string.IsNullOrEmpty(this._userName))
                packet.AddPayload(Data.FromString(this._userName));

            // Password
            if (!string.IsNullOrEmpty(this._password))
                packet.AddPayload(Data.FromString(this._password));

            // create & return with a tuple
            return (packet,
                this._session,
                this._keepAlive,
                properties.TryFindData(PacketProperties.MaximumPacketSize, DataTypes.FourByteInteger, out var maximumPacketSizeData) ? maximumPacketSizeData.Integer : UInt32.MaxValue,
                properties.TryFindData(PacketProperties.ReceiveMaximum, DataTypes.TwoByteInteger, out var receiveMaximumData) ? (UInt16)receiveMaximumData.Integer : UInt16.MaxValue);
        }
    }

    // https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901046
    internal struct ConnectPropertyBuilder
    {
        private Properties _properties;

        public ConnectPropertyBuilder WithSessionExpiryInterval(UInt32 seconds)
        {
            this._properties.ThrowIfPresent(PacketProperties.SessionExpiryInterval);

            this._properties.AddProperty(new Property { Type = PacketProperties.SessionExpiryInterval, Data = Data.FromFourByteInteger(seconds) });
            return this;
        }

        public ConnectPropertyBuilder WithReceiveMaximum(UInt16 value)
        {
            if (value == 0)
                throw new ArgumentException($"{nameof(value)} must be larger than 0!");

            this._properties.ThrowIfPresent(PacketProperties.ReceiveMaximum);

            this._properties.AddProperty(new Property { Type = PacketProperties.ReceiveMaximum, Data = Data.FromTwoByteInteger(value) });
            return this;
        }

        public ConnectPropertyBuilder WithMaximumPacketSize(UInt32 maximumPacketSize)
        {
            if (maximumPacketSize == 0)
                throw new ArgumentException($"{nameof(maximumPacketSize)} must be larger than zero!");

            this._properties.ThrowIfPresent(PacketProperties.MaximumPacketSize);

            this._properties.AddProperty(new Property { Type = PacketProperties.MaximumPacketSize, Data = Data.FromFourByteInteger(maximumPacketSize) });
            return this;
        }

        public ConnectPropertyBuilder WithTopicAliasMaximum(UInt16 maximum)
        {
            this._properties.ThrowIfPresent(PacketProperties.TopicAliasMaximum);

            this._properties.AddProperty(new Property { Type = PacketProperties.TopicAliasMaximum, Data = Data.FromTwoByteInteger(maximum) });
            return this;
        }

        public ConnectPropertyBuilder WithRequestResponseInformation(bool request)
        {
            this._properties.ThrowIfPresent(PacketProperties.RequestResponseInformation);

            this._properties.AddProperty(new Property { Type = PacketProperties.RequestResponseInformation, Data = Data.FromByte(request ? 1 : 0) });

            return this;
        }

        public ConnectPropertyBuilder WithRequestProblemInformation(bool request)
        {
            this._properties.ThrowIfPresent(PacketProperties.RequestProblemInformation);

            this._properties.AddProperty(new Property { Type = PacketProperties.RequestProblemInformation, Data = Data.FromByte(request ? 1 : 0) });

            return this;
        }

        public ConnectPropertyBuilder WithUserProperty(string key, string value)
        {
            this._properties.AddProperty(new Property { Type = PacketProperties.UserProperty, Data = Data.FromStringPair(key, value) });
            return this;
        }

        public ConnectPropertyBuilder WithExtendedAuthenticationMethod(string method)
        {
            this._properties.ThrowIfPresent(PacketProperties.AuthenticationMethod);

            this._properties.AddProperty(new Property { Type = PacketProperties.AuthenticationMethod, Data = Data.FromString(method) });
            return this;
        }

        public ConnectPropertyBuilder WithExtendedAuthenticationData(byte[] data)
        {
            this._properties.ThrowIfPresent(PacketProperties.AuthenticationData);

            this._properties.AddProperty(new Property { Type = PacketProperties.AuthenticationData, Data = Data.FromArray(data) });
            return this;
        }

        internal Properties Build()
        {
            if (this._properties.Find(PacketProperties.TopicAliasMaximum).Data.Type == DataTypes.UnSet)
                WithTopicAliasMaximum(ushort.MaxValue);

            return this._properties;
        }
    }
}
