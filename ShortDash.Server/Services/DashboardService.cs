using Microsoft.EntityFrameworkCore;
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
                .Where(a => a.DashboardActionId == dashboardActionId)
                .FirstOrDefaultAsync();
        }

        public async Task<List<DashboardAction>> GetDashboardActionsAsync()
        {
            return await dbContext.DashboardActions
                .OrderBy(a => a.Title)
                .ToListAsync();
        }

        public async Task<Dashboard> GetDashboardAsync(int dashboardId)
        {
            return await dbContext.Dashboards
                .Include(d => d.DashboardCells)
                .ThenInclude(c => c.DashboardAction)
                .Where(d => d.DashboardId == dashboardId)
                .OrderBy(d => d.Title)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Dashboard>> GetDashboardsAsync()
        {
            return await dbContext.Dashboards.ToListAsync();
        }

        public async Task<DashboardAction> UpdateDashboardActionAsync(DashboardAction dashboardAction)
        {
            dbContext.Update(dashboardAction);
            await dbContext.SaveChangesAsync();
            return dashboardAction;
        }

        public async Task<Dashboard> UpdateDashboardAsync(Dashboard dashboard)
        {
            dbContext.Update(dashboard);
            await dbContext.SaveChangesAsync();
            return dashboard;
        }
    }
}