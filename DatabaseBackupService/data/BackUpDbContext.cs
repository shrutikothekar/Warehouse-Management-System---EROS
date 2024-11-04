using System.Collections.Generic;
using DatabaseBackupService.data;

using Microsoft.AspNetCore.Identity;

using Microsoft.EntityFrameworkCore;

namespace DatabaseBackupService.data
{
    public class BackUpDbContext : DbContext
    {
        public BackUpDbContext(DbContextOptions<BackUpDbContext> options)
            : base(options)
        {
        }
        //public DbSet<DatabaseBackupService.Models.supplier_master> supplier_master { get; set; } = default!;
    }




}
