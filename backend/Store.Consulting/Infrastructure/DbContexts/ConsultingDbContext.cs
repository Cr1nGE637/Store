using Microsoft.EntityFrameworkCore;
using Store.Consulting.Infrastructure.Entity;

namespace Store.Consulting.Infrastructure.DbContexts;

public sealed class ConsultingDbContext(DbContextOptions<ConsultingDbContext> options) : DbContext(options)
{
    public DbSet<CompatibilityRuleEntity> CompatibilityRules { get; set; }
    public DbSet<ConsultationResultEntity> ConsultationResults { get; set; }
    public DbSet<RecommendationEventEntity> RecommendationEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("consulting");

        modelBuilder.Entity<CompatibilityRuleEntity>(builder =>
        {
            builder.ToTable("CompatibilityRules");
            builder.HasKey(rule => rule.Id);
            builder.Property(rule => rule.Id).ValueGeneratedNever();
            builder.Property(rule => rule.Code).IsRequired().HasMaxLength(100);
            builder.Property(rule => rule.Name).IsRequired().HasMaxLength(150);
            builder.Property(rule => rule.SourceCategoryCode).IsRequired().HasMaxLength(50);
            builder.Property(rule => rule.TargetCategoryCode).IsRequired().HasMaxLength(50);
            builder.Property(rule => rule.SourceSpecificationKey).IsRequired().HasMaxLength(100);
            builder.Property(rule => rule.TargetSpecificationKey).IsRequired().HasMaxLength(100);
            builder.Property(rule => rule.Operator).IsRequired().HasMaxLength(50);
            builder.Property(rule => rule.ExpectedValue).HasMaxLength(250);
            builder.Property(rule => rule.Severity).IsRequired().HasMaxLength(50);
            builder.Property(rule => rule.MessageTemplate).IsRequired().HasMaxLength(500);
            builder.Property(rule => rule.RecommendationType).HasMaxLength(50);
            builder.HasIndex(rule => rule.Code).IsUnique();
            builder.HasIndex(rule => new { rule.SourceCategoryCode, rule.TargetCategoryCode, rule.IsActive });
        });

        modelBuilder.Entity<ConsultationResultEntity>(builder =>
        {
            builder.ToTable("ConsultationResults");
            builder.HasKey(result => result.Id);
            builder.Property(result => result.Id).ValueGeneratedNever();
            builder.Property(result => result.Status).IsRequired().HasMaxLength(50);
            builder.Property(result => result.ProductIdsJson).HasColumnType("jsonb");
            builder.Property(result => result.ItemsJson).HasColumnType("jsonb");
            builder.Property(result => result.FindingsJson).HasColumnType("jsonb");
            builder.Property(result => result.RecommendationsJson).HasColumnType("jsonb");
            builder.HasIndex(result => result.CustomerId);
            builder.HasIndex(result => result.CheckedAtUtc);
        });

        modelBuilder.Entity<RecommendationEventEntity>(builder =>
        {
            builder.ToTable("RecommendationEvents");
            builder.HasKey(recommendation => recommendation.Id);
            builder.Property(recommendation => recommendation.Id).ValueGeneratedNever();
            builder.Property(recommendation => recommendation.Type).IsRequired().HasMaxLength(50);
            builder.Property(recommendation => recommendation.Reason).IsRequired().HasMaxLength(500);
            builder.HasIndex(recommendation => recommendation.ConsultationId);
            builder.HasIndex(recommendation => recommendation.ProductId);
        });
    }
}
