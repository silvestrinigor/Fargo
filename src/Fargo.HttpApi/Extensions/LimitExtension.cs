using Fargo.Application.Commom;

namespace Fargo.HttpApi.Extensions
{
    /// <summary>
    /// Provides helper extensions for the <see cref="Limit"/> value object.
    /// </summary>
    /// <remarks>
    /// This extension centralizes HTTP/API specific behaviors related to pagination limits.
    /// In particular, it defines the default limit used when a client does not specify one.
    /// </remarks>
    public static class LimitExtension
    {
        /// <summary>
        /// The default limit value used by the HTTP API when no limit is provided by the client.
        /// </summary>
        public const int DefaultLimitValue = 20;

        extension(Limit limit)
        {
            /// <summary>
            /// Creates a <see cref="Limit"/> instance using the API default limit value.
            /// </summary>
            /// <returns>
            /// A <see cref="Limit"/> initialized with <see cref="DefaultLimitValue"/>.
            /// </returns>
            /// <remarks>
            /// This helper is intended for HTTP/API scenarios where pagination parameters
            /// are optional and a safe default must be applied when the client does not
            /// explicitly specify a limit.
            /// </remarks>
            public static Limit DefaultLimit()
            {
                return new Limit(DefaultLimitValue);
            }
        }
    }
}