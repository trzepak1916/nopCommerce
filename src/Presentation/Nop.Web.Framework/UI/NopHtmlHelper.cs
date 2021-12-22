using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using Nop.Core;
using Nop.Core.Configuration;
using Nop.Core.Domain.Seo;
using Nop.Services.Localization;
using Nop.Services.Themes;
using Nop.Web.Framework.Configuration;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Framework.Themes;
using Nop.Web.Framework.UI.Paging;
using WebOptimizer;

namespace Nop.Web.Framework.UI
{
    /// <summary>
    /// Represents the HTML helper implementation
    /// </summary>
    public partial class NopHtmlHelper : INopHtmlHelper
    {
        #region Fields

        private readonly AppSettings _appSettings;
        private readonly HtmlEncoder _htmlEncoder;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IAssetPipeline _assetPipeline;
        private readonly ILocalizationService _localizationService;
        private readonly IThemeContext _themeContext;
        private readonly IThemeProvider _themeProvider;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly SeoSettings _seoSettings;

        private readonly Dictionary<ResourceLocation, List<ScriptReferenceMeta>> _scriptParts = new();
        private readonly Dictionary<ResourceLocation, List<string>> _inlineScriptParts = new();
        private readonly List<CssReferenceMeta> _cssParts = new();

        private readonly List<string> _canonicalUrlParts = new();
        private readonly List<string> _headCustomParts = new();
        private readonly List<string> _metaDescriptionParts = new();
        private readonly List<string> _metaKeywordParts = new();
        private readonly List<string> _pageCssClassParts = new();
        private readonly List<string> _titleParts = new();

        private string _activeAdminMenuSystemName;
        private string _editPageUrl;

        #endregion

        #region Ctor

        public NopHtmlHelper(AppSettings appSettings,
            IActionContextAccessor actionContextAccessor,
            HtmlEncoder htmlEncoder,
            IAssetPipeline assetPipeline,
            ILocalizationService localizationService,
            IThemeContext themeContext,
            IThemeProvider themeProvider,
            IUrlHelperFactory urlHelperFactory,
            IWebHostEnvironment webHostEnvironment,
            SeoSettings seoSettings)
        {
            _appSettings = appSettings;
            _actionContextAccessor = actionContextAccessor;
            _htmlEncoder = htmlEncoder;
            _assetPipeline = assetPipeline;
            _localizationService = localizationService;
            _themeContext = themeContext;
            _themeProvider = themeProvider;
            _urlHelperFactory = urlHelperFactory;
            _webHostEnvironment = webHostEnvironment;
            _seoSettings = seoSettings;
        }

        #endregion

        #region Utils

        private string GetAssetKey(string key, ResourceLocation location)
        {
            var keyPrefix = Enum.GetName(location) + key;
            var routeKey = GetRouteName(handleDefaultRoutes: true);

            if (string.IsNullOrEmpty(routeKey))
                return keyPrefix;

            return string.Concat(routeKey, ".", keyPrefix);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Add title element to the <![CDATA[<head>]]>
        /// </summary>
        /// <param name="part">Title part</param>
        public virtual void AddTitleParts(string part)
        {
            if (string.IsNullOrEmpty(part))
                return;

            _titleParts.Add(part);
        }

        /// <summary>
        /// Append title element to the <![CDATA[<head>]]>
        /// </summary>
        /// <param name="part">Title part</param>
        public virtual void AppendTitleParts(string part)
        {
            if (string.IsNullOrEmpty(part))
                return;

            _titleParts.Insert(0, part);
        }

        /// <summary>
        /// Generate all title parts
        /// </summary>
        /// <param name="addDefaultTitle">A value indicating whether to insert a default title</param>
        /// <param name="part">Title part</param>
        /// <returns>Generated HTML string</returns>
        public virtual IHtmlContent GenerateTitle(bool addDefaultTitle = true, string part = "")
        {
            AppendTitleParts(part);

            var specificTitle = string.Join(_seoSettings.PageTitleSeparator, _titleParts.AsEnumerable().Reverse().ToArray());
            string result;
            if (!string.IsNullOrEmpty(specificTitle))
            {
                if (addDefaultTitle)
                {
                    //store name + page title
                    switch (_seoSettings.PageTitleSeoAdjustment)
                    {
                        case PageTitleSeoAdjustment.PagenameAfterStorename:
                            {
                                result = string.Join(_seoSettings.PageTitleSeparator, _seoSettings.DefaultTitle, specificTitle);
                            }
                            break;
                        case PageTitleSeoAdjustment.StorenameAfterPagename:
                        default:
                            {
                                result = string.Join(_seoSettings.PageTitleSeparator, specificTitle, _seoSettings.DefaultTitle);
                            }
                            break;
                    }
                }
                else
                {
                    //page title only
                    result = specificTitle;
                }
            }
            else
            {
                //store name only
                result = _seoSettings.DefaultTitle;
            }
            return new HtmlString(_htmlEncoder.Encode(result));
        }

        /// <summary>
        /// Add meta description element to the <![CDATA[<head>]]>
        /// </summary>
        /// <param name="part">Meta description part</param>
        public virtual void AddMetaDescriptionParts(string part)
        {
            if (string.IsNullOrEmpty(part))
                return;

            _metaDescriptionParts.Add(part);
        }

        /// <summary>
        /// Append meta description element to the <![CDATA[<head>]]>
        /// </summary>
        /// <param name="part">Meta description part</param>
        public virtual void AppendMetaDescriptionParts(string part)
        {
            if (string.IsNullOrEmpty(part))
                return;

            _metaDescriptionParts.Insert(0, part);
        }

        /// <summary>
        /// Generate all description parts
        /// </summary>
        /// <param name="part">Meta description part</param>
        /// <returns>Generated HTML string</returns>
        public virtual IHtmlContent GenerateMetaDescription(string part = "")
        {
            AppendMetaDescriptionParts(part);

            var metaDescription = string.Join(", ", _metaDescriptionParts.AsEnumerable().Reverse().ToArray());
            var result = !string.IsNullOrEmpty(metaDescription) ? metaDescription : _seoSettings.DefaultMetaDescription;

            return new HtmlString(_htmlEncoder.Encode(result));
        }

        /// <summary>
        /// Add meta keyword element to the <![CDATA[<head>]]>
        /// </summary>
        /// <param name="part">Meta keyword part</param>
        public virtual void AddMetaKeywordParts(string part)
        {
            if (string.IsNullOrEmpty(part))
                return;

            _metaKeywordParts.Add(part);
        }

        /// <summary>
        /// Append meta keyword element to the <![CDATA[<head>]]>
        /// </summary>
        /// <param name="part">Meta keyword part</param>
        public virtual void AppendMetaKeywordParts(string part)
        {
            if (string.IsNullOrEmpty(part))
                return;

            _metaKeywordParts.Insert(0, part);
        }

        /// <summary>
        /// Generate all keyword parts
        /// </summary>
        /// <param name="part">Meta keyword part</param>
        /// <returns>Generated HTML string</returns>
        public virtual IHtmlContent GenerateMetaKeywords(string part = "")
        {
            AppendMetaKeywordParts(part);

            var metaKeyword = string.Join(", ", _metaKeywordParts.AsEnumerable().Reverse().ToArray());
            var result = !string.IsNullOrEmpty(metaKeyword) ? metaKeyword : _seoSettings.DefaultMetaKeywords;

            return new HtmlString(_htmlEncoder.Encode(result));
        }

        /// <summary>
        /// Add script element
        /// </summary>
        /// <param name="location">A location of the script element</param>
        /// <param name="src">Script path (minified version)</param>
        /// <param name="debugSrc">Script path (full debug version). If empty, then minified version will be used</param>
        /// <param name="excludeFromBundle">A value indicating whether to exclude this script from bundling</param>
        /// <param name="isAsync">A value indicating whether to add an attribute "async" or not for js files</param>
        public virtual void AddScriptParts(ResourceLocation location, string src, string debugSrc = "", bool excludeFromBundle = false, bool isAsync = false)
        {
            if (!_scriptParts.ContainsKey(location))
                _scriptParts.Add(location, new List<ScriptReferenceMeta>());

            if (string.IsNullOrEmpty(src))
                return;

            if (string.IsNullOrEmpty(debugSrc))
                debugSrc = src;

            _scriptParts[location].Add(new ScriptReferenceMeta
            {
                ExcludeFromBundle = excludeFromBundle,
                IsAsync = isAsync,
                Src = src,
                DebugSrc = debugSrc
            });
        }

        /// <summary>
        /// Append script element
        /// </summary>
        /// <param name="location">A location of the script element</param>
        /// <param name="src">Script path (minified version)</param>
        /// <param name="debugSrc">Script path (full debug version). If empty, then minified version will be used</param>
        /// <param name="excludeFromBundle">A value indicating whether to exclude this script from bundling</param>
        /// <param name="isAsync">A value indicating whether to add an attribute "async" or not for js files</param>
        public virtual void AppendScriptParts(ResourceLocation location, string src, string debugSrc = "", bool excludeFromBundle = false, bool isAsync = false)
        {
            if (!_scriptParts.ContainsKey(location))
                _scriptParts.Add(location, new List<ScriptReferenceMeta>());

            if (string.IsNullOrEmpty(src))
                return;

            if (string.IsNullOrEmpty(debugSrc))
                debugSrc = src;

            _scriptParts[location].Insert(0, new ScriptReferenceMeta
            {
                ExcludeFromBundle = excludeFromBundle,
                IsAsync = isAsync,
                Src = src,
                DebugSrc = debugSrc
            });
        }

        /// <summary>
        /// Generate all script parts
        /// </summary>
        /// <param name="location">A location of the script element</param>
        /// <returns>Generated HTML string</returns>
        public virtual IHtmlContent GenerateScripts(ResourceLocation location)
        {
            if (!_scriptParts.ContainsKey(location) || _scriptParts[location] == null)
                return HtmlString.Empty;

            if (!_scriptParts.Any())
                return HtmlString.Empty;

            var debugModel = _webHostEnvironment.IsDevelopment();

            var result = new StringBuilder();

            var woConfig = _appSettings.Get<WebOptimizerConfig>();

            var pathBase = _actionContextAccessor.ActionContext?.HttpContext?.Request?.PathBase ?? PathString.Empty;

            if (woConfig.EnableJavaScriptBundling && _scriptParts[location].Any(item => !item.ExcludeFromBundle))
            {
                var bundleKey = string.Concat("/js/", GetAssetKey(woConfig.JavaScriptBundleSuffix, location), ".js");

                var sources = _scriptParts[location]
                    .Where(item => !item.ExcludeFromBundle)
                    .Select(item => debugModel ? item.DebugSrc : item.Src)
                    .Distinct().ToArray();

                if (_assetPipeline.TryGetAssetFromRoute(bundleKey, out var bundleAsset))
                {
                    if (bundleAsset.SourceFiles.Count != sources.Length || !bundleAsset.SourceFiles.SequenceEqual(sources))
                    {
                        bundleAsset.SourceFiles.Clear();
                        foreach (var source in sources)
                            bundleAsset.TryAddSourceFile(source);
                    }
                }
                else
                {
                    bundleAsset = _assetPipeline.AddJavaScriptBundle(bundleKey, sources);
                }

                result.AppendFormat("<script type=\"{0}\" src=\"{1}{2}?v={3}\"></script>",
                    MimeTypes.TextJavascript, pathBase, bundleAsset.Route, bundleAsset.GenerateCacheKey(_actionContextAccessor.ActionContext.HttpContext));
            }

            var scripts = _scriptParts[location]
                .Where(item => !woConfig.EnableJavaScriptBundling || item.ExcludeFromBundle)
                .Distinct();

            foreach (var item in scripts)
            {
                var src = debugModel ? item.DebugSrc : item.Src;

                //remove the application path from the generated URL if exists
                PathString.FromUriComponent(src).StartsWithSegments(pathBase, out var url);

                if (!_assetPipeline.TryGetAssetFromRoute(url, out var asset))
                {
                    asset = _assetPipeline.AddJavaScriptBundle(url, url);
                }

                result.AppendFormat("<script type=\"{0}\" src=\"{1}{2}?v={3}\"></script>",
                    MimeTypes.TextJavascript, pathBase, asset.Route, asset.GenerateCacheKey(_actionContextAccessor.ActionContext.HttpContext));

                result.Append(Environment.NewLine);
            }

            return new HtmlString(result.ToString());
        }

        /// <summary>
        /// Add inline script element
        /// </summary>
        /// <param name="location">A location of the script element</param>
        /// <param name="script">Script</param>
        public virtual void AddInlineScriptParts(ResourceLocation location, string script)
        {
            if (!_inlineScriptParts.ContainsKey(location))
                _inlineScriptParts.Add(location, new());

            if (string.IsNullOrEmpty(script))
                return;

            if (_inlineScriptParts[location].Contains(script))
                return;

            _inlineScriptParts[location].Add(script);
        }

        /// <summary>
        /// Append inline script element
        /// </summary>
        /// <param name="location">A location of the script element</param>
        /// <param name="script">Script</param>
        public virtual void AppendInlineScriptParts(ResourceLocation location, string script)
        {
            if (!_inlineScriptParts.ContainsKey(location))
                _inlineScriptParts.Add(location, new());

            if (string.IsNullOrEmpty(script))
                return;

            if (_inlineScriptParts[location].Contains(script))
                return;

            _inlineScriptParts[location].Insert(0, script);
        }

        /// <summary>
        /// Generate all inline script parts
        /// </summary>
        /// <param name="location">A location of the script element</param>
        /// <returns>Generated HTML string</returns>
        public virtual IHtmlContent GenerateInlineScripts(ResourceLocation location)
        {
            if (!_inlineScriptParts.ContainsKey(location) || _inlineScriptParts[location] == null)
                return HtmlString.Empty;

            if (!_inlineScriptParts.Any())
                return HtmlString.Empty;

            var result = new StringBuilder();
            foreach (var item in _inlineScriptParts[location])
            {
                result.Append(item);
                result.Append(Environment.NewLine);
            }
            return new HtmlString(result.ToString());
        }

        /// <summary>
        /// Add CSS element
        /// </summary>
        /// <param name="src">Script path (minified version)</param>
        /// <param name="debugSrc">Script path (full debug version). If empty, then minified version will be used</param>
        /// <param name="excludeFromBundle">A value indicating whether to exclude this style sheet from bundling</param>
        public virtual void AddCssFileParts(string src, string debugSrc, bool excludeFromBundle = false)
        {
            if (string.IsNullOrEmpty(src))
                return;

            if (string.IsNullOrEmpty(debugSrc))
                debugSrc = src;

            _cssParts.Add(new CssReferenceMeta
            {
                ExcludeFromBundle = excludeFromBundle,
                Src = src,
                DebugSrc = debugSrc
            });
        }

        /// <summary>
        /// Append CSS element
        /// </summary>
        /// <param name="src">Script path (minified version)</param>
        /// <param name="debugSrc">Script path (full debug version). If empty, then minified version will be used</param>
        /// <param name="excludeFromBundle">A value indicating whether to exclude this style sheet from bundling</param>
        public virtual void AppendCssFileParts(string src, string debugSrc, bool excludeFromBundle = false)
        {
            if (string.IsNullOrEmpty(src))
                return;

            if (string.IsNullOrEmpty(debugSrc))
                debugSrc = src;

            _cssParts.Insert(0, new CssReferenceMeta
            {
                Src = src,
                DebugSrc = debugSrc
            });
        }

        /// <summary>
        /// Generate all CSS parts
        /// </summary>
        /// <returns>Generated HTML string</returns>
        public virtual IHtmlContent GenerateCssFiles()
        {
            if (_cssParts.Count == 0)
                return HtmlString.Empty;

            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

            var debugModel = _webHostEnvironment.IsDevelopment();

            var result = new StringBuilder();

            var woConfig = _appSettings.Get<WebOptimizerConfig>();

            var pathBase = _actionContextAccessor.ActionContext?.HttpContext?.Request?.PathBase ?? PathString.Empty;

            if (woConfig.EnableCssBundling && _cssParts.Any(item => !item.ExcludeFromBundle))
            {
                var bundleKey = string.Concat("/css/", GetAssetKey(woConfig.CssBundleSuffix, ResourceLocation.Head), ".css");

                var sources = _cssParts
                    .Where(item => !item.ExcludeFromBundle)
                    .Distinct()
                    .Select(item => debugModel ? item.DebugSrc : item.Src).ToArray();

                var bundleAsset = getOrCreateBundle(bundleKey, sources);

                if (bundleAsset.SourceFiles.Count != sources.Length || !bundleAsset.SourceFiles.SequenceEqual(sources))
                {
                    bundleAsset.SourceFiles.Clear();
                    foreach (var source in sources)
                        bundleAsset.TryAddSourceFile(source);
                }

                result.AppendFormat("<link rel=\"stylesheet\" type=\"{0}\" href=\"{1}{2}?v={3}\" />",
                    MimeTypes.TextCss, pathBase, bundleAsset.Route, bundleAsset.GenerateCacheKey(_actionContextAccessor.ActionContext.HttpContext));
            }

            var styles = _cssParts
                    .Where(item => !woConfig.EnableCssBundling || item.ExcludeFromBundle)
                    .Distinct();

            foreach (var item in styles)
            {
                var src = debugModel ? item.DebugSrc : item.Src;

                //remove the application path from the generated URL if exists
                PathString.FromUriComponent(src).StartsWithSegments(pathBase, out var url);

                var asset = getOrCreateBundle(url, url);

                result.AppendFormat("<link rel=\"stylesheet\" type=\"{0}\" href=\"{1}{2}?v={3}\" />",
                    MimeTypes.TextCss, pathBase, asset.Route, asset.GenerateCacheKey(_actionContextAccessor.ActionContext.HttpContext));
                result.AppendLine();
            }

            return new HtmlString(result.ToString());

            /// <summary>
            /// Get or create an asset to the optimization pipeline.
            /// </summary>
            /// <param name="bundleKey">A registered route</param>
            /// <param name="sourceFiles">Relative file names of the sources to optimize</param>
            /// <returns></returns>//
            IAsset getOrCreateBundle(string bundleKey, params string[] sourceFiles)
            {
                //we call this method directly to avoid applying fingerprint
                if (!_assetPipeline.TryGetAssetFromRoute(bundleKey, out var bundleAsset))
                {
                    bundleAsset = _assetPipeline.AddBundle(bundleKey, $"{MimeTypes.TextCss}; charset=UTF-8", sourceFiles)
                        .EnforceFileExtensions(".css")
                        .AdjustRelativePaths()
                        .Concatenate()
                        .AddResponseHeader(HeaderNames.XContentTypeOptions, "nosniff")
                        .MinifyCss();
                }

                return bundleAsset;
            }
        }

        /// <summary>
        /// Add canonical URL element to the <![CDATA[<head>]]>
        /// </summary>
        /// <param name="part">Canonical URL part</param>
        /// <param name="withQueryString">Whether to use canonical URLs with query string parameters</param>
        public virtual void AddCanonicalUrlParts(string part, bool withQueryString = false)
        {
            if (string.IsNullOrEmpty(part))
                return;

            if (withQueryString)
            {
                //add ordered query string parameters
                var queryParameters = _actionContextAccessor.ActionContext.HttpContext.Request.Query.OrderBy(parameter => parameter.Key)
                    .ToDictionary(parameter => parameter.Key, parameter => parameter.Value.ToString());
                part = QueryHelpers.AddQueryString(part, queryParameters);
            }

            _canonicalUrlParts.Add(part);
        }

        /// <summary>
        /// Append canonical URL element to the <![CDATA[<head>]]>
        /// </summary>
        /// <param name="part">Canonical URL part</param>
        public virtual void AppendCanonicalUrlParts(string part)
        {
            if (string.IsNullOrEmpty(part))
                return;

            _canonicalUrlParts.Insert(0, part);
        }

        /// <summary>
        /// Generate all canonical URL parts
        /// </summary>
        /// <returns>Generated HTML string</returns>
        public virtual IHtmlContent GenerateCanonicalUrls()
        {
            var result = new StringBuilder();
            foreach (var canonicalUrl in _canonicalUrlParts)
            {
                result.AppendFormat("<link rel=\"canonical\" href=\"{0}\" />", canonicalUrl);
                result.Append(Environment.NewLine);
            }
            return new HtmlString(result.ToString());
        }

        /// <summary>
        /// Add any custom element to the <![CDATA[<head>]]> element
        /// </summary>
        /// <param name="part">The entire element. For example, <![CDATA[<meta name="msvalidate.01" content="123121231231313123123" />]]></param>
        public virtual void AddHeadCustomParts(string part)
        {
            if (string.IsNullOrEmpty(part))
                return;

            _headCustomParts.Add(part);
        }

        /// <summary>
        /// Append any custom element to the <![CDATA[<head>]]> element
        /// </summary>
        /// <param name="part">The entire element. For example, <![CDATA[<meta name="msvalidate.01" content="123121231231313123123" />]]></param>
        public virtual void AppendHeadCustomParts(string part)
        {
            if (string.IsNullOrEmpty(part))
                return;

            _headCustomParts.Insert(0, part);
        }

        /// <summary>
        /// Generate all custom elements
        /// </summary>
        /// <returns>Generated HTML string</returns>
        public virtual IHtmlContent GenerateHeadCustom()
        {
            //use only distinct rows
            var distinctParts = _headCustomParts.Distinct().ToList();
            if (!distinctParts.Any())
                return HtmlString.Empty;

            var result = new StringBuilder();
            foreach (var path in distinctParts)
            {
                result.Append(path);
                result.Append(Environment.NewLine);
            }
            return new HtmlString(result.ToString());
        }

        /// <summary>
        /// Add CSS class to the <![CDATA[<head>]]> element
        /// </summary>
        /// <param name="part">CSS class</param>
        public virtual void AddPageCssClassParts(string part)
        {
            if (string.IsNullOrEmpty(part))
                return;

            _pageCssClassParts.Add(part);
        }

        /// <summary>
        /// Append CSS class to the <![CDATA[<head>]]> element
        /// </summary>
        /// <param name="part">CSS class</param>
        public virtual void AppendPageCssClassParts(string part)
        {
            if (string.IsNullOrEmpty(part))
                return;

            _pageCssClassParts.Insert(0, part);
        }

        /// <summary>
        /// Generate all title parts
        /// </summary>
        /// <param name="part">CSS class</param>
        /// <returns>Generated string</returns>
        public virtual string GeneratePageCssClasses(string part = "")
        {
            AppendPageCssClassParts(part);

            var result = string.Join(" ", _pageCssClassParts.AsEnumerable().Reverse().ToArray());

            if (string.IsNullOrEmpty(result))
                return string.Empty;

            return _htmlEncoder.Encode(result);
        }

        /// <summary>
        /// Specify "edit page" URL
        /// </summary>
        /// <param name="url">URL</param>
        public virtual void AddEditPageUrl(string url)
        {
            _editPageUrl = url;
        }

        /// <summary>
        /// Get "edit page" URL
        /// </summary>
        /// <returns>URL</returns>
        public virtual string GetEditPageUrl()
        {
            return _editPageUrl;
        }

        /// <summary>
        /// Specify system name of admin menu item that should be selected (expanded)
        /// </summary>
        /// <param name="systemName">System name</param>
        public virtual void SetActiveMenuItemSystemName(string systemName)
        {
            _activeAdminMenuSystemName = systemName;
        }

        /// <summary>
        /// Get system name of admin menu item that should be selected (expanded)
        /// </summary>
        /// <returns>System name</returns>
        public virtual string GetActiveMenuItemSystemName()
        {
            return _activeAdminMenuSystemName;
        }

        /// <summary>
        /// Ger JQuery Datepicker date format from the .net current culture
        /// </summary>
        /// <returns>Format string that supported in JQuery Datepicker.</returns>
        public string GetJQueryDateFormat()
        {
            /*
                *  Date used in this comment : 5th - Nov - 2009 (Thursday)
                *
                *  .NET    JQueryUI        Output      Comment
                *  --------------------------------------------------------------
                *  d       d               5           day of month(No leading zero)
                *  dd      dd              05          day of month(two digit)
                *  ddd     D               Thu         day short name
                *  dddd    DD              Thursday    day long name
                *  M       m               11          month of year(No leading zero)
                *  MM      mm              11          month of year(two digit)
                *  MMM     M               Nov         month name short
                *  MMMM    MM              November    month name long.
                *  yy      y               09          Year(two digit)
                *  yyyy    yy              2009        Year(four digit)             *
                */

            var currentFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;

            // Convert the date
            currentFormat = currentFormat.Replace("dddd", "DD");
            currentFormat = currentFormat.Replace("ddd", "D");

            // Convert month
            if (currentFormat.Contains("MMMM"))
            {
                currentFormat = currentFormat.Replace("MMMM", "MM");
            }
            else if (currentFormat.Contains("MMM"))
            {
                currentFormat = currentFormat.Replace("MMM", "M");
            }
            else if (currentFormat.Contains("MM"))
            {
                currentFormat = currentFormat.Replace("MM", "mm");
            }
            else
            {
                currentFormat = currentFormat.Replace("M", "m");
            }

            // Convert year
            currentFormat = currentFormat.Contains("yyyy") ?
                currentFormat.Replace("yyyy", "yy") : currentFormat.Replace("yy", "y");

            return currentFormat;
        }

        /// <summary>
        /// Get the route name associated with the request rendering this page
        /// </summary>
        /// <param name="handleDefaultRoutes">A value indicating whether to build the name using engine information unless otherwise specified</param>
        /// <returns>Route name</returns>
        public virtual string GetRouteName(bool handleDefaultRoutes = false)
        {
            var actionContext = _actionContextAccessor.ActionContext;

            if (actionContext is null)
                return string.Empty;

            var httpContext = actionContext.HttpContext;
            var routeName = httpContext.GetEndpoint()?.Metadata.GetMetadata<RouteNameMetadata>()?.RouteName ?? string.Empty;

            if (!string.IsNullOrEmpty(routeName) && routeName != "areaRoute")
                return routeName;

            //then try to get a generic one (actually it's an action name, not the route)
            if (httpContext.GetRouteValue(NopPathRouteDefaults.SeNameFieldKey) is not null &&
                httpContext.GetRouteValue(NopPathRouteDefaults.ActionFieldKey) is string actionKey)
            {
                //there are some cases when the action name doesn't match the route name
                //it's not easy to make them the same, so we'll just handle them here
                return actionKey switch
                {
                    "ProductDetails" => "Product",
                    "TopicDetails" => "Topic",
                    _ => actionKey
                };
            }

            if (handleDefaultRoutes)
            {
                return actionContext.ActionDescriptor switch
                {
                    ControllerActionDescriptor controllerAction => string.Concat(controllerAction.ControllerName, controllerAction.ActionName),
                    CompiledPageActionDescriptor compiledPage => string.Concat(compiledPage.AreaName, compiledPage.ViewEnginePath.Replace("/", "")),
                    PageActionDescriptor pageAction => string.Concat(pageAction.AreaName, pageAction.ViewEnginePath.Replace("/", "")),
                    _ => actionContext.ActionDescriptor.DisplayName.Replace("/", "")
                };
            }

            return routeName;
        }

        /// <summary>
        /// Get a value of the text flow uses for the current UI culture
        /// </summary>
        /// <param name="ignoreRtl">A value indicating whether to we should ignore RTL language property for admin area. False by default</param>
        /// <returns>"rtl" if text flows from right to left; otherwise, "ltr".</returns>
        public string GetUIDirection(bool ignoreRtl = false)
        {
            if (ignoreRtl)
                return "ltr";

            return CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft ? "rtl" : "ltr";
        }

        /// <summary>
        /// Prepare a common pager
        /// </summary>
        /// <param name="model">Pager model</param>
        /// <returns>Pager</returns>
        /// <remarks>We have two pagers: The first one can have custom routes. The second one just adds query string parameter</remarks>
        public Pager Pager(IPageableModel model)
        {
            return new Pager(model, _actionContextAccessor.ActionContext?.HttpContext);
        }

        /// <summary>
        /// Prepare a common pager
        /// </summary>
        /// <param name="model">Pager model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the pager
        /// </returns>
        /// <remarks>We have two pagers: The first one can have custom routes. The second one just adds query string parameter</remarks>
        public async Task<IHtmlContent> PagerAsync(PagerModel model)
        {
            if (model.TotalRecords == 0)
                return new HtmlString(string.Empty);

            var links = new StringBuilder();
            if (model.ShowTotalSummary && (model.TotalPages > 0))
            {
                links.Append("<li class=\"total-summary\">");
                links.AppendFormat(await model.GetCurrentPageTextAsync(), model.PageIndex + 1, model.TotalPages, model.TotalRecords);
                links.Append("</li>");
            }

            if (model.ShowPagerItems && (model.TotalPages > 1))
            {
                var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

                if (model.ShowFirst)
                {
                    //first page
                    if ((model.PageIndex >= 3) && (model.TotalPages > model.IndividualPagesDisplayedCount))
                    {
                        model.RouteValues.pageNumber = 1;

                        links.Append("<li class=\"first-page\">");

                        links.AppendFormat("<a title=\"{0}\" href=\"{1}\">{2}</a>",
                                await _localizationService.GetResourceAsync("Pager.FirstPageTitle"),
                                model.UseRouteLinks ? 
                                    urlHelper.RouteUrl(model.RouteActionName, values: model.RouteValues) :
                                    urlHelper.ActionLink(model.RouteActionName, values: model.RouteValues),
                                await model.GetFirstButtonTextAsync());
                        links.Append("</li>");
                    }
                }

                if (model.ShowPrevious)
                {
                    //previous page
                    if (model.PageIndex > 0)
                    {
                        model.RouteValues.pageNumber = model.PageIndex;

                        links.Append("<li class=\"previous-page\">");

                        links.AppendFormat("<a title=\"{0}\" href=\"{1}\">{2}</a>",
                                await _localizationService.GetResourceAsync("Pager.PreviousPageTitle"),
                                model.UseRouteLinks ?
                                    urlHelper.RouteUrl(model.RouteActionName, values: model.RouteValues) :
                                    urlHelper.ActionLink(model.RouteActionName, values: model.RouteValues),
                                await model.GetPreviousButtonTextAsync());
                        links.Append("</li>");
                    }
                }

                if (model.ShowIndividualPages)
                {
                    //individual pages
                    var firstIndividualPageIndex = model.GetFirstIndividualPageIndex();
                    var lastIndividualPageIndex = model.GetLastIndividualPageIndex();
                    for (var i = firstIndividualPageIndex; i <= lastIndividualPageIndex; i++)
                    {
                        if (model.PageIndex == i)
                        {
                            links.AppendFormat("<li class=\"current-page\"><span>{0}</span></li>", i + 1);
                        }
                        else
                        {
                            model.RouteValues.pageNumber = i + 1;

                            links.Append("<li class=\"individual-page\">");

                            links.AppendFormat("<a title=\"{0}\" href=\"{1}\">{2}</a>",
                                string.Format(await _localizationService.GetResourceAsync("Pager.PageLinkTitle"), i + 1),
                                model.UseRouteLinks ?
                                    urlHelper.RouteUrl(model.RouteActionName, values: model.RouteValues) :
                                    urlHelper.ActionLink(model.RouteActionName, values: model.RouteValues),
                                i + 1);
                            links.Append("</li>");
                        }
                    }
                }

                if (model.ShowNext)
                {
                    //next page
                    if ((model.PageIndex + 1) < model.TotalPages)
                    {
                        model.RouteValues.pageNumber = (model.PageIndex + 2);

                        links.Append("<li class=\"next-page\">");

                        links.AppendFormat("<a title=\"{0}\" href=\"{1}\">{2}</a>",
                            await _localizationService.GetResourceAsync("Pager.NextPageTitle"),
                            model.UseRouteLinks ?
                                urlHelper.RouteUrl(model.RouteActionName, values: model.RouteValues) :
                                urlHelper.ActionLink(model.RouteActionName, values: model.RouteValues),
                            await model.GetNextButtonTextAsync());
                        links.Append("</li>");
                    }
                }

                if (model.ShowLast)
                {
                    //last page
                    if (((model.PageIndex + 3) < model.TotalPages) && (model.TotalPages > model.IndividualPagesDisplayedCount))
                    {
                        model.RouteValues.pageNumber = model.TotalPages;

                        links.Append("<li class=\"last-page\">");

                        links.AppendFormat("<a title=\"{0}\" href=\"{1}\">{2}</a>",
                            await _localizationService.GetResourceAsync("Pager.LastPageTitle"),
                            model.UseRouteLinks ?
                                urlHelper.RouteUrl(model.RouteActionName, values: model.RouteValues) :
                                urlHelper.ActionLink(model.RouteActionName, values: model.RouteValues),
                            await model.GetLastButtonTextAsync());
                        links.Append("</li>");
                    }
                }
            }

            var result = links.ToString();
            if (!string.IsNullOrEmpty(result))
                result = "<ul>" + result + "</ul>";

            return new HtmlString(result);
        }

        /// <summary>
        /// Return a value indicating whether the working language and theme support RTL (right-to-left)
        /// </summary>
        /// <param name="themeName">Theme name</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the value
        /// </returns>
        public async Task<bool> ShouldUseRtlThemeAsync(string themeName = null)
        {
            if (!CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft)
                return false;

            //ensure that the active theme also supports it
            themeName ??= await _themeContext.GetWorkingThemeNameAsync();
            var theme = await _themeProvider.GetThemeBySystemNameAsync(themeName);

            return theme?.SupportRtl ?? false;
        }

        #endregion

        #region Nested classes

        /// <summary>
        /// JS file meta data
        /// </summary>
        private record struct ScriptReferenceMeta
        {
            /// <summary>
            /// A value indicating whether to exclude the script from bundling
            /// </summary>
            public bool ExcludeFromBundle { get; set; }

            /// <summary>
            /// A value indicating whether to load the script asynchronously
            /// </summary>
            public bool IsAsync { get; set; }

            /// <summary>
            /// Src for production
            /// </summary>
            public string Src { get; set; }

            /// <summary>
            /// Src for debugging
            /// </summary>
            public string DebugSrc { get; set; }
        }

        /// <summary>
        /// CSS file meta data
        /// </summary>
        private record struct CssReferenceMeta
        {
            /// <summary>
            /// A value indicating whether to exclude the script from bundling
            /// </summary>
            public bool ExcludeFromBundle { get; set; }

            /// <summary>
            /// Src for production
            /// </summary>
            public string Src { get; set; }

            /// <summary>
            /// Src for debugging
            /// </summary>
            public string DebugSrc { get; set; }
        }

        #endregion
    }
}