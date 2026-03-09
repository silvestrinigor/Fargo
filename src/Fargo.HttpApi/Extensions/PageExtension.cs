using Fargo.Application.Commom;

namespace Fargo.HttpApi.Extensions
{
    /// <summary>
    /// Provides helper extensions for the <see cref="Page"/> value object.
    /// </summary>
    /// <remarks>
    /// This extension centralizes HTTP/API specific behaviors related to pagination pages.
    /// In particular, it defines the default page index used when a client does not specify one.
    /// </remarks>
    public static class PageExtension
    {
        /// <summary>
        /// The default page index used by the HTTP API when no page is provided by the client.
        /// </summary>
        public const int DefaultPageValue = 1;

        extension(Page page)
        {
            /// <summary>
            /// Creates a <see cref="Page"/> instance using the API default page value.
            /// </summary>
            /// <returns>
            /// A <see cref="Page"/> initialized with <see cref="DefaultPageValue"/>.
            /// </returns>
            /// <remarks>
            /// This helper is intended for HTTP/API scenarios where pagination parameters
            /// are optional and a safe default must be applied when the client does not
            /// explicitly specify a page.
            /// </remarks>
            public static Page DefaultPage()
            {
                return new Page(DefaultPageValue);
            }
        }
    }
}