using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;

namespace Umbraco.Web.Routing
{
    public class PublishedRequestBuilder : IPublishedRequestBuilder
    {
        private readonly IFileService _fileService;
        private IReadOnlyDictionary<string, string> _headers;
        private bool _cacheability;
        private IReadOnlyList<string> _cacheExtensions;
        private string _redirectUrl;
        private HttpStatusCode? _responseStatus;
        private IPublishedContent _publishedContent;
        private bool _ignorePublishedContentCollisions;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedRequestBuilder"/> class.
        /// </summary>
        public PublishedRequestBuilder(Uri uri, IFileService fileService)
        {
            Uri = uri;
            _fileService = fileService;
        }

        /// <inheritdoc/>
        public Uri Uri { get; }

        /// <inheritdoc/>
        public DomainAndUri Domain { get; private set; }

        /// <inheritdoc/>
        public string Culture { get; private set; }

        /// <inheritdoc/>
        public ITemplate Template { get; private set; }

        /// <inheritdoc/>
        public bool IsInternalRedirect { get; private set; }

        /// <inheritdoc/>
        public int? ResponseStatusCode => _responseStatus.HasValue ? (int?)_responseStatus : null;

        /// <inheritdoc/>
        public IPublishedContent PublishedContent
        {
            get => _publishedContent;
            private set
            {
                _publishedContent = value;
                IsInternalRedirect = false;
                Template = null;
            }
        }

        /// <inheritdoc/>
        public IPublishedRequest Build() => new PublishedRequest(
                Uri,
                PublishedContent,
                IsInternalRedirect,
                Template,
                Domain,
                Culture,
                _redirectUrl,
                _responseStatus.HasValue ? (int?)_responseStatus : null,
                _cacheExtensions,
                _headers,
                _cacheability,
                _ignorePublishedContentCollisions);

        /// <inheritdoc/>
        public IPublishedRequestBuilder SetNoCacheHeader(bool cacheability)
        {
            _cacheability = cacheability;
            return this;
        }

        /// <inheritdoc/>
        public IPublishedRequestBuilder SetCacheExtensions(IEnumerable<string> cacheExtensions)
        {
            _cacheExtensions = cacheExtensions.ToList();
            return this;
        }

        /// <inheritdoc/>
        public IPublishedRequestBuilder SetCulture(string culture)
        {
            Culture = culture;
            return this;
        }

        /// <inheritdoc/>
        public IPublishedRequestBuilder SetDomain(DomainAndUri domain)
        {
            Domain = domain;
            SetCulture(domain.Culture);
            return this;
        }

        /// <inheritdoc/>
        public IPublishedRequestBuilder SetHeaders(IReadOnlyDictionary<string, string> headers)
        {
            _headers = headers;
            return this;
        }

        /// <inheritdoc/>
        public IPublishedRequestBuilder SetInternalRedirect(IPublishedContent content)
        {
            // unless a template has been set already by the finder,
            // template should be null at that point.

            // redirecting to self
            if (PublishedContent != null && content.Id == PublishedContent.Id)
            {
                // no need to set PublishedContent, we're done
                IsInternalRedirect = true;
                return this;
            }

            // else

            // set published content - this resets the template, and sets IsInternalRedirect to false
            PublishedContent = content;
            IsInternalRedirect = true;

            return this;
        }

        /// <inheritdoc/>
        public IPublishedRequestBuilder SetPublishedContent(IPublishedContent content)
        {
            PublishedContent = content;
            IsInternalRedirect = false;
            return this;
        }

        /// <inheritdoc/>
        public IPublishedRequestBuilder SetRedirect(string url, int status = (int)HttpStatusCode.Redirect)
        {
            _redirectUrl = url;
            _responseStatus = (HttpStatusCode)status;
            return this;
        }

        /// <inheritdoc/>
        public IPublishedRequestBuilder SetRedirectPermanent(string url)
        {
            _redirectUrl = url;
            _responseStatus = HttpStatusCode.Moved;
            return this;
        }

        /// <inheritdoc/>
        public IPublishedRequestBuilder SetResponseStatus(int code)
        {
            _responseStatus = (HttpStatusCode)code;
            return this;
        }

        /// <inheritdoc/>
        public IPublishedRequestBuilder SetTemplate(ITemplate template)
        {
            Template = template;
            return this;
        }

        /// <inheritdoc/>
        public bool TrySetTemplate(string alias)
        {
            if (string.IsNullOrWhiteSpace(alias))
            {
                Template = null;
                return true;
            }

            // NOTE - can we still get it with whitespaces in it due to old legacy bugs?
            alias = alias.Replace(" ", string.Empty);

            ITemplate model = _fileService.GetTemplate(alias);
            if (model == null)
            {
                return false;
            }

            Template = model;
            return true;
        }

        /// <inheritdoc/>
        public void IgnorePublishedContentCollisions() => _ignorePublishedContentCollisions = true;
    }
}
