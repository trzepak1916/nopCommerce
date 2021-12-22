using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Models.Order;
using Nop.Web.Models.ShoppingCart;

namespace Nop.Plugin.Widgets.What3words.Components
{
    [ViewComponent(Name = What3wordsDefaults.ORDER_PUBLIC_VIEW_COMPONENT_NAME)]
    public class What3wordsOrderPublicViewComponent : NopViewComponent
    {
        #region Fields

        private readonly IAddressService _addressService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IStoreContext _storeContext;
        private readonly IWidgetPluginManager _widgetPluginManager;
        private readonly IWorkContext _workContext;
        private readonly What3wordsSettings _what3WordsSettings;

        #endregion

        #region Ctor

        public What3wordsOrderPublicViewComponent(IAddressService addressService,
            IGenericAttributeService genericAttributeService,
            IStoreContext storeContext,
            IWidgetPluginManager widgetPluginManager,
            IWorkContext workContext,
            What3wordsSettings what3WordsSettings)
        {
            _addressService = addressService;
            _genericAttributeService = genericAttributeService;
            _storeContext = storeContext;
            _widgetPluginManager = widgetPluginManager;
            _workContext = workContext;
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
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _widgetPluginManager.IsPluginActiveAsync(What3wordsDefaults.SystemName, customer))
                return Content(string.Empty);

            if (!_what3WordsSettings.Enabled)
                return Content(string.Empty);

            var store = await _storeContext.GetCurrentStoreAsync();
            var value = widgetZone.Equals(PublicWidgetZones.OrderSummaryBillingAddress)
                ? await _genericAttributeService.GetAttributeAsync<string>(customer, What3wordsDefaults.BillingAddressAttribute, store.Id)
                : (widgetZone.Equals(PublicWidgetZones.OrderSummaryShippingAddress)
                ? await _genericAttributeService.GetAttributeAsync<string>(customer, What3wordsDefaults.ShippingAddressAttribute, store.Id)
                : null);

            if (string.IsNullOrEmpty(value) && additionalData is ShoppingCartModel.OrderReviewDataModel summaryModel)
            {
                var address = widgetZone.Equals(PublicWidgetZones.OrderSummaryBillingAddress)
                    ? await _addressService.GetAddressByIdAsync(summaryModel.BillingAddress.Id)
                    : (widgetZone.Equals(PublicWidgetZones.OrderSummaryShippingAddress)
                    ? await _addressService.GetAddressByIdAsync(summaryModel.ShippingAddress.Id)
                    : null);
                value = address is not null
                    ? await _genericAttributeService.GetAttributeAsync<string>(address, What3wordsDefaults.ValueAttribute)
                    : null;
            }

            if (string.IsNullOrEmpty(value) && additionalData is OrderDetailsModel detailsModel)
            {
                var address = widgetZone.Equals(PublicWidgetZones.OrderDetailsBillingAddress)
                    ? await _addressService.GetAddressByIdAsync(detailsModel.BillingAddress.Id)
                    : (widgetZone.Equals(PublicWidgetZones.OrderDetailsShippingAddress)
                    ? await _addressService.GetAddressByIdAsync(detailsModel.ShippingAddress.Id)
                    : null);
                value = address is not null
                    ? await _genericAttributeService.GetAttributeAsync<string>(address, What3wordsDefaults.ValueAttribute)
                    : null;
            }

            if (string.IsNullOrEmpty(value))
                return Content(string.Empty);

            return View("~/Plugins/Widgets.What3words/Views/PublicOrderAddress.cshtml", value);
        }

        #endregion
    }
}
