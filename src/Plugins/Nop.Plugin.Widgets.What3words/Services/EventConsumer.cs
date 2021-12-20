using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Cms;
using Nop.Services.Events;

namespace Nop.Plugin.Widgets.What3words.Services
{
    /// <summary>
    /// Represents plugin event consumer
    /// </summary>
    public class EventConsumer :
        IConsumer<OrderPlacedEvent>
    {
        #region Fields
        
        private readonly IWidgetPluginManager _widgetPluginManager;
        private readonly IWorkContext _workContext;
        private readonly ServiceManager _serviceManager;
        private readonly What3wordsSettings _what3WordsSettings;

        #endregion

        #region Ctor

        public EventConsumer(
            IWidgetPluginManager widgetPluginManager,
            IWorkContext workContext,
            ServiceManager serviceManager,
            What3wordsSettings what3WordsSettings)
        {
            _widgetPluginManager = widgetPluginManager;
            _workContext = workContext;
            _serviceManager = serviceManager;
            _what3WordsSettings = what3WordsSettings;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handle order placed event
        /// </summary>
        /// <param name="eventMessage">Event message</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(OrderPlacedEvent eventMessage)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            if (!await _widgetPluginManager.IsPluginActiveAsync(What3wordsDefaults.SystemName, customer))
                return;

            if (!_what3WordsSettings.Enabled)
                return;

            if (eventMessage?.Order != null)
                await _serviceManager.SaveOrderAddressAsync(eventMessage.Order, customer);
        }

        #endregion

    }
}
