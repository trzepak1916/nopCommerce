using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Widgets.What3words.Models;
using Nop.Plugin.Widgets.What3words.Services;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Widgets.What3words.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class What3wordsController : BasePluginController
    {
        #region Fields

        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly ServiceManager _serviceManager;
        private readonly What3wordsSettings _what3WordsSettings;

        #endregion

        #region Ctor

        public What3wordsController(IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext,
            IWorkContext workContext,
            ServiceManager serviceManager,
            What3wordsSettings what3WordsSettings)
        {
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
            _workContext = workContext;
            _serviceManager = serviceManager;
            _what3WordsSettings = what3WordsSettings;
        }

        #endregion

        #region Methods

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            var model = new ConfigurationModel
            {
                Enabled = _what3WordsSettings.Enabled
            };

            return View("~/Plugins/Widgets.What3words/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return await Configure();

            _what3WordsSettings.Enabled = model.Enabled;

            //request client api key
            _what3WordsSettings.ApiKey = await _serviceManager.GetClientApiAsync();
            if (string.IsNullOrEmpty(_what3WordsSettings.ApiKey))
            {
                _notificationService
                    .ErrorNotification(await _localizationService.GetResourceAsync("Plugins.Widgets.What3words.Configuration.Failed"));
                return await Configure();
            }

            await _settingService.SaveSettingAsync(_what3WordsSettings);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }

        /// <summary>
        /// Save address
        /// </summary>
        /// <param name="words">Address value</param>
        /// <param name="prefix">"BillingNewAddress" - Billing address; "ShippingNewAddress" - Shipping address</param>
        [HttpPost]
        public async Task<IActionResult> SelectedSuggestion(string words, string prefix)
        {
            if (!_what3WordsSettings.Enabled)
                return Ok();

            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();

            if (prefix == What3wordsDefaults.BillingAddressPrefix)
            {
                //We save both addresses, since shipping address may be the same as billing address
                await _genericAttributeService.SaveAttributeAsync(customer, What3wordsDefaults.BillingAddressAttribute, words, store.Id);
                await _genericAttributeService.SaveAttributeAsync(customer, What3wordsDefaults.ShippingAddressAttribute, words, store.Id);
            }

            if (prefix == What3wordsDefaults.ShippingAddressPrefix)
                await _genericAttributeService.SaveAttributeAsync(customer, What3wordsDefaults.ShippingAddressAttribute, words, store.Id);

            return Ok();
        }
    }

    #endregion
}