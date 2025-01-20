using System;

namespace Best.ServerSentEvents
{
    /// <summary>
    /// Represents a single Server-Sent Event message as specified by the W3C SSE specification.
    /// This encapsulates individual data sent over an SSE connection, providing event details, payload data, and related metadata.
    /// Each message can represent actual data or comments from the server.
    /// </summary>
    [Best.HTTP.Shared.PlatformSupport.IL2CPP.Preserve]
    public sealed class Message
    {
        /// <summary>
        /// Represents the unique identifier for the event message, utilized to ensure message continuity in case of connection disruptions.
        /// </summary>
        /// <example>If the server sends a message with an "id" of "1", and subsequently the connection is interrupted, 
        /// the client will send a `Last-Event-ID` header with the value "1" upon reconnection, signaling the server from where to resume.
        /// If an "id" field is received with no value, it resets the last event ID to an empty string, implying that no `Last-Event-ID` header will be sent during the next reconnection attempt.
        /// A <c>null</c> Id indicates that the server didn't provide an identifier for that particular message.
        /// </example>
        [Best.HTTP.Shared.PlatformSupport.IL2CPP.Preserve]
        public string Id { get; internal set; }

        /// <summary>
        /// Name of the event, or an empty string.
        /// </summary>
        [Best.HTTP.Shared.PlatformSupport.IL2CPP.Preserve]
        public string Event { get; internal set; }

        /// <summary>
        /// The actual payload of the message.
        /// </summary>
        [Best.HTTP.Shared.PlatformSupport.IL2CPP.Preserve]
        public string Data { get; internal set; }

        /// <summary>
        /// A reconnection time, in milliseconds. This must initially be a user-agent-defined value, probably in the region of a few seconds.
        /// </summary>
        [Best.HTTP.Shared.PlatformSupport.IL2CPP.Preserve]
        public TimeSpan Retry { get; internal set; }

        /// <summary>
        /// If this is true, the Data property holds the comment sent by the server.
        /// </summary>
        [Best.HTTP.Shared.PlatformSupport.IL2CPP.Preserve]
        internal bool IsComment { get; set; }

        public override string ToString()
        {
            return string.Format("\"{0}\": \"{1}\"", Event, Data);
        }
    }
}
