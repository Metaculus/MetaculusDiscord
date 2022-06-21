using Microsoft.EntityFrameworkCore;
using MetaculusDiscord.Model;
using Microsoft.Extensions.Configuration;

namespace MetaculusDiscord.Data;

/// <summary>
/// Contains DbSets for objects that are stored in the database.
/// </summary>
public class MetaculusContext : DbContext
{
    public MetaculusContext(DbContextOptions<MetaculusContext> options) : base(options)
    {
    }

    public DbSet<UserQuestionAlert> UserQuestionAlerts { get; set; }
    public DbSet<ChannelQuestionAlert> ChannelQuestionAlerts { get; set; }


    public class MetaculusContextFactory : IDbContextFactory<MetaculusContext>
    {
        private readonly IConfiguration _config;

        public MetaculusContextFactory()
        {
            _config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .Build();
        }

        public MetaculusContext CreateDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<MetaculusContext>();
            optionsBuilder.UseNpgsql(_config.GetConnectionString("Default"));
            return new MetaculusContext(optionsBuilder.Options);
        }
    }
}