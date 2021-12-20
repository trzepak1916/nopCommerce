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
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly ISettingService _settingService;
        private readonly ServiceManager _serviceManager;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public What3wordsController(
            IGenericAttributeService genericAttributeService,
            IPermissionService permissionService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            ISettingService settingService,
            ServiceManager serviceManager,
            IWorkContext workContext)
        {
            _genericAttributeService = genericAttributeService;
            _permissionService = permissionService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _settingService = settingService;
            _serviceManager = serviceManager;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            //load settings for active store scope
            var what3wordsSettings = await _settingService.LoadSettingAsync<What3wordsSettings>();

            //prepare model
            var model = new ConfigurationModel
            {
                Enabled = what3wordsSettings.Enabled
            };

            return View("~/Plugins/Widgets.What3words/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [FormValueRequired("save")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return await Configure();

            //load settings for active store scope
            var what3wordsSettings = await _settingService.LoadSettingAsync<What3wordsSettings>();

            //set settings
            what3wordsSettings.Enabled = model.Enabled;
            await _settingService.SaveSettingAsync(what3wordsSettings, settings => settings.Enabled, clearCache: false);

            //request client api key
            what3wordsSettings.ApiKey = await _serviceManager.GetClientApiAsync();

            if (string.IsNullOrEmpty(what3wordsSettings.ApiKey))
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugins.Widgets.What3words.Configuration.Filed"));
                return await Configure();
            }

            await _settingService.SaveSettingAsync(what3wordsSettings, settings => settings.ApiKey, clearCache: false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

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
            var customer = await _workContext.GetCurrentCustomerAsync();
            switch (prefix)
            {
                case "BillingNewAddress":
                    //We save both addresses, since shipping address may be the same as billing address
                    await _genericAttributeService.SaveAttributeAsync(customer, What3wordsDefaults.What3wordsBillingAddressAttribute, words);
                    await _genericAttributeService.SaveAttributeAsync(customer, What3wordsDefaults.What3wordsShippingAddressAttribute, words);
                    break;
                case "ShippingNewAddress":
                    await _genericAttributeService.SaveAttributeAsync(customer, What3wordsDefaults.What3wordsShippingAddressAttribute, words);
                    break;
            }
            
            return Ok();
        }
    }

    #endregion
}