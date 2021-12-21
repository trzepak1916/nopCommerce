using System.Threading.Tasks;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Services.Cms;
using Nop.Services.Customers;
using Nop.Services.Events;

namespace Nop.Plugin.Widgets.What3words.Services
{
    /// <summary>
    /// Represents plugin event consumer
    /// </summary>
    public class EventConsumer :
        IConsumer<EntityDeletedEvent<Address>>,
        IConsumer<OrderPlacedEvent>
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IWidgetPluginManager _widgetPluginManager;
        private readonly ServiceManager _serviceManager;
        private readonly What3wordsSettings _what3WordsSettings;

        #endregion

        #region Ctor

        public EventConsumer(ICustomerService customerService,
            IWidgetPluginManager widgetPluginManager,
            ServiceManager serviceManager,
            What3wordsSettings what3WordsSettings)
        {
            _customerService = customerService;
            _widgetPluginManager = widgetPluginManager;
            _serviceManager = serviceManager;
            _what3WordsSettings = what3WordsSettings;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handle entity deleted event
        /// </summary>
        /// <param name="eventMessage">Event message</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityDeletedEvent<Address> eventMessage)
        {
            if (eventMessage.Entity is null)
                return;

            await _serviceManager.DeleteAddressAsync(eventMessage.Entity);
        }

        /// <summary>
        /// Handle order placed event
        /// </summary>
        /// <param name="eventMessage">Event message</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(OrderPlacedEvent eventMessage)
        {
            if (eventMessage.Order is null)
                return;

            var customer = await _customerService.GetCustomerByIdAsync(eventMessage.Order.CustomerId);
            if (!await _widgetPluginManager.IsPluginActiveAsync(What3wordsDefaults.SystemName, customer))
                return;

            if (!_what3WordsSettings.Enabled)
                return;

            await _serviceManager.SaveOrderAddressAsync(eventMessage.Order, customer);
        }

        #endregion

    }
}
