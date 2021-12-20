using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Nop.Core;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Nop.Plugin.Widgets.What3words.Services
{
    /// <summary>
    /// Represents HTTP client to request what3words services
    /// </summary>
    public class What3wordsHttpClient
    {
        #region Fields

        private readonly HttpClient _httpClient;

        #endregion

        #region Ctor

        public What3wordsHttpClient(HttpClient httpClient)
        {
            //configure client
            httpClient.BaseAddress = new Uri("https://www.nopcommerce.com/");
            httpClient.Timeout = TimeSpan.FromSeconds(20);
            httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, What3wordsDefaults.UserAgent);
            httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, MimeTypes.ApplicationJson);

            _httpClient = httpClient;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Request client api
        /// </summary>
        /// <returns>The asynchronous task whose result contains response details</returns>
        public async Task<string> RequestAsyncClientApi()
        {
            try
            {
                //execute request and get response
                var httpResponse = await _httpClient.GetStringAsync("what3words/client-api");

                //return result
                var result = JsonConvert.DeserializeAnonymousType(httpResponse,
                                new { Message = string.Empty, ClientApi = string.Empty });
                
                if (string.IsNullOrEmpty(result.Message))
                    throw new NopException($"Generating client keys error - {result.Message}");
                
                return result.ClientApi;
            }
            catch (AggregateException exception)
            {
                //rethrow actual exception
                throw exception.InnerException;
            }
        }

        #endregion
    }
}