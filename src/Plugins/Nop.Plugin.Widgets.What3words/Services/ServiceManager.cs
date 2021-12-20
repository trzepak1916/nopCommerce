using System;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Common;
using Nop.Services.Logging;

namespace Nop.Plugin.Widgets.What3words.Services
{
    /// <summary>
    /// Represents the plugin service manager
    /// </summary>
    public class ServiceManager
    {
        #region Fields

        private readonly IAddressService _addressService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILogger _logger;
        private readonly IWorkContext _workContext;
        private readonly What3wordsHttpClient _what3WordsHttpClient;

        #endregion

        #region Ctor

        public ServiceManager(IAddressService addressService,
            IGenericAttributeService genericAttributeService,
            ILogger logger,
            IWorkContext workContext,
            What3wordsHttpClient what3WordsHttpClient)
        {
            _addressService = addressService;
            _genericAttributeService = genericAttributeService;
            _logger = logger;
            _workContext = workContext;
            _what3WordsHttpClient = what3WordsHttpClient;
        }

        #endregion

        #region Methods

        public async Task<string> GetClientApiAsync()
        {
            try
            {
                return await _what3WordsHttpClient.RequestAsyncClientApi();
            }
            catch (Exception exception)
            {
                //log full error
                await _logger.ErrorAsync($"what3Words error: {exception.Message}.", exception, await _workContext.GetCurrentCustomerAsync());
                return string.Empty;
            }
        }

        /// <summary>
        /// Save address for order and customer entities
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task SaveOrderAddressAsync(Order order, Customer customer)
        {
            //get field what3words address
            var billingAddress = await _genericAttributeService.GetAttributeAsync<string>(customer, What3wordsDefaults.What3wordsBillingAddressAttribute);
            var shippingAddress = await _genericAttributeService.GetAttributeAsync<string>(customer, What3wordsDefaults.What3wordsShippingAddressAttribute);

            //clear
            await _genericAttributeService.SaveAttributeAsync(customer, What3wordsDefaults.What3wordsBillingAddressAttribute, "");
            await _genericAttributeService.SaveAttributeAsync(customer, What3wordsDefaults.What3wordsShippingAddressAttribute, "");

            //Order Address
            var orderBillingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
            var orderShippingAddress = await _addressService.GetAddressByIdAsync(order.ShippingAddressId ?? 0);
            await _genericAttributeService.SaveAttributeAsync(orderBillingAddress, What3wordsDefaults.What3wordsOrderBillingAddressAttribute, billingAddress);
            await _genericAttributeService.SaveAttributeAsync(orderShippingAddress, What3wordsDefaults.What3wordsOrderShippingAddressAttribute, shippingAddress);

            //Customer Address
            var customerBillingAddress = await _addressService.GetAddressByIdAsync(customer.BillingAddressId ?? 0);
            var customerShippingAddress = await _addressService.GetAddressByIdAsync(customer.ShippingAddressId ?? 0);
            await _genericAttributeService.SaveAttributeAsync(customerBillingAddress, What3wordsDefaults.What3wordsCustomerBillingAddressAttribute, billingAddress);
            await _genericAttributeService.SaveAttributeAsync(customerShippingAddress, What3wordsDefaults.What3wordsCustomerShippingAddressAttribute, shippingAddress);
        }

        #endregion
    }
}
