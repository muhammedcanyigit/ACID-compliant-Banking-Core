# 📓 Project Development Logbook

## [GÜN 1] - Proje Kurulumu ve Veritabanı Mimarisi

### 🛠️ Yapılanlar:
- [x] Proje mimarisi ve workflow rehberi belirlendi (`README.md`).
- [x] .NET Core Web API Solution ve proje klasör yapısı oluşturuldu (`BankCore.sln`, `BankCore.Api`).
- [x] GitHub reposu bağlandı ve ilk versiyon yüklendi.
- [x] SQL Veritabanı şeması ve kısıtlamaları (Constraints) tasarlandı (`Database.sql`).
- [x] Bankacılık mantığına uygun C# Entity sınıfları yazıldı (`Customer.cs`, `Account.cs`, `Transaction.cs`).

### 📌 Sonraki Adım:
- Entity Framework Core (EF Core) NuGet paketlerini projeye eklemek.
- `BankDbContext.cs` sınıfını yazıp Fluent API ile veritabanı ilişkilerini (1-N, N-N, Precision) yapılandırmak.