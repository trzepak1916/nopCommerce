using Nop.Core;

namespace Nop.Plugin.Widgets.What3words
{
    /// <summary>
    /// Represents plugin constants
    /// </summary>
    public static class What3wordsDefaults
    {
        /// <summary>
        /// Gets the system name
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
        public static string BillingAddressAttribute => "What3wordsBillingAddress";

        /// <summary>
        /// Gets a key of the attribute to store words for what3words shipping address
        /// </summary>
        public static string ShippingAddressAttribute => "What3wordsShippingAddress";

        /// <summary>
        /// Gets a field prefix on the customer address pages
        /// </summary>
        public static string AddressPrefix => "Address";

        /// <summary>
        /// Gets a field prefix on the checkout billing address page
        /// </summary>
        public static string BillingAddressPrefix => "BillingNewAddress";

        /// <summary>
        /// Gets a field prefix on the checkout billing address page
        /// </summary>
        public static string ShippingAddressPrefix => "ShippingNewAddress";
    }
}