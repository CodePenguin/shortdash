using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShortDash.Server.Data
{
    public class DashboardService
    {
        private ApplicationDbContext dbContext;

        public DashboardService(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Dashboard> AddDashboardAsync(Dashboard dashboard)
        {
            dbContext.Add(dashboard);
            await dbContext.SaveChangesAsync();
            return dashboard;
        }

        public async Task<Dashboard> GetDashboardAsync(int dashboardId)
        {
            return await dbContext.Dashboards.FirstOrDefaultAsync(d => d.DashboardId == dashboardId);
        }

        public async Task<List<Dashboard>> GetDashboardsAsync()
        {
            return await dbContext.Dashboards.ToListAsync();
        }

        public async Task<DashboardAction> AddDashboardActionAsync(DashboardAction dashboardAction)
        {
            dbContext.Add(dashboardAction);
            await dbContext.SaveChangesAsync();
            return dashboardAction;
        }

        public async Task<DashboardAction> DeleteDashboardActionAsync(DashboardAction dashboardAction)
        {
            dbContext.Remove(dashboardAction);
            await dbContext.SaveChangesAsync();
            return dashboardAction;
        }

        public async Task<List<DashboardAction>> GetDashboardActionsAsync()
        {
            return await dbContext.DashboardActions.ToListAsync();
        }

        public async Task<DashboardAction> GetDashboardActionAsync(int dashboardActionId)
        {
            return await dbContext.DashboardActions.FirstOrDefaultAsync(d => d.DashboardActionId == dashboardActionId);
        }

        public async Task<DashboardAction> UpdateDashboardActionAsync(DashboardAction dashboardAction)
        {
            dbContext.Update(dashboardAction);
            await dbContext.SaveChangesAsync();
            return dashboardAction;
        }
    }
}
