using Nop.Core;

namespace Nop.Plugin.Widgets.What3words
{
    public static class What3wordsDefaults
    {
        /// <summary>
        /// Retargeting system name
        /// </summary>
        public static string SystemName => "Widgets.What3words";

        /// <summary>
        /// Gets the user agent used to request third-party services
        /// </summary>
        public static string UserAgent => $"nopCommerce-{NopVersion.CURRENT_VERSION}";

        /// <summary>
        /// Name of the view component to display widget in public store
        /// </summary>
        public const string VIEW_COMPONENT_NAME = "What3words";

        /// <summary>
        /// Name of the view component to display widget in admin panel (order details)
        /// </summary>
        public const string ORDER_ADMIN_VIEW_COMPONENT_NAME = "What3wordsOrderAdmin";

        /// <summary>
        /// Name of the view component to display widget in public store (order summary)
        /// </summary>
        public const string ORDER_PUBLIC_VIEW_COMPONENT_NAME = "What3wordsOrderPublic";

        /// <summary>
        /// Gets the configuration route name
        /// </summary>
        public static string ConfigurationRouteName => "Plugin.Widgets.What3words.Configure";

        /// <summary>
        /// Gets a key of the attribute to store words for what3words billing address
        /// </summary>
        public static string What3wordsBillingAddressAttribute => "What3wordsBillingAddress";

        /// <summary>
        /// Gets a key of the attribute to store words for what3words shipping address
        /// </summary>
        public static string What3wordsShippingAddressAttribute => "What3wordsShippingAddress";

        /// <summary>
        /// Gets a key of the attribute to store words for what3words Order billing address
        /// </summary>
        public static string What3wordsOrderBillingAddressAttribute => "What3wordsOrderBillingAddress";

        /// <summary>
        /// Gets a key of the attribute to store words for what3words Order shipping address
        /// </summary>
        public static string What3wordsOrderShippingAddressAttribute => "What3wordsOrderShippingAddress";

        /// <summary>
        /// Gets a key of the attribute to store words for what3words Customer billing address
        /// </summary>
        public static string What3wordsCustomerBillingAddressAttribute => "What3wordsCustomerBillingAddress";

        /// <summary>
        /// Gets a key of the attribute to store words for what3words Customer shipping address
        /// </summary>
        public static string What3wordsCustomerShippingAddressAttribute => "What3wordsCustomerShippingAddress";

        /// <summary>
        /// Gets the route name of customer address edit endpoint
        /// </summary>
        public static string CustomerInfoAddressRouteName => "CustomerAddressEdit";
    }
}
