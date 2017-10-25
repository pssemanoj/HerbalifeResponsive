namespace MyHerbalife3.Ordering.WebAPI
{
    /// <summary>
    /// Contains the constants that represent custom headers
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// 
        /// </summary>
        public static class RequestHeaders
        {
            /// <summary>
            /// 
            /// </summary>
            public const string Locale = "X-HLLOCALE";

            /// <summary>
            /// 
            /// </summary>
            public const string ClientApp = "X-HLCLIENTAPP";

            /// <summary>
            /// 
            /// </summary>
            public const string ClientDevice = "X-HLCLIENTDEVICE";

            /// <summary>
            /// 
            /// </summary>
            public const string ClientOs = "X-HLCLIENTOS";

            /// <summary>
            /// 
            /// </summary>
            public const string Build = "X-HLBUILD";

            /// <summary>
            /// 
            /// </summary>
            public const string UserToken = "X-HLUSER-TOKEN";

            /// <summary>
            /// 
            /// </summary>
            public const string Client = "X-HLCLIENT";
        }

        public static class ResponseHeaders
        {
            /// <summary>
            /// 
            /// </summary>
            public const string ServerHost = "X-HLSERVERHOST";

            /// <summary>
            /// 
            /// </summary>
            public const string ServerDate = "X-HLSERVERDATE";
        }

        public static class SSOValues
        {
            /// <summary>
            /// 
            /// </summary>
            public const string Applications = "SSOApplications";

            /// <summary>
            /// 
            /// </summary>
            public const string Locales = "SSOLocales";

            /// <summary>
            /// 
            /// </summary>
            public const string IkioskLocales = "SSOIKIOSKLocales";

            /// <summary>
            /// 
            /// </summary>
            public const string Enabled = "SSOEnabled";

        }

        /// <summary>
        /// 
        /// </summary>
        public const string CustomHeadersKey = "CustomHeaders";

        /// <summary>
        /// 
        /// </summary>
        public const string RequiredHeadersKey = "RequiredCustomHeaders";

        /// <summary>
        /// 
        /// </summary>
        public const string TracingLoggerName = "TracingLogger";
    }
}
