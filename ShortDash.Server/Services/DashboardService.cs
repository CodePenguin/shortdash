using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ShortDash.Server.Data;
using System;
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
            dashboardActionTarget.DashboardActionTargetId = GenerateDashboardActionTargetId();
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

        public async Task<DashboardDevice> AddDashboardDeviceAsync(DashboardDevice dashboardDevice)
        {
            dbContext.Add(dashboardDevice);
            await dbContext.SaveChangesAsync();
            return dashboardDevice;
        }

        public async Task<DashboardDevice> DeleteDashboardActionAsync(DashboardDevice dashboardDevice)
        {
            dbContext.Remove(dashboardDevice);
            await dbContext.SaveChangesAsync();
            return dashboardDevice;
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

        public async Task<DashboardActionTarget> GetDashboardActionTargetAsync(string dashboardActionTargetId)
        {
            return await dbContext.DashboardActionTargets
                .Where(t => t.DashboardActionTargetId == dashboardActionTargetId)
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
                .ThenInclude(a => a.DashboardSubActionChildren)
                .ThenInclude(sa => sa.DashboardActionChild)
                .Where(d => d.DashboardId == dashboardId)
                .OrderBy(d => d.Name)
                .FirstOrDefaultAsync();
        }

        public async Task<DashboardDevice> GetDashboardDeviceAsync(string dashboardDeviceId)
        {
            return await dbContext.DashboardDevices
                .Where(d => d.DashboardDeviceId == dashboardDeviceId)
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

        public async Task<DashboardDevice> UpdateDashboardDeviceAsync(DashboardDevice dashboardDevice)
        {
            dbContext.Update(dashboardDevice);
            await dbContext.SaveChangesAsync();
            return dashboardDevice;
        }

        private string GenerateDashboardActionTargetId()
        {
            const string characterSpace = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var r = new Random();
            var attempts = 1000;
            while (attempts > 0)
            {
                var targetId = "";
                for (var i = 0; i < 6; i++)
                {
                    targetId += characterSpace[r.Next(characterSpace.Length - 1)];
                }
                var target = dbContext.DashboardActionTargets.Where(t => t.DashboardActionTargetId == targetId).FirstOrDefault();
                if (target == null)
                {
                    return targetId;
                }
                attempts -= 1;
            }
            throw new Exception("Failed to generate new Target ID");
        }
    }
}