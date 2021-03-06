﻿using System;
using Castle.Infrastructure;
using Castle.Messages;

namespace Castle.Config
{
    /// <summary>
    /// Castle intrinsic configuration attributes/values
    /// </summary>
    public class CastleConfiguration
    {
        /// <exception cref="ArgumentException">Thrown when <paramref name="apiSecret"/> is null or empty</exception>>
        public CastleConfiguration(string apiSecret)
        {
            ArgumentGuard.NotNullOrEmpty(apiSecret, nameof(apiSecret));

            ApiSecret = apiSecret;
            AllowList = Headers.AllowList;
        }

        /// <summary>
        /// Secret used to authenticate with the Castle Api (Required)
        /// </summary>
        public string ApiSecret { get; }

        /// <summary>
        /// The response action to return in case of a failover in an Authenticate request
        /// </summary>
        public ActionType FailOverStrategy { get; set; } = ActionType.Allow;

        /// <summary>
        /// Timeout for requests, in milliseconds
        /// </summary>
        public int Timeout { get; set; } = 1000;

        /// <summary>
        /// Base Castle Api url
        /// </summary>
        public string BaseUrl { get; set; } = "https://api.castle.io";

        /// <summary>
        /// Log level applied by the injected <see cref="ICastleLogger"/> implementation
        /// </summary>
        public LogLevel LogLevel { get; set; } = LogLevel.Error;

        /// <summary>
        /// AllowList for headers in request context object
        /// </summary>
        public string[] AllowList { get; set; } = { };

        /// <summary>
        /// DenyList for headers in request context object
        /// </summary>
        public string[] DenyList { get; set; } = { };

        /// <summary>
        /// IP Headers to look for a client IP address
        /// </summary>
        public string[] IpHeaders { get; set; }

        /// <summary>
        /// Trusted public proxies list
        /// </summary>
        public string[] TrustedProxies { get; set; } = { };

        /// <summary>
        /// Number of trusted proxies used in the chain
        /// </summary>
        public int TrustedProxyDepth { get; set; }

        /// <summary>
        /// Is trusting all of the proxy IPs in X-Forwarded-For enabled
        /// </summary>
        public bool TrustProxyChain { get; set; }

        /// <summary>
        /// If true, no requests are actually sent to the Castle Api, and Authenticate returns a failover response
        /// </summary>
        public bool DoNotTrack { get; set; } = false;

        /// <summary>
        /// Your own logger implementation, for internal SDK logging
        /// </summary>
        public ICastleLogger Logger { get; set; }

        /// <summary>
        /// Configuration access from within the SDK
        /// </summary>
        internal static CastleConfiguration Configuration { get; private set; }

        internal static void SetConfiguration(CastleConfiguration configuration)
        {
            Configuration = configuration;
        }
    }
}
