﻿using Microsoft.EntityFrameworkCore;
using ShortDash.Server.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShortDash.Server.Services
{
    public class DashboardService
    {
        private readonly ApplicationDbContext dbContext;

        public DashboardService(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<DashboardAction> AddDashboardActionAsync(DashboardAction dashboardAction)
        {
            dbContext.Add(dashboardAction);
            await dbContext.SaveChangesAsync();
            return dashboardAction;
        }

        public async Task<DashboardActionTarget> AddDashboardActionTargetAsync(DashboardActionTarget dashboardActionTarget)
        {
            dbContext.Add(dashboardActionTarget);
            await dbContext.SaveChangesAsync();
            return dashboardActionTarget;
        }

        public async Task<Dashboard> AddDashboardAsync(Dashboard dashboard)
        {
            dbContext.Add(dashboard);
            await dbContext.SaveChangesAsync();
            return dashboard;
        }

        public async Task<DashboardAction> DeleteDashboardActionAsync(DashboardAction dashboardAction)
        {
            dbContext.Remove(dashboardAction);
            await dbContext.SaveChangesAsync();
            return dashboardAction;
        }

        public async Task<DashboardActionTarget> DeleteDashboardActionTargetAsync(DashboardActionTarget dashboardActionTarget)
        {
            dbContext.Remove(dashboardActionTarget);
            await dbContext.SaveChangesAsync();
            return dashboardActionTarget;
        }

        public async Task<Dashboard> DeleteDashboardAsync(Dashboard dashboard)
        {
            dbContext.Remove(dashboard);
            await dbContext.SaveChangesAsync();
            return dashboard;
        }

        public async Task<DashboardCell> DeleteDashboardCellAsync(DashboardCell dashboardCell)
        {
            dbContext.Remove(dashboardCell);
            await dbContext.SaveChangesAsync();
            return dashboardCell;
        }

        public async Task<DashboardAction> GetDashboardActionAsync(int dashboardActionId)
        {
            return await dbContext.DashboardActions
                .Include(a => a.DashboardSubActionChildren)
                    .ThenInclude(c => c.DashboardActionChild)
                .Where(a => a.DashboardActionId == dashboardActionId)
                .FirstOrDefaultAsync();
        }

        public async Task<List<DashboardAction>> GetDashboardActionsAsync()
        {
            return await dbContext.DashboardActions
                .OrderBy(a => a.Label)
                .ToListAsync();
        }

        public async Task<DashboardActionTarget> GetDashboardActionTargetAsync(int dashboardActionTargetId)
        {
            return await dbContext.DashboardActionTargets
                .Where(a => a.DashboardActionTargetId == dashboardActionTargetId)
                .FirstOrDefaultAsync();
        }

        public async Task<List<DashboardActionTarget>> GetDashboardActionTargetsAsync()
        {
            return await dbContext.DashboardActionTargets.ToListAsync();
        }

        public async Task<Dashboard> GetDashboardAsync(int dashboardId)
        {
            return await dbContext.Dashboards
                .Include(d => d.DashboardCells)
                .ThenInclude(c => c.DashboardAction)
                .Where(d => d.DashboardId == dashboardId)
                .OrderBy(d => d.Name)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Dashboard>> GetDashboardsAsync()
        {
            return await dbContext.Dashboards.ToListAsync();
        }

        public async Task<DashboardAction> UpdateDashboardActionAsync(DashboardAction dashboardAction, List<DashboardSubAction> subActionRemovalList = null)
        {
            if (subActionRemovalList != null)
            {
                foreach (var subAction in subActionRemovalList)
                {
                    dbContext.Remove(subAction);
                }
            }
            dbContext.Update(dashboardAction);
            await dbContext.SaveChangesAsync();
            return dashboardAction;
        }

        public async Task<DashboardActionTarget> UpdateDashboardActionTargetAsync(DashboardActionTarget dashboardActionTarget)
        {
            dbContext.Update(dashboardActionTarget);
            await dbContext.SaveChangesAsync();
            return dashboardActionTarget;
        }

        public async Task<Dashboard> UpdateDashboardAsync(Dashboard dashboard, List<DashboardCell> cellRemovalList = null)
        {
            if (cellRemovalList != null)
            {
                foreach (var cell in cellRemovalList)
                {
                    dbContext.Remove(cell);
                }
            }
            dbContext.Update(dashboard);
            await dbContext.SaveChangesAsync();
            return dashboard;
        }
    }
}