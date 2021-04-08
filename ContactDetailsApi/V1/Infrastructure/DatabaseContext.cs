using Microsoft.EntityFrameworkCore;

namespace ContactDetailsApi.V1.Infrastructure
{

    public class DatabaseContext : DbContext
    {
        //TODO: rename DatabaseContext to reflect the data source it is representing. eg. MosaicContext.
        //Guidance on the context class can be found here https://github.com/LBHackney-IT/lbh-contact-details-api/wiki/DatabaseContext
        public DatabaseContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<ContactDetailsEntity> DatabaseEntities { get; set; }
    }
}
