using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Widgets.What3words
{
    /// <summary>
    /// Represents what3words plugin
    /// </summary>
    public class What3wordsPlugin : BasePlugin, IWidgetPlugin
    {
        #region Fields

        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IUrlHelperFactory _urlHelperFactory;

        #endregion

        #region Ctor

        public What3wordsPlugin(
            IActionContextAccessor actionContextAccessor,
            ILocalizationService localizationService,
            ISettingService settingService,
            IUrlHelperFactory urlHelperFactory)
        {
            _actionContextAccessor = actionContextAccessor;
            _localizationService = localizationService;
            _settingService = settingService;
            _urlHelperFactory = urlHelperFactory;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets widget zones where this widget should be rendered
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the widget zones
        /// </returns>
        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string> 
            {
                PublicWidgetZones.AddressBottom,
                PublicWidgetZones.OrderSummaryBillingAddressBottom,
                PublicWidgetZones.OrderSummaryShippingAddressBottom,

                AdminWidgetZones.OrderBillingAddressDetailsBottom,
                AdminWidgetZones.OrderShippingAddressDetailsBottom
            });
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext).RouteUrl(What3wordsDefaults.ConfigurationRouteName);
        }

        /// <summary>
        /// Gets a name of a view component for displaying widget
        /// </summary>
        /// <param name="widgetZone">Name of the widget zone</param>
        /// <returns>View component name</returns>
        public string GetWidgetViewComponentName(string widgetZone)
        {
            if (widgetZone.Equals(PublicWidgetZones.OrderSummaryBillingAddressBottom) || 
                widgetZone.Equals(PublicWidgetZones.OrderSummaryShippingAddressBottom))
                return What3wordsDefaults.ORDER_PUBLIC_VIEW_COMPONENT_NAME;

            if (widgetZone.Equals(AdminWidgetZones.OrderBillingAddressDetailsBottom) ||
                widgetZone.Equals(AdminWidgetZones.OrderShippingAddressDetailsBottom))
                return What3wordsDefaults.ORDER_ADMIN_VIEW_COMPONENT_NAME;

            return What3wordsDefaults.VIEW_COMPONENT_NAME;
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task InstallAsync()
        {
            //locales
            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.Widgets.What3words.Configuration"] = "Configuration",
                ["Plugins.Widgets.What3words.Configuration.Fields.Enabled"] = "Enabled",
                ["Plugins.Widgets.What3words.Configuration.Fields.Enabled.Hint"] = "Toggle to enable/disable what3words service.",
                ["Plugins.Widgets.What3words.Configuration.Filed"] = "Failed to get the generated API key.",
                ["Plugins.Widgets.What3words.Address.Field.Label"] = "what3words address",
                ["Plugins.Widgets.What3words.Address.Field.Tooltip"] = "Is your property hard to find? To help ypur delivery driver find your exact location, please enter your what3words dekivery address.",
                ["Plugins.Widgets.What3words.Address.Field.Tooltip.Link"] = "Find yours here."
            });

            await base.InstallAsync();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<What3wordsSettings>();

            //locales
            await _localizationService.DeleteLocaleResourcesAsync("Plugins.Widgets.What3words");

            await base.UninstallAsync();
        }

        #endregion

        /// <summary>
        /// Gets a value indicating whether to hide this plugin on the widget list page in the admin area
        /// </summary>
        public bool HideInWidgetList => false;
    }
}