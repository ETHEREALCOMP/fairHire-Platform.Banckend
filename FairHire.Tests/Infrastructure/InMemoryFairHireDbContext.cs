using FairHire.Infrastructure.Postgres;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FairHire.Tests.Infrastructure
{
    public static class InMemoryFairHireDbContext
    {
        public static FairHireDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<FairHireDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            return new FairHireDbContext(options);
        }

    }

}
