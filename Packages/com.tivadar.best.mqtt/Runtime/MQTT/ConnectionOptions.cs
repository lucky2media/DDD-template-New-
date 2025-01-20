using System;

namespace Best.MQTT
{
    /// <summary>
    /// Supported transports that can be used to connect with.
    /// </summary>
    public enum SupportedTransports
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        /// <summary>
        /// Transport over raw TCP.
        /// </summary>
        /// <remarks>
        /// NOT available under WebGL.
        /// </remarks>
        TCP = 0,
#endif

        /// <summary>
        /// Transport using WebSocket connections.
        /// </summary>
        /// <remarks>
        /// Available on all supported platforms!
        /// </remarks>
        WebSocket = 1
    }

    public enum SupportedProtocolVersions
    {
        MQTT_3_1_1,
        MQTT_5_0
    }

    /// <summary>
    /// Connection related options to pass to the MQTTClient.
    /// </summary>
    /// <example>
    /// Simple example on how to create a connection:
    /// <code>
    /// ConnectionOptions options = new ConnectionOptions
    /// {
    ///     Host = "localhost",
    ///     Port = 1883,
    ///     ProtocolVersion = SupportedProtocolVersions.MQTT_3_1_1
    /// };
    ///
    /// var client = new MQTTClient(options);
    /// </code>
    /// </example>
    public sealed class ConnectionOptions
    {
        /// <summary>
        /// Host name or IP address of the broker.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Port number where the broker is listening on.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Whether to use a secure protocol (TLS over TCP or wss://).
        /// </summary>
        public bool UseTLS { get; set; }

        /// <summary>
        /// Selected transport to connect with.
        /// </summary>
        public SupportedTransports Transport { get; set; }

        /// <summary>
        /// Optional path for websocket, its default is "/mqtt".
        /// </summary>
        public string Path { get; set; } = "/mqtt";

        /// <summary>
        /// The protocol version that the plugin has to use to connect with to the server.
        /// </summary>
        public SupportedProtocolVersions ProtocolVersion { get; set; } = SupportedProtocolVersions.MQTT_5_0;
    }

    /// <summary>
    /// Builder class to help creating <see cref="ConnectionOptions"/> instances.
    /// </summary>
    /// <example>
    /// The following example creates a <see cref="ConnectionOptions"/> to connect to localhost on port 1883 using the TCP transport and MQTT protocol version v3.1.1.
    /// <code>
    /// var options = new ConnectionOptionsBuilder()
    ///         .WithTCP("localhost", 1883)
    ///         .WithProtocolVersion(SupportedProtocolVersions.MQTT_3_1_1)
    ///         .Build();
    /// 
    ///     var client = new MQTTClient(options);
    /// </code>
    /// </example>
    /// 
    /// <example>
    /// This is the same as the previous example, but the builder creates the <see cref="MQTTClient"/> too.
    /// <code>
    /// var client = new ConnectionOptionsBuilder()
    ///         .WithTCP("localhost", 1883)
    ///         .WithProtocolVersion(SupportedProtocolVersions.MQTT_3_1_1)
    ///         .CreateClient();
    /// </code>
    /// </example>
    public sealed class ConnectionOptionsBuilder
    {
        private ConnectionOptions Options { get; set; } = new ConnectionOptions();

#if !UNITY_WEBGL || UNITY_EDITOR
        /// <summary>
        /// Add options for a TCP connection.
        /// </summary>
        public ConnectionOptionsBuilder WithTCP(string host, int port)
        {
            this.Options.Host = host;
            this.Options.Port = port;
            this.Options.Transport = SupportedTransports.TCP;

            return this;
        }
#endif

        /// <summary>
        /// Add options for a WebSocket connection.
        /// </summary>
        public ConnectionOptionsBuilder WithWebSocket(string host, int port)
        {
            this.Options.Host = host;
            this.Options.Port = port;
            this.Options.Transport = SupportedTransports.WebSocket;

            return this;
        }

        /// <summary>
        /// When used MQTTClient going to use TLS to secure the communication.
        /// </summary>
        public ConnectionOptionsBuilder WithTLS()
        {
            this.Options.UseTLS = true;

            return this;
        }

        /// <summary>
        /// Used by the WebSocket transport to connect to the given path.
        /// </summary>
        public ConnectionOptionsBuilder WithPath(string path)
        {
            this.Options.Path = path;
            return this;
        }

        /// <summary>
        /// The protocol version that the plugin has to use to connect with to the server.
        /// </summary>
        public ConnectionOptionsBuilder WithProtocolVersion(SupportedProtocolVersions version)
        {
            this.Options.ProtocolVersion = version;
            return this;
        }

        /// <summary>
        /// Creates an MQTTClient object with the already set options.
        /// </summary>
        public MQTTClient CreateClient() => new MQTTClient(this.Build());

        /// <summary>
        /// Creates the final ConnectionOptions instance.
        /// </summary>
        public ConnectionOptions Build() => this.Options;
    }
}
