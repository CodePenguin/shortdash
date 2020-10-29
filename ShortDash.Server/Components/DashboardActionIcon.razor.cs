using Microsoft.AspNetCore.Components;
using ShortDash.Server.Actions;
using ShortDash.Server.Data;
using ShortDash.Server.Extensions;
using ShortDash.Server.Services;
using ShortDash.Server.Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ShortDash.Server.Components
{
    public partial class DashboardActionIcon : ComponentBase
    {
        private bool _toggleState;

        private Color BackgroundColor => ToggleState ? DashboardAction.ToggleBackgroundColor : DashboardAction.BackgroundColor;

        [Parameter]
        public DashboardAction DashboardAction { get; set; }

        [Parameter]
        public bool EditMode { get; set; }

        private string Icon => ToggleState ? DashboardAction.ToggleIcon : DashboardAction.Icon;

        [Parameter]
        public bool IsExecuting { get; set; }

        private string Label => ToggleState ? DashboardAction.ToggleLabel : DashboardAction.Label;

        [Parameter]
        public bool ToggleState
        {
            get => _toggleState;
            set
            {
                _toggleState = value;
                RefreshStyles();
            }
        }

        private Dictionary<string, object> CellAttributes { get; set; } = new Dictionary<string, object>();

        private string TextClass { get; set; }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            RefreshStyles();
        }

        private bool HasIcon()
        {
            var uri = new Uri(new Uri("http://localhost"), Icon);
            return !string.IsNullOrWhiteSpace(Icon) && string.IsNullOrEmpty(Path.GetExtension(uri.LocalPath));
        }

        private bool HasImage()
        {
            return !string.IsNullOrWhiteSpace(Icon);
        }

        private bool IsExecutable(DashboardAction action)
        {
            var actionTypeName = action?.ActionTypeName;
            return !string.IsNullOrWhiteSpace(actionTypeName) && actionTypeName != typeof(DashSeparatorAction).FullName;
        }

        private void RefreshStyles()
        {
            CellAttributes.Clear();
            if (DashboardAction.BackgroundColor != null)
            {
                CellAttributes.Add("style", "background-color: " + BackgroundColor.ToHtmlString());
                TextClass = BackgroundColor.TextClass();
            }
            else
            {
                TextClass = "dark";
            }
        }
    }
}