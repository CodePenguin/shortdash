using ShortDash.Core.Plugins;
using System.ComponentModel.DataAnnotations;

namespace ShortDash.Plugins.Core.Common
{
    [ShortDashAction(
            Title = "Web Request",
            Description = "Execute a web request.",
            ParametersType = typeof(WebRequestParameters))]
    public class WebRequestAction : WebRequestActionBase, IShortDashAction
    {
        public WebRequestAction(IShortDashPluginLogger<WebRequestAction> logger) : base(logger)
        {
        }

        public ShortDashActionResult Execute(object parametersObject, bool toggleState)
        {
            var parameters = parametersObject as WebRequestParameters;
            return ExecuteWebRequest(parameters.Url, parameters.Method, parameters.ContentType, parameters.Data);
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
        [FormInput(TypeName = "TextArea")]
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
