using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Widgets.What3words.Components
{
    [ViewComponent(Name = What3wordsDefaults.ORDER_ADMIN_VIEW_COMPONENT_NAME)]
    public class What3wordsOrderAdminViewComponent : NopViewComponent
    {
        #region Fields

        private readonly IAddressService _addressService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly What3wordsSettings _what3WordsSettings;
        private readonly IWidgetPluginManager _widgetPluginManager;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public What3wordsOrderAdminViewComponent(IAddressService addressService, 
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
            if (widgetZone.Equals(AdminWidgetZones.OrderBillingAddressDetailsBottom))
            {
                var orderBillingAddressId = additionalData is OrderModel model ? model.BillingAddress.Id : 0;
                var orderBillingAddress = await _addressService.GetAddressByIdAsync(orderBillingAddressId);
                value = await _genericAttributeService.GetAttributeAsync<string>(orderBillingAddress, What3wordsDefaults.What3wordsOrderBillingAddressAttribute);
            }

            if (widgetZone.Equals(AdminWidgetZones.OrderShippingAddressDetailsBottom))
            {
                var orderShippingAddressId = additionalData is OrderModel model ? model.ShippingAddress.Id : 0;
                var orderShippingAddress = await _addressService.GetAddressByIdAsync(orderShippingAddressId);
                value = await _genericAttributeService.GetAttributeAsync<string>(orderShippingAddress, What3wordsDefaults.What3wordsOrderShippingAddressAttribute);
            }
            
            if (string.IsNullOrEmpty(value))
                return Content(string.Empty);

            return View("~/Plugins/Widgets.What3words/Views/AdminOrderAddress.cshtml", value);
        }

        #endregion
    }
}
