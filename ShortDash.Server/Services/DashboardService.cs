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
        private readonly ApplicationDbContext dbContext;

        public DashboardService(ApplicationDbContext dbContext, DataSignatureManager dataSignatureManager)
        {
            this.dbContext = dbContext;
            this.dataSignatureManager = dataSignatureManager;
        }

        public async Task<DashboardAction> AddDashboardActionAsync(DashboardAction dashboardAction)
        {
            dataSignatureManager.GenerateSignature(dashboardAction);
            dbContext.Add(dashboardAction);
            await dbContext.SaveChangesAsync();
            return dashboardAction;
        }

        public async Task<DashboardActionTarget> AddDashboardActionTargetAsync(DashboardActionTarget dashboardActionTarget)
        {
            dashboardActionTarget.DashboardActionTargetId = GenerateDashboardActionTargetId();
            dataSignatureManager.GenerateSignature(dashboardActionTarget);
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
            dataSignatureManager.GenerateSignature(dashboardDevice);
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

        public async Task<DashboardDevice> DeleteDashboardDeviceAsync(DashboardDevice dashboardDevice)
        {
            dbContext.Remove(dashboardDevice);
            await dbContext.SaveChangesAsync();
            return dashboardDevice;
        }

        public string GetConfigurationSection(string configurationSectionId)
        {
            var configurationSection = dbContext.ConfigurationSections
                .Where(s => s.ConfigurationSectionId == configurationSectionId)
                .FirstOrDefault();
            return configurationSection?.Data;
        }

        public Task<DashboardAction> GetDashboardActionAsync(int dashboardActionId)
        {
            return dbContext.DashboardActions
                .Include(a => a.DashboardSubActionChildren)
                .ThenInclude(c => c.DashboardActionChild)
                .Where(a => a.DashboardActionId == dashboardActionId)
                .FirstOrDefaultAsync();
        }

        public async Task<DashboardAction> GetDashboardActionCopyAsync(int dashboardActionId)
        {
            var action = await GetDashboardActionAsync(dashboardActionId);
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
            return dbContext.DashboardActions
                .OrderBy(a => a.Label)
                .ToListAsync();
        }

        public Task<DashboardActionTarget> GetDashboardActionTargetAsync(string dashboardActionTargetId)
        {
            return dbContext.DashboardActionTargets
                .Where(t => t.DashboardActionTargetId == dashboardActionTargetId)
                .FirstOrDefaultAsync();
        }

        public Task<List<DashboardActionTarget>> GetDashboardActionTargetsAsync()
        {
            return dbContext.DashboardActionTargets.ToListAsync();
        }

        public Task<Dashboard> GetDashboardAsync(int dashboardId)
        {
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
            return dbContext.DashboardDevices
                .Where(d => d.DashboardDeviceId == dashboardDeviceId)
                .FirstOrDefaultAsync();
        }

        public Task<List<DashboardDevice>> GetDashboardDevicesAsync()
        {
            return dbContext.DashboardDevices.ToListAsync();
        }

        public Task<List<Dashboard>> GetDashboardsAsync()
        {
            return dbContext.Dashboards.ToListAsync();
        }

        public void SetConfigurationSection(string configurationSectionId, string data)
        {
            var configurationSection = dbContext.ConfigurationSections
                .Where(s => s.ConfigurationSectionId == configurationSectionId)
                .FirstOrDefault();
            if (configurationSection == null)
            {
                configurationSection = new ConfigurationSection
                {
                    ConfigurationSectionId = configurationSectionId,
                    Data = data
                };
                dbContext.Add(configurationSection);
            }
            else
            {
                configurationSection.Data = data;
                dbContext.Update(configurationSection);
            }
            dbContext.SaveChanges();
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
            dataSignatureManager.GenerateSignature(dashboardAction);
            dbContext.Update(dashboardAction);
            await dbContext.SaveChangesAsync();
            return dashboardAction;
        }

        public async Task<DashboardActionTarget> UpdateDashboardActionTargetAsync(DashboardActionTarget dashboardActionTarget)
        {
            dataSignatureManager.GenerateSignature(dashboardActionTarget);
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
            dataSignatureManager.GenerateSignature(dashboardDevice);
            dbContext.Update(dashboardDevice);
            await dbContext.SaveChangesAsync();
            return dashboardDevice;
        }

        public bool VerifySignature(object data)
        {
            return dataSignatureManager.VerifySignature(data);
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