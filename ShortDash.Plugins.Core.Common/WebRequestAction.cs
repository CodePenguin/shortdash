using ShortDash.Core.Plugins;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net;

namespace ShortDash.Plugins.Core.Common
{
    [ShortDashAction(
            Title = "Web Request",
            Description = "Execute a web request.",
            ParametersType = typeof(WebRequestParameters))]
    public class WebRequestAction : IShortDashAction
    {
        private readonly IShortDashPluginLogger<WebRequestAction> logger;

        public WebRequestAction(IShortDashPluginLogger<WebRequestAction> logger)
        {
            this.logger = logger;
        }

        public bool Execute(object parametersObject, ref bool toggleState)
        {
            var parameters = parametersObject as WebRequestParameters;
            try
            {
                var request = WebRequest.Create(parameters.Url);
                request.Method = parameters.Method?.ToUpper();
                request.ContentType = !string.IsNullOrWhiteSpace(parameters.ContentType) ? parameters.ContentType : GetDefaultContentType(request.Method);
                if (!string.IsNullOrEmpty(parameters.Data))
                {
                    using var writer = new StreamWriter(request.GetRequestStream());
                    writer.Write(parameters.Data);
                }
                request.GetResponse();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError($"Web Request Error: {ex.Message}");
                return false;
            }
        }

        private string GetDefaultContentType(string method)
        {
            return !method.Equals("GET") ? "application/x-www-form-urlencoded" : "";
        }
    }

    public class WebRequestParameters
    {
        [Display(
            Name = "Content Type",
            Order = 3)]
        public string ContentType { get; set; } = "";

        [Display(
            Name = "Data",
            Order = 4)]
        public string Data { get; set; } = null;

        [Display(
            Name = "Method",
            Order = 2)]
        public string Method { get; set; } = "GET";

        [Required]
        [Display(
            Name = "URL",
            Order = 1)]
        public string Url { get; set; }
    }
}
