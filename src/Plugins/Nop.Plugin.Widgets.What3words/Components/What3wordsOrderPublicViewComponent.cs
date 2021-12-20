using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Widgets.What3words.Components
{
    [ViewComponent(Name = What3wordsDefaults.ORDER_PUBLIC_VIEW_COMPONENT_NAME)]
    public class What3wordsOrderPublicViewComponent : NopViewComponent
    {
        #region Fields

        private readonly IAddressService _addressService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly What3wordsSettings _what3WordsSettings;
        private readonly IWidgetPluginManager _widgetPluginManager;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public What3wordsOrderPublicViewComponent(IAddressService addressService, 
            IGenericAttributeService genericAttributeService,
            What3wordsSettings what3WordsSettings,
            IWidgetPluginManager widgetPluginManager,
            IWorkContext workContext
            )
        {
            _addressService = addressService;
            _genericAttributeService = genericAttributeService;
            _what3WordsSettings = what3WordsSettings;
            _widgetPluginManager = widgetPluginManager;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            //ensure that what3words widget is active and enabled
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!(await _widgetPluginManager.IsPluginActiveAsync(What3wordsDefaults.SystemName, customer) &&
                _what3WordsSettings.Enabled))
                return Content(string.Empty);

            var value = string.Empty;
            if (widgetZone.Equals(PublicWidgetZones.OrderSummaryBillingAddressBottom))
            {
                value = await _genericAttributeService.GetAttributeAsync<string>(customer, What3wordsDefaults.What3wordsBillingAddressAttribute);
                if (string.IsNullOrEmpty(value))
                {
                    var customerBillingAddress = await _addressService.GetAddressByIdAsync(customer.BillingAddressId ?? 0);
                    value = await _genericAttributeService.GetAttributeAsync<string>(customerBillingAddress, What3wordsDefaults.What3wordsCustomerBillingAddressAttribute);
                }
            }

            if (widgetZone.Equals(PublicWidgetZones.OrderSummaryShippingAddressBottom))
            {
                value = await _genericAttributeService.GetAttributeAsync<string>(customer, What3wordsDefaults.What3wordsShippingAddressAttribute);
                if (string.IsNullOrEmpty(value))
                {
                    var customerShippingAddress = await _addressService.GetAddressByIdAsync(customer.ShippingAddressId ?? 0);
                    value = await _genericAttributeService.GetAttributeAsync<string>(customerShippingAddress, What3wordsDefaults.What3wordsCustomerShippingAddressAttribute);
                }
            }

            if (string.IsNullOrEmpty(value))
                return Content(string.Empty);

            return View("~/Plugins/Widgets.What3words/Views/PublicOrderAddress.cshtml", value);
        }

        #endregion
    }
}
