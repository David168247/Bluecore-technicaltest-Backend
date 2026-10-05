using BluecoreApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace BluecoreApi.Data.Configurations;

public sealed class CreditRequestConfiguration : IEntityTypeConfiguration<CreditRequest>
{
    public void Configure(EntityTypeBuilder<CreditRequest> entity)
    {
        entity.ToTable("credit_cases", "esquema_c");
        entity.HasKey(x => x.Id).HasName("pk_credit_cases");
        entity.Property(x => x.Id).HasColumnName("id");
        entity.Property(x => x.ApplicantId).HasColumnName("applicant_id");
        entity.Property(x => x.Amount).HasColumnName("amount");
        entity.Property(x => x.TermMonths).HasColumnName("term_months");
        entity.Property(x => x.Status).HasColumnName("status");
        entity.Property(x => x.Comment).HasColumnName("comment");
        entity.Property(x => x.CreatedAt).HasColumnName("created_at");
        entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
    }
}
