using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Plugin.Widgets.What3words.Models;
using Nop.Services.Cms;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Widgets.What3words.Components
{
    [ViewComponent(Name = What3wordsDefaults.VIEW_COMPONENT_NAME)]
    public class What3wordsViewComponent : NopViewComponent
    {
        #region Fields

        private readonly IWidgetPluginManager _widgetPluginManager;
        private readonly What3wordsSettings _what3WordsSettings;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public What3wordsViewComponent(
            IWidgetPluginManager widgetPluginManager,
            What3wordsSettings what3WordsSettings,
            IWorkContext workContext
            )
        {
            _widgetPluginManager = widgetPluginManager;
            _what3WordsSettings = what3WordsSettings;
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

            var routeName = HttpContext.GetEndpoint()?.Metadata.GetMetadata<RouteNameMetadata>()?.RouteName;
            if (routeName.Equals(What3wordsDefaults.CustomerInfoAddressRouteName))
                    return Content(string.Empty);

            if (widgetZone.Equals(PublicWidgetZones.AddressBottom))
            {
                var model = new What3wordsAddressModel
                {
                    ApiKey = _what3WordsSettings.ApiKey,
                    Prefix = ViewData.TemplateInfo.HtmlFieldPrefix
                };

                return View("~/Plugins/Widgets.What3words/Views/PublicInfo.cshtml", model);
            }
            else
                return Content(string.Empty);
        }

        #endregion
    }
}
