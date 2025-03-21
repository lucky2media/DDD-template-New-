using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Best.HTTP.HostSetting;
using Best.HTTP.Shared.PlatformSupport.Threading;

namespace Best.HTTP.Hosts.Settings
{
    /**
     * Host Settings Hierarchy for the following hosts, settings are stored as leafs:
     * 
     * *.com
     * *.example.com
     * example.com
     * 
     * '*' matches one or more subdomains so *.example.com 
     *  - matches a.example.com and a.b.example.com
     *  - but doesn't match example.com!
     * 
     *                              
     *                               
     *                    [com]                 [localhost]                [org]                      [*]
     *               +------+------+                 |                       |                         |
     *               |             |              [setting]                 [*]                     [setting]
     *         [example]          [*]                                        |
     *         /       \           |                                      [setting]
     *       [b]     [setting]  [setting]
     *        |
     *       [a]
     *        |
     *     [setting]
     * */

    /// <summary>
    /// Manages host-specific settings for HTTP requests based on hostnames.
    /// The HostSettingsManager is a powerful tool for fine-tuning HTTP request and connection behaviors
    /// on a per-host basis. It enables you to define custom settings for specific hostnames 
    /// while maintaining default settings for all other hosts. This level of granularity allows you to
    /// optimize and customize HTTP requests for different endpoints within your application.
    /// </summary>
    /// <remarks>
    /// When host-specific settings are not found for a given host variant, the default <see cref="HostSettings"/>
    /// associated with the "*" host will be returned.
    /// </remarks>
    public sealed class HostSettingsManager
    {
        private const char SPLITTER = '.';

        private SortedList<string, Node> _rootNodes = new SortedList<string, Node>(AsteriskStringComparer.Instance);
        private List<string> _segments = new List<string>(4);

        private ConcurrentDictionary<string, HostSettings> _cache = new ConcurrentDictionary<string, HostSettings>();

        /// <summary>
        /// Initializes a new instance of the <see cref="HostSettingsManager"/> class with default settings for all hosts ("*").
        /// </summary>
        public HostSettingsManager() => Add("*", new HostSettings());

        /// <summary>
        /// Adds default settings for the host part of the specified URI. This is equivalent to calling <see cref="Add(Uri, HostSettings)"/> with the a new <see cref="HostSettings"/>.
        /// </summary>
        /// <param name="uri">The URI for which default settings should be applied. Only the host part of the URI will be used.</param>
        /// <returns>A <see cref="HostSettings"/> instance with default values.</returns>
        public HostSettings AddDefault(Uri uri) => Add(uri, new HostSettings());

        /// <summary>
        /// Adds default settings for the the specified host name. This is equivalent to calling <see cref="Add(string, HostSettings)"/> with the a new <see cref="HostSettings"/>.
        /// </summary>
        /// <param name="hostname">The hostname for which default settings should be applied.</param>
        /// <returns>A <see cref="HostSettings"/> instance with default values.</returns>
        public HostSettings AddDefault(string hostname) => Add(hostname, new HostSettings());

        /// <summary>
        /// Adds host-specific settings for the host part of the specified URI.
        /// </summary>
        /// <param name="uri">The URI for which settings should be applied. Only the host part of the URI will be used.</param>
        /// <param name="settings">The <see cref="HostSettings"/> to apply.</param>
        public HostSettings Add(Uri uri, HostSettings settings) => Add(uri.Host, settings);

        /// <summary>
        /// Adds host-specific settings for the specified hostname.
        /// </summary>
        /// <param name="hostname">The hostname for which settings should be applied.</param>
        /// <param name="settings">The <see cref="HostSettings"/> to apply.</param>
        /// <exception cref="ArgumentNullException">Thrown when either the hostname or settings is null.</exception>
        /// <exception cref="FormatException">Thrown when the hostname contains more than one asterisk ('*').</exception>
        public HostSettings Add(string hostname, HostSettings settings)
        {
            if (string.IsNullOrEmpty(hostname))
                throw new ArgumentNullException(nameof(hostname));

            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            if (hostname.IndexOf('*') != hostname.LastIndexOf('*'))
                throw new FormatException($"{nameof(hostname)} (\"{hostname}\") MUST contain only one '*'!");

            // From "a.b.example.com" create a list: [ "com", "example", "b", "a"]
            lock(this._segments)
            {
                this._segments.Clear();
                this._segments.AddRange(hostname.Split(SPLITTER, StringSplitOptions.RemoveEmptyEntries));
                this._segments.Reverse();

                string subKey = this._segments[0];
                this._segments.RemoveAt(0);

            if (!_rootNodes.TryGetValue(subKey, out var node))
                _rootNodes.Add(subKey, node = new Node(subKey, null));

                node.Add(this._segments, settings);

                this._cache.Clear();
            }

            return settings;
        }

        /// <summary>
        /// Gets <see cref="HostSettings"/> for the host part of the specified <see cref="HostVariant"/>. Returns the default settings associated with "*" when not found.
        /// </summary>
        /// <param name="variant">The <see cref="HostVariant"/> for which settings should be retrieved. Only the host part of the variant will be used.</param>
        /// <returns>The host settings for the specified host variant or the default settings for "*" if not found.</returns>
        public HostSettings Get(HostVariant variant, bool fallbackToWildcard = true) => Get(variant.Host, fallbackToWildcard);

        /// <summary>
        /// Gets <see cref="HostSettings"/> for the host part of the specified <see cref="HostKey"/>. Returns the default settings associated with "*" when not found.
        /// </summary>
        /// <param name="hostKey">The <see cref="HostKey"/> for which settings should be retrieved. Only the host part of the host key will be used.</param>
        /// <returns>The host settings for the specified host key or the default settings for "*" if not found.</returns>
        public HostSettings Get(HostKey hostKey, bool fallbackToWildcard = true) => Get(hostKey.Host, fallbackToWildcard);

        /// <summary>
        /// Gets <see cref="HostSettings"/> for the host part of the specified <see cref="Uri"/>. Returns the default settings associated with "*" when not found.
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> for which settings should be retrieved. Only the host part of the URI will be used.</param>
        /// <returns>The host settings for the specified URI or the default settings for "*" if not found.</returns>
        public HostSettings Get(Uri uri, bool fallbackToWildcard = true) => Get(uri.Host, fallbackToWildcard);

        /// <summary>
        /// Gets <see cref="HostSettings"/> for the host part of the specified hostname. Returns the default settings associated with "*" when not found.
        /// </summary>
        /// <param name="hostname">The hostname for which settings should be retrieved. Only the host part of the hostname will be used.</param>
        /// <returns>The host settings for the specified hostname or the default settings for "*" if not found.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the hostname is null.</exception>
        public HostSettings Get(string hostname, bool fallbackToWildcard = true)
        {
            if (string.IsNullOrEmpty(hostname))
                throw new ArgumentNullException(nameof(hostname));

            HostSettings foundSettings = null;

            if (this._cache.TryGetValue(hostname, out foundSettings))
                return foundSettings;

            lock (this._segments)
            {
	            // This splits the hostname (a.b.c.tld) into segments (["a", "b", "c", "tld"]), reverse it (["tld", "c", "b", "a"])
	            //  and creates a final List<string> object.
	                this._segments.Clear();
	                this._segments.AddRange(hostname.Split(SPLITTER, StringSplitOptions.RemoveEmptyEntries));
	                this._segments.Reverse();
	
	                string subKey = this._segments[0];
	                this._segments.RemoveAt(0);
	
	            if (_rootNodes.TryGetValue(subKey, out var node))
	                    foundSettings = node.Find(this._segments);
            }

            if (fallbackToWildcard && foundSettings == null && _rootNodes.TryGetValue("*", out var asteriskNode))
                foundSettings = asteriskNode.hostSettings;

            if (foundSettings != null)
                this._cache.AddOrUpdate(hostname, foundSettings, (key, settings) => foundSettings);

            return foundSettings;
        }


        /// <summary>
        /// Clears all host-specific settings and resetting the default ("*") with default values.
        /// </summary>
        public void Clear()
        {
            _rootNodes.Clear();
            _cache.Clear();
            Add("*", new HostSettings());
        }
    }
}
