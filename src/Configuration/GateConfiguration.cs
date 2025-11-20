using GateEntryExit.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GateEntryExit.Configuration
{
    public class GateConfiguration : IEntityTypeConfiguration<Gate>
    {
        public void Configure(EntityTypeBuilder<Gate> builder)
        {
            builder.Property(b => b.Name).HasMaxLength(50);

            // a way to seed
            //builder.HasData(new Gate
            //{
            //    Name = "Gate56"
            //});
        }
    }
}
