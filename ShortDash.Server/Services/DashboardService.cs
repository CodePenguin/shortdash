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
        private readonly DataSignatureManager dataSignatureManager;
        private readonly ApplicationDbContextFactory dbContextFactory;

        public DashboardService(ApplicationDbContextFactory dbContextFactory, DataSignatureManager dataSignatureManager)
        {
            this.dbContextFactory = dbContextFactory;
            this.dataSignatureManager = dataSignatureManager;
        }

        public async Task<DashboardAction> AddDashboardActionAsync(DashboardAction dashboardAction)
        {
            using var dbContext = dbContextFactory.CreateDbContext();
            dataSignatureManager.GenerateSignature(dashboardAction);
            dbContext.Add(dashboardAction);
            await dbContext.SaveChangesAsync();
            return dashboardAction;
        }

        public async Task<DashboardActionTarget> AddDashboardActionTargetAsync(DashboardActionTarget dashboardActionTarget)
        {
            using var dbContext = dbContextFactory.CreateDbContext();
            dataSignatureManager.GenerateSignature(dashboardActionTarget);
            dbContext.Add(dashboardActionTarget);
            await dbContext.SaveChangesAsync();
            return dashboardActionTarget;
        }

        public async Task<Dashboard> AddDashboardAsync(Dashboard dashboard)
        {
            using var dbContext = dbContextFactory.CreateDbContext();
            dbContext.Add(dashboard);
            await dbContext.SaveChangesAsync();
            return dashboard;
        }

        public async Task<DashboardDevice> AddDashboardDeviceAsync(DashboardDevice dashboardDevice)
        {
            using var dbContext = dbContextFactory.CreateDbContext();
            dataSignatureManager.GenerateSignature(dashboardDevice);
            dbContext.Add(dashboardDevice);
            await dbContext.SaveChangesAsync();
            return dashboardDevice;
        }

        public async Task<DashboardDevice> DeleteDashboardActionAsync(DashboardDevice dashboardDevice)
        {
            using var dbContext = dbContextFactory.CreateDbContext();
            dbContext.Remove(dashboardDevice);
            await dbContext.SaveChangesAsync();
            return dashboardDevice;
        }

        public async Task<DashboardAction> DeleteDashboardActionAsync(DashboardAction dashboardAction)
        {
            using var dbContext = dbContextFactory.CreateDbContext();
            dbContext.Remove(dashboardAction);
            await dbContext.SaveChangesAsync();
            return dashboardAction;
        }

        public async Task<DashboardActionTarget> DeleteDashboardActionTargetAsync(DashboardActionTarget dashboardActionTarget)
        {
            using var dbContext = dbContextFactory.CreateDbContext();
            var targetedActions = await dbContext.DashboardActions
                .Where(a => a.DashboardActionTargetId == dashboardActionTarget.DashboardActionTargetId)
                .ToListAsync();
            foreach (var action in targetedActions)
            {
                action.DashboardActionTargetId = DashboardActionTarget.ServerTargetId;
                dataSignatureManager.GenerateSignature(action);
                dbContext.Update(action);
            }
            dbContext.Remove(dashboardActionTarget);
            await dbContext.SaveChangesAsync();
            return dashboardActionTarget;
        }

        public async Task<Dashboard> DeleteDashboardAsync(Dashboard dashboard)
        {
            using var dbContext = dbContextFactory.CreateDbContext();
            dbContext.Remove(dashboard);
            await dbContext.SaveChangesAsync();
            return dashboard;
        }

        public async Task<DashboardCell> DeleteDashboardCellAsync(DashboardCell dashboardCell)
        {
            using var dbContext = dbContextFactory.CreateDbContext();
            dbContext.Remove(dashboardCell);
            await dbContext.SaveChangesAsync();
            return dashboardCell;
        }

        public async Task<DashboardDevice> DeleteDashboardDeviceAsync(DashboardDevice dashboardDevice)
        {
            using var dbContext = dbContextFactory.CreateDbContext();
            dbContext.Remove(dashboardDevice);
            await dbContext.SaveChangesAsync();
            return dashboardDevice;
        }

        public Task<DashboardAction> GetDashboardActionAsync(int dashboardActionId)
        {
            using var dbContext = dbContextFactory.CreateDbContext();
            return dbContext.DashboardActions
                .Include(a => a.DashboardSubActionChildren)
                .ThenInclude(c => c.DashboardActionChild)
                .Where(a => a.DashboardActionId == dashboardActionId)
                .FirstOrDefaultAsync();
        }

        public async Task<DashboardAction> GetDashboardActionCopyAsync(int dashboardActionId)
        {
            using var dbContext = dbContextFactory.CreateDbContext();
            var action = await GetDashboardActionAsync(dashboardActionId);
            if (action == null)
            {
                return null;
            }
            dbContext.Entry(action).State = EntityState.Detached;
            action.DashboardActionId = 0;
            foreach (var child in action.DashboardSubActionChildren)
            {
                dbContext.Entry(child).State = EntityState.Detached;
                child.DashboardActionParentId = 0;
                child.DashboardActionParent = null;
            }
            return action;
        }

        public Task<List<DashboardAction>> GetDashboardActionsAsync()
        {
            using var dbContext = dbContextFactory.CreateDbContext();
            return dbContext.DashboardActions
                .OrderBy(a => a.Label)
                .ToListAsync();
        }

        public Task<DashboardActionTarget> GetDashboardActionTargetAsync(string dashboardActionTargetId)
        {
            using var dbContext = dbContextFactory.CreateDbContext();
            return dbContext.DashboardActionTargets
                .Where(t => t.DashboardActionTargetId == dashboardActionTargetId)
                .FirstOrDefaultAsync();
        }

        public Task<List<DashboardActionTarget>> GetDashboardActionTargetsAsync()
        {
            using var dbContext = dbContextFactory.CreateDbContext();
            return dbContext.DashboardActionTargets.ToListAsync();
        }

        public Task<Dashboard> GetDashboardAsync(int dashboardId)
        {
            using var dbContext = dbContextFactory.CreateDbContext();
            return dbContext.Dashboards
                .Include(d => d.DashboardCells)
                .ThenInclude(c => c.DashboardAction)
                .ThenInclude(a => a.DashboardSubActionChildren)
                .ThenInclude(sa => sa.DashboardActionChild)
                .Where(d => d.DashboardId == dashboardId)
                .OrderBy(d => d.Name)
                .FirstOrDefaultAsync();
        }

        public Task<DashboardDevice> GetDashboardDeviceAsync(string dashboardDeviceId)
        {
            using var dbContext = dbContextFactory.CreateDbContext();
            return dbContext.DashboardDevices
                .Where(d => d.DashboardDeviceId == dashboardDeviceId)
                .FirstOrDefaultAsync();
        }

        public Task<List<DashboardDevice>> GetDashboardDevicesAsync()
        {
            using var dbContext = dbContextFactory.CreateDbContext();
            return dbContext.DashboardDevices.ToListAsync();
        }

        public Task<List<Dashboard>> GetDashboardsAsync()
        {
            using var dbContext = dbContextFactory.CreateDbContext();
            return dbContext.Dashboards.ToListAsync();
        }

        public async Task<DashboardAction> UpdateDashboardActionAsync(DashboardAction dashboardAction, List<DashboardSubAction> subActionRemovalList = null)
        {
            using var dbContext = dbContextFactory.CreateDbContext();
            if (subActionRemovalList != null)
            {
                foreach (var subAction in subActionRemovalList)
                {
                    dbContext.Remove(subAction);
                }
            }
            dataSignatureManager.GenerateSignature(dashboardAction);
            dbContext.Update(dashboardAction);
            await dbContext.SaveChangesAsync();
            return dashboardAction;
        }

        public async Task<DashboardActionTarget> UpdateDashboardActionTargetAsync(DashboardActionTarget dashboardActionTarget)
        {
            using var dbContext = dbContextFactory.CreateDbContext();
            dataSignatureManager.GenerateSignature(dashboardActionTarget);
            dbContext.Update(dashboardActionTarget);
            await dbContext.SaveChangesAsync();
            return dashboardActionTarget;
        }

        public async Task<Dashboard> UpdateDashboardAsync(Dashboard dashboard, List<DashboardCell> cellRemovalList = null)
        {
            using var dbContext = dbContextFactory.CreateDbContext();
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
            using var dbContext = dbContextFactory.CreateDbContext();
            dataSignatureManager.GenerateSignature(dashboardDevice);
            dbContext.Update(dashboardDevice);
            await dbContext.SaveChangesAsync();
            return dashboardDevice;
        }

        public bool VerifySignature(object data)
        {
            return dataSignatureManager.VerifySignature(data);
        }
    }
}