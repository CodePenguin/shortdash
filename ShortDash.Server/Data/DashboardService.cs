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

        public async Task<List<DashboardAction>> GetDashboardActionsAsync()
        {
            return await dbContext.DashboardActions.ToListAsync();
        }
    }
}
