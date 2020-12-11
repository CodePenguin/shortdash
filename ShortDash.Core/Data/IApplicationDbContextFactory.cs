namespace ShortDash.Core.Data
{
    public interface IApplicationDbContextFactory
    {
        public ApplicationDbContext CreateDbContext();
    }
}
