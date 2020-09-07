using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using ShortDash.Server.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;

namespace ShortDash.Server.Components
{
    public partial class DashboardGrid : ComponentBase
    {
        [Parameter]
        public Dashboard Dashboard { get; set; }

        [Parameter]
        public bool EditMode { get; set; } = true;

        [CascadingParameter]
        public IModalService ModalService { get; set; }

        [Parameter]
        public EventCallback<DashboardCell> OnRemoveCell { get; set; }

        [Parameter]
        public EventCallback OnSaveChanges { get; set; }

        private List<DashboardCell> DashboardCells { get; set; } = new List<DashboardCell>();

        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            DashboardCells.Clear();
            DashboardCells.AddRange(Dashboard.DashboardCells.OrderBy(c => c.Sequence).ThenBy(c => c.DashboardCellId).ToList());
        }

        protected async void ShowAddDialog()
        {
            var result = await AddDashboardActionDialog.ShowAsync(ModalService);
            if (result.Cancelled) { return; }
            Dashboard.DashboardCells.Add(new DashboardCell { DashboardActionId = (int)result.Data });
            await SaveChanges();
        }

        private void MoveCellLeft(DashboardCell cell)
        {
            var index = DashboardCells.IndexOf(cell);
            if (index < 1) { return; }
            DashboardCells[index] = DashboardCells[index - 1];
            DashboardCells[index - 1] = cell;
            SaveChanges();
        }

        private void MoveCellRight(DashboardCell cell)
        {
            var index = DashboardCells.IndexOf(cell);
            if (index >= DashboardCells.Count - 1) { return; }
            DashboardCells[index] = DashboardCells[index + 1];
            DashboardCells[index + 1] = cell;
            SaveChanges();
        }

        private void RemoveCell(DashboardCell cell)
        {
            OnRemoveCell.InvokeAsync(cell);
            DashboardCells.Remove(cell);
        }

        private Task SaveChanges()
        {
            for (var i = 0; i < DashboardCells.Count; i++)
            {
                DashboardCells[i].Sequence = i;
            }
            return OnSaveChanges.InvokeAsync(null);
        }
    }
}