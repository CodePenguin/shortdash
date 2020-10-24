using ShortDash.Core.Plugins;
using System.ComponentModel.DataAnnotations;

namespace ShortDash.Plugins.Core.Common
{
    [ShortDashAction(
            Title = "Toggle Web Request",
            Description = "Execute web requests based on if the action is toggled.",
            ParametersType = typeof(ToggleWebRequestParameters),
            Toggle = true)]
    public class ToggleWebRequestAction : WebRequestActionBase, IShortDashAction
    {
        public ToggleWebRequestAction(IShortDashPluginLogger<WebRequestAction> logger) : base(logger)
        {
        }

        public ShortDashActionResult Execute(object parametersObject, bool toggleState)
        {
            var parameters = parametersObject as ToggleWebRequestParameters;
            var url = toggleState ? parameters.ToggleUrl : parameters.Url;
            var method = toggleState ? parameters.ToggleMethod : parameters.Method;
            var contentType = toggleState ? parameters.ToggleContentType : parameters.ContentType;
            var data = toggleState ? parameters.ToggleData : parameters.ToggleData;
            var result = ExecuteWebRequest(url, method, contentType, data);
            result.ToggleState = !toggleState;
            return result;
        }
    }

    public class ToggleWebRequestParameters : WebRequestParameters
    {
        [Display(
            Name = "Toggle Content Type",
            Order = 7)]
        public string ToggleContentType { get; set; } = "";

        [Display(
            Name = "Toggle Data",
            Order = 8)]
        [FormInput(TypeName = "InputTextArea")]
        public string ToggleData { get; set; } = null;

        [Display(
            Name = "Toggle Method",
            Order = 6)]
        public string ToggleMethod { get; set; } = "GET";

        [Required]
        [Display(
            Name = "Toggle URL",
            Order = 5)]
        public string ToggleUrl { get; set; }
    }
}
