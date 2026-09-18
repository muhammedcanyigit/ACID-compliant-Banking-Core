using BankCore.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace BankCore.Api.Data;

public class BankDbContext : DbContext
{
    public BankDbContext(DbContextOptions<BankDbContext> options) : base(options)
    {
    }

    // Veritabanındaki Tablolarımızın C# Karşılıkları (DbSet)
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

    // Fluent API: Veritabanı Kurallarının Mimarisi
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. CUSTOMER MİMARİSİ
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.HasIndex(c => c.Email).IsUnique();
            entity.HasIndex(c => c.IdentityNumber).IsUnique();
        });

        // 2. ACCOUNT MİMARİSİ & BANKACILIK HASSASİYETİ
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.HasIndex(a => a.AccountNumber).IsUnique();

            // Hassas Para Hesabı: SQL DECIMAL(18,2)
            entity.Property(a => a.Balance)
                .HasPrecision(18, 2);

            // İlişki: 1 Müşteri -> N Hesap
            entity.HasOne(a => a.Customer)
                .WithMany(c => c.Accounts)
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.Restrict); // Müşteri silinse de hesap kalır
        });

        // 3. TRANSACTION (DEKONT) MİMARİSİ
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(t => t.Id);

            // Hassas Transfer Tutarı
            entity.Property(t => t.Amount)
                .HasPrecision(18, 2);

            // İlişki: Gönderen Hesap (1 Account -> N Sent Transactions)
            entity.HasOne(t => t.SenderAccount)
                .WithMany(a => a.SentTransactions)
                .HasForeignKey(t => t.SenderAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            // İlişki: Alıcı Hesap (1 Account -> N Received Transactions)
            entity.HasOne(t => t.ReceiverAccount)
                .WithMany(a => a.ReceivedTransactions)
                .HasForeignKey(t => t.ReceiverAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}