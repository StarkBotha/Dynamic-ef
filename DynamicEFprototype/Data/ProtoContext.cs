using DynamicEFprototype.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace DynamicEFprototype.Data;

public class ProtoContext :DbContext
{
    public ProtoContext(DbContextOptions<ProtoContext> options) : base(options)
    {

    }

    public virtual DbSet<Product> Products { get; set; }
}