using ShortDash.Core.Plugins;
using System;
using System.IO;
using System.Net;

namespace ShortDash.Plugins.Core.Common
{
    public abstract class WebRequestActionBase
    {
        protected readonly IShortDashPluginLogger logger;

        protected WebRequestActionBase(IShortDashPluginLogger logger)
        {
            this.logger = logger;
        }

        public ShortDashActionResult ExecuteWebRequest(string url, string method, string contentType, string data)
        {
            try
            {
                var request = WebRequest.Create(url);
                request.Method = method?.ToUpper();
                request.ContentType = !string.IsNullOrWhiteSpace(contentType) ? contentType : GetDefaultContentType(method);
                if (!string.IsNullOrEmpty(data))
                {
                    using var writer = new StreamWriter(request.GetRequestStream());
                    writer.Write(data);
                }
                var response = request.GetResponse();
                return new ShortDashActionResult { Success = true };
            }
            catch (Exception ex)
            {
                logger.LogError($"Web Request Error: {ex.Message}");
                return new ShortDashActionResult { UserMessage = ex.Message };
            }
        }

        private static string GetDefaultContentType(string method)
        {
            return !method.Equals("GET") ? "application/x-www-form-urlencoded" : "";
        }
    }
}
