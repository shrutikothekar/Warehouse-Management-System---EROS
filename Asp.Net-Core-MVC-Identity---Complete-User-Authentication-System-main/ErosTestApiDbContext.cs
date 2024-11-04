using AuthSystem.Areas.Identity.Data;
using AuthSystem.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace eros.Areas.Identity.Data
{
    public class ErosTestApiDbContext : IdentityDbContext<ApplicationUser>
    {
        internal object OtherEntity;

        public ErosTestApiDbContext(DbContextOptions<ErosTestApiDbContext> options)
            : base(options)
        {
        }

        public ErosTestApiDbContext()
        {
        }

        internal Task savechangesasync()
        {
            throw new NotImplementedException();
        }
        public DbSet<eros.Models.Product_Master> Product_Master { get; set; } = default!;
        public DbSet<eros.Models.Storage_Operation> Storage_Operation { get; set; } = default!;

    }
}
