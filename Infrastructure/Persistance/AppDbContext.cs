using AuthGDPR.Domain.Entities.Auth;
using AuthGDPR.Domain.Entities.Consent;
using AuthGDPR.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace AuthGDPR.Infrastructure.Persistance
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        { }

        // Usa ApplicationUser invece di User
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<UserConsent> UserConsents { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<DataSubjectRequest> DataSubjectRequests { get; set; }
        public DbSet<RefreshTokenEntity> RefreshTokens { get; set; }

        // Nuovi DbSet per la gestione dei consensi
        public DbSet<ConsentPolicy> ConsentPolicies { get; set; }
        public DbSet<UserConsentHistory> UserConsentHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.FirstName).IsRequired();
                entity.Property(u => u.LastName).IsRequired();
                entity.Property(u => u.CreatedAt).IsRequired();

                entity.HasMany(u => u.Consents)
                      .WithOne(c => c.User)
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.DataSubjectRequests)
                      .WithOne(r => r.User)
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(u => u.PseudonymizedUserId).IsRequired();
            });

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(a => a.Id);
                // Converte l'enum MessageCategory in stringa nel DB
                entity.Property(a => a.MessageCategory).IsRequired().HasConversion<string>().HasMaxLength(50);
                entity.Property(a => a.Timestamp).IsRequired();
                // Converte l'enum ActionType in stringa nel DB
                entity.Property(a => a.ActionType).IsRequired().HasConversion<string>().HasMaxLength(50);
                entity.Property(a => a.EntityName).IsRequired();
                entity.Property(a => a.EntityId).IsRequired();

                entity.Property(a => a.TraceId).IsRequired();
            });

            // Configurazione di ConsentPolicy
            modelBuilder.Entity<ConsentPolicy>(entity =>
            {
                entity.HasKey(cp => cp.Id);
                entity.Property(cp => cp.Version).IsRequired();
                entity.Property(cp => cp.Text).IsRequired();
                entity.Property(cp => cp.EffectiveDate).IsRequired();
            });

            // Configurazione di UserConsent
            modelBuilder.Entity<UserConsent>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.ConsentDate).IsRequired();
                entity.Property(c => c.CreatedDate).IsRequired();

                // Relazione con ConsentPolicy (il consenso "approvato" si riferisce a una versione specifica)
                entity.HasOne(c => c.ConsentPolicy)
                      .WithMany() // Se desideri la collezione inversa, modifica in .WithMany(cp => cp.UserConsents)
                      .HasForeignKey(c => c.ConsentPolicyId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configurazione di UserConsentHistory (opzionale)
            modelBuilder.Entity<UserConsentHistory>(entity =>
            {
                entity.HasKey(uch => uch.Id);
                entity.Property(uch => uch.ChangeDate).IsRequired();
                entity.Property(uch => uch.ConsentType).HasConversion<string>().HasMaxLength(50);
                entity.Property(uch => uch.ChangeType).HasConversion<string>().HasMaxLength(50);

                // Relazione con UserConsent
                entity.HasOne(uch => uch.UserConsent)
                      .WithMany()
                      .HasForeignKey(uch => uch.UserConsentId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<DataSubjectRequest>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.RequestType).IsRequired();
                entity.Property(r => r.RequestDate).IsRequired();
                entity.Property(r => r.Status).IsRequired();
                entity.Property(r => r.TraceIdentifier).IsRequired();
            });
        }
    }
}
