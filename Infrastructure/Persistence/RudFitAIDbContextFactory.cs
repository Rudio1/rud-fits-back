using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Options;
using RudFitAI.Application.Options;

namespace RudFitAI.Infrastructure.Persistence;

public sealed class RudFitAIDbContextFactory : IDesignTimeDbContextFactory<RudFitAIDbContext>
{
    public RudFitAIDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<RudFitAIDbContext> builder = new();
        builder.UseSqlServer(
            "Server=(localdb)\\mssqllocaldb;Database=RudFitAI;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True");

        return new RudFitAIDbContext(builder.Options, Options.Create(new PersistenceOptions()));
    }
}
