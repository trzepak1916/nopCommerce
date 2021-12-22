using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IWidgetPluginManager _widgetPluginManager;
        private readonly What3wordsSettings _what3WordsSettings;

        #endregion

        #region Ctor

        public What3wordsOrderAdminViewComponent(IAddressService addressService,
            IGenericAttributeService genericAttributeService,
            IWidgetPluginManager widgetPluginManager,
            What3wordsSettings what3WordsSettings)
        {
            _addressService = addressService;
            _genericAttributeService = genericAttributeService;
            _widgetPluginManager = widgetPluginManager;
            _what3WordsSettings = what3WordsSettings;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Invoke the widget view component
        /// </summary>
        /// <param name="widgetZone">Widget zone</param>
        /// <param name="additionalData">Additional parameters</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the view component result
        /// </returns>
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            //ensure that what3words widget is active and enabled
            if (!await _widgetPluginManager.IsPluginActiveAsync(What3wordsDefaults.SystemName))
                return Content(string.Empty);

            if (!_what3WordsSettings.Enabled)
                return Content(string.Empty);

            if (additionalData is not OrderModel model)
                return Content(string.Empty);

            var address = widgetZone.Equals(AdminWidgetZones.OrderBillingAddressDetailsBottom)
                ? await _addressService.GetAddressByIdAsync(model.BillingAddress.Id)
                : (widgetZone.Equals(AdminWidgetZones.OrderShippingAddressDetailsBottom)
                ? await _addressService.GetAddressByIdAsync(model.ShippingAddress.Id)
                : null);
            var value = address is not null
                ? await _genericAttributeService.GetAttributeAsync<string>(address, What3wordsDefaults.ValueAttribute)
                : null;
            if (string.IsNullOrEmpty(value))
                return Content(string.Empty);

            return View("~/Plugins/Widgets.What3words/Views/AdminOrderAddress.cshtml", value);
        }

        #endregion
    }
}
