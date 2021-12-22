using System;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Common;
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
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly What3wordsHttpClient _what3WordsHttpClient;
        private readonly What3wordsSettings _what3WordsSettings;

        #endregion

        #region Ctor

        public ServiceManager(IAddressService addressService,
            IGenericAttributeService genericAttributeService,
            ILogger logger,
            IStoreContext storeContext,
            IWorkContext workContext,
            What3wordsHttpClient what3WordsHttpClient,
            What3wordsSettings what3WordsSettings)
        {
            _addressService = addressService;
            _genericAttributeService = genericAttributeService;
            _logger = logger;
            _storeContext = storeContext;
            _workContext = workContext;
            _what3WordsHttpClient = what3WordsHttpClient;
            _what3WordsSettings = what3WordsSettings;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get existing API key or create a new one
        /// </summary>
        /// <returns>The asynchronous task whose result contains API ket</returns>
        public async Task<string> GetClientApiAsync()
        {
            if (!string.IsNullOrEmpty(_what3WordsSettings.ApiKey))
                return _what3WordsSettings.ApiKey;

            try
            {
                var store = await _storeContext.GetCurrentStoreAsync();
                var storeUrl = $"{store.Url?.TrimEnd('/')}/";
                return await _what3WordsHttpClient.RequestClientApiAsync(storeUrl);
            }
            catch (Exception exception)
            {
                //log full error
                await _logger.ErrorAsync($"what3words error: {exception.Message}.", exception, await _workContext.GetCurrentCustomerAsync());
                return string.Empty;
            }
        }

        /// <summary>
        /// Save address values for order and customer
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="customer">Customer</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task SaveOrderAddressAsync(Order order, Customer customer)
        {
            if (order is null)
                throw new ArgumentNullException(nameof(order));

            if (customer is null)
                throw new ArgumentNullException(nameof(customer));

            async Task saveAddressValueAsync(int? customerAddressId, int? orderAddressId, string attributeName)
            {
                //get customer address
                var customerAddress = await _addressService.GetAddressByIdAsync(customerAddressId ?? 0);

                //try to get value from customer (in case of using a new address during checkout)
                var addressValue = await _genericAttributeService.GetAttributeAsync<string>(customer, attributeName, order.StoreId);
                if (!string.IsNullOrEmpty(addressValue))
                {
                    //move value from customer to the customer address for next use
                    await _genericAttributeService.SaveAttributeAsync<string>(customer, attributeName, null, order.StoreId);
                    if (customerAddress is not null)
                        await _genericAttributeService.SaveAttributeAsync(customerAddress, What3wordsDefaults.ValueAttribute, addressValue);
                }
                else
                {
                    //or get value from the existing customer address
                    addressValue = customerAddress is not null
                        ? await _genericAttributeService.GetAttributeAsync<string>(customerAddress, What3wordsDefaults.ValueAttribute)
                        : null;
                }

                //save value for the order address if any
                if (!string.IsNullOrEmpty(addressValue))
                {
                    var orderAddress = await _addressService.GetAddressByIdAsync(orderAddressId ?? 0);
                    if (orderAddress is not null)
                        await _genericAttributeService.SaveAttributeAsync(orderAddress, What3wordsDefaults.ValueAttribute, addressValue);
                }
            }

            await saveAddressValueAsync(customer.BillingAddressId, order.BillingAddressId, What3wordsDefaults.BillingAddressAttribute);
            await saveAddressValueAsync(customer.ShippingAddressId, order.ShippingAddressId, What3wordsDefaults.ShippingAddressAttribute);
        }

        /// <summary>
        /// Delete address values
        /// </summary>
        /// <param name="address">Address</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task DeleteAddressAsync(Address address)
        {
            if (address is null)
                throw new ArgumentNullException(nameof(address));

            await _genericAttributeService.SaveAttributeAsync<string>(address, What3wordsDefaults.ValueAttribute, null);
        }

        #endregion
    }
}