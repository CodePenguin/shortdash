﻿using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using ShortDash.Server.Actions;
using ShortDash.Server.Data;
using ShortDash.Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShortDash.Server.Components
{
    public partial class DashboardGridCell : ComponentBase
    {
        [Parameter]
        public DashboardCell Cell { get; set; }

        [Parameter]
        public bool EditMode { get; set; }

        [Parameter]
        public EventCallback<DashboardCell> OnMoveCellLeft { get; set; }

        [Parameter]
        public EventCallback<DashboardCell> OnMoveCellRight { get; set; }

        [Parameter]
        public EventCallback<DashboardCell> OnRemoveCell { get; set; }

        protected Task MoveCellLeft()
        {
            return OnMoveCellLeft.InvokeAsync(Cell);
        }

        protected Task MoveCellRight()
        {
            return OnMoveCellRight.InvokeAsync(Cell);
        }

        protected Task RemoveCell()
        {
            return OnRemoveCell.InvokeAsync(Cell);
        }
    }
}