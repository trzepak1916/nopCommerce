using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Services.Localization;
using Nop.Services.Seo;
using Nop.Services.Topics;
using Nop.Web.Framework.Extensions;
using Nop.Web.Models.Boards;

namespace Nop.Web.Extensions
{
    public static class HtmlExtensions
    {
        /// <summary>
        /// Prepare a special small pager for forum topics
        /// </summary>
        /// <typeparam name="TModel">Model type</typeparam>
        /// <param name="html">HTML helper</param>
        /// <param name="model">Model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the pager
        /// </returns>
        public static async Task<IHtmlContent> ForumTopicSmallPagerAsync<TModel>(this IHtmlHelper<TModel> html, ForumTopicRowModel model)
        {
            var localizationService = EngineContext.Current.Resolve<ILocalizationService>();

            var forumTopicId = model.Id;
            var forumTopicSlug = model.SeName;
            var totalPages = model.TotalPostPages;

            if (totalPages > 0)
            {
                var links = new StringBuilder();

                if (totalPages <= 4)
                {
                    for (var x = 1; x <= totalPages; x++)
                    {
                        var link = html.RouteLink(x.ToString(),
                            "TopicSlugPaged",
                            new { id = forumTopicId, pageNumber = x, slug = forumTopicSlug },
                            new { title = string.Format(await localizationService.GetResourceAsync("Pager.PageLinkTitle"), x.ToString()) });
                        links.Append(await link.RenderHtmlContentAsync());
                        if (x < totalPages)
                            links.Append(", ");
                    }
                }
                else
                {
                    var link1 = html.RouteLink("1",
                        "TopicSlugPaged",
                        new { id = forumTopicId, pageNumber = 1, slug = forumTopicSlug },
                        new { title = string.Format(await localizationService.GetResourceAsync("Pager.PageLinkTitle"), 1) });
                    links.Append(await link1.RenderHtmlContentAsync());

                    links.Append(" ... ");

                    for (var x = totalPages - 2; x <= totalPages; x++)
                    {
                        var link2 = html.RouteLink(x.ToString(),
                            "TopicSlugPaged",
                            new { id = forumTopicId, pageNumber = x, slug = forumTopicSlug },
                            new { title = string.Format(await localizationService.GetResourceAsync("Pager.PageLinkTitle"), x.ToString()) });
                        links.Append(await link2.RenderHtmlContentAsync());

                        if (x < totalPages)
                            links.Append(", ");
                    }
                }

                // Inserts the topic page links into the localized string ([Go to page: {0}])
                return new HtmlString(string.Format(await localizationService.GetResourceAsync("Forum.Topics.GotoPostPager"), links));
            }

            return new HtmlString(string.Empty);
        }

        /// <summary>
        /// Get topic SEO name by system name
        /// </summary>
        /// <typeparam name="TModel">Model type</typeparam>
        /// <param name="html">HTML helper</param>
        /// <param name="systemName">System name</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the topic SEO Name
        /// </returns>
        public static async Task<string> GetTopicSeNameAsync<TModel>(this IHtmlHelper<TModel> html, string systemName)
        {
            var storeContext = EngineContext.Current.Resolve<IStoreContext>();
            var store = await storeContext.GetCurrentStoreAsync();
            var topicService = EngineContext.Current.Resolve<ITopicService>();
            var topic = await topicService.GetTopicBySystemNameAsync(systemName, store.Id);

            if (topic == null)
                return string.Empty;

            var urlRecordService = EngineContext.Current.Resolve<IUrlRecordService>();

            return await urlRecordService.GetSeNameAsync(topic);
        }
    }
}