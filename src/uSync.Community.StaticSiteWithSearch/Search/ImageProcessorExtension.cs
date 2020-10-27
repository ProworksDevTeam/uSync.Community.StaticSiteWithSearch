﻿using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using uSync.Community.StaticSiteWithSearch.Config;
using uSync.Community.StaticSiteWithSearch.Publisher;
using uSync.Publisher.Publishers;

namespace uSync.Community.StaticSiteWithSearch.Search
{
    public class ImageProcessorExtension : BaseStaticSitePublisherExtension
    {
        private static readonly UTF8Encoding _encoding = new UTF8Encoding(false);
        private readonly ILogger _logger;

        public ImageProcessorExtension(ILogger logger)
        {
            _logger = logger;
        }

        public override object BeginPublish(Guid id, string syncRoot, SyncPublisherAction action, ActionArguments args, IPublisherSearchConfig config)
        {
            var baseUri = HttpContext.Current.Request.Url;
            var basePath = Path.GetFullPath(Path.Combine(syncRoot, id.ToString()));
            return new Context(baseUri, basePath);
        }

        public override string TransformHtml(object state, IContent content, string itemPath, string generatedHtml)
        {
            try
            {
                if (state is Context ctx)
                {
                    var doc = new HtmlDocument();
                    var hadChanges = false;

                    doc.LoadHtml(generatedHtml);

                    foreach (var node in doc.DocumentNode.DescendantsAndSelf())
                    {
                        if (node.NodeType != HtmlNodeType.Element
                            || !string.Equals("IMG", node.Name, StringComparison.InvariantCultureIgnoreCase)
                            || !(node.Attributes["src"]?.DeEntitizeValue is string str && Uri.TryCreate(str, UriKind.Relative, out var src) && str.StartsWith("/media", StringComparison.InvariantCultureIgnoreCase) && str.Contains("?"))
                            ) continue;

                        var updated = RetrieveAndSaveUrl(ctx, itemPath, src);
                        if (string.IsNullOrWhiteSpace(updated)) continue;
                        
                        node.Attributes["src"].Value = updated;
                        hadChanges = true;
                    }

                    if (hadChanges) {
                        using (var ms = new MemoryStream())
                        {
                            doc.Save(ms, _encoding);
                            ms.Flush();

                            generatedHtml = _encoding.GetString(ms.ToArray());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Warn<ImageProcessorExtension>(ex, "Could not transform HTML for {path}", itemPath);
            }

            return base.TransformHtml(state, content, itemPath, generatedHtml);
        }

        private string RetrieveAndSaveUrl(Context ctx, string itemPath, Uri src)
        {
            var baseUrl = new Uri(ctx.BaseUri, !itemPath.EndsWith("/") ? itemPath + '/' : itemPath);
            var url = new Uri(baseUrl, src);
            var query = url.Query;

            if (!string.IsNullOrWhiteSpace(query)) query = query.Substring(1);
            if (string.IsNullOrWhiteSpace(query)) return null;

            var relativePath = url.AbsolutePath.TrimStart('/');
            var extension = Path.GetExtension(relativePath);
            var idx = query.IndexOf("&format=");
            idx = idx >= 0 ? idx + 8 : (query.StartsWith("format=") ? 7 : -1);

            if (idx > 0)
            {
                var end = query.IndexOf('&', idx);
                if (end < 0 || end > idx) extension = query.Substring(idx, (end > 0 ? end : query.Length) - idx);
            }
            using (var hasher = new SHA256Managed())
            {
                query = Convert.ToBase64String(hasher.ComputeHash(_encoding.GetBytes(query))).Replace('+', '.').Replace('/', '-').Replace('=', '_');
            }
            relativePath = relativePath.Substring(0, relativePath.Length - Path.GetExtension(relativePath).Length) + $"-{query}{extension}";

            var path = Path.GetFullPath(Path.Combine(ctx.BasePath, relativePath));
            if (System.IO.File.Exists(path)) return relativePath;

            try
            {
                using (var client = new WebClient())
                {
                    client.DownloadFile(url, path);

                    return relativePath;
                }
            }
            catch (Exception ex)
            {
                _logger.Warn<ImageProcessorExtension>(ex, "Could not retrieve the image {src}", url.PathAndQuery);
            }

            return null;
        }

        private class Context
        {
            public Context(Uri baseUri, string basePath)
            {
                BaseUri = baseUri;
                BasePath = basePath;
            }

            public Uri BaseUri { get; }
            public string BasePath { get; }
        }
    }
}
