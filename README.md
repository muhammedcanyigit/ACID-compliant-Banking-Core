================================================================================
           SENTINELBANK - CORE API: GELİŞTİRME VE MİMARİ MANTIĞI
================================================================================
Slogan: ACID uyumlu, RBAC yetkilendirmeli ve kural tabanlı sahtekarlık (Fraud)
        analizi yapan bankacılık backend çekirdeği.
Teknolojiler: C# (.NET Core Web API), PostgreSQL / MSSQL, Entity Framework Core,
             JWT + Refresh Token, Redis.

================================================================================
[BÖLÜM 1] DİYAGRAM ŞEKİLLERİ VE ANLAMLARI (AKIŞ ŞEMASI REHBERİ)
================================================================================

Akış şemalarında (Flowchart) kullanılan temel semboller ve anlamları:

1. DİKDÖRTGEN (İşlem / Process):
   - Anlamı: Yapılacak somut bir işi, kod bloğunu veya adımı temsil eder.
   - Örnek: "Gelen şifreyi Hash'le", "Hesap bakiyesini güncelle".

2. ELMAS / EŞKENAR DÖRTGEN (Karar / Decision):
   - Anlamı: Bir mantıksal kontrolü ifade eder. İki veya daha fazla çıkışı vardır
     (Evet/Hayır, Doğru/Yanlış).
   - Örnek: "Token geçerli mi?", "Bakiye yeterli mi?".

3. YUVARLAK / STADYUM ŞEKLİ (Başlangıç & Bitiş / Terminal):
   - Anlamı: Sürecin nerede başlayıp nerede bittiğini gösterir.
   - Örnek: "İstek Alındı (Start)", "İşlem Tamamlandı (Stop)".

4. SİLİNDİR (Veritabanı / Data Store):
   - Anlamı: Kalıcı olarak verinin saklandığı DB veya önbellek (Cache) katmanıdır.
   - Örnek: "PostgreSQL Database", "Redis Cache".

5. OKLAR (Akış Yönü / Flow Line):
   - Anlamı: Verinin veya kontrolün adım adım nereden nereye gittiğini belirtir.

================================================================================
[BÖLÜM 2] AKIŞ ŞEMASI VE GELİŞTİRME ADIMLARI (WORKFLOW DÖKÜMÜ)
================================================================================

--------------------------------------------------------------------------------
KATMAN 1: HESAP VE İŞLEM ÇEKİRDEĞİ (ACCOUNT & TRANSACTION CORE)
--------------------------------------------------------------------------------

[ADIM 1.1] Veritabanı Tasarımı (Database Design)
  ├── İşlem: Tablo ve İlişki Yapılarının Kurulması
  ├── Tablolar:
  │    ├── Customers (Id, Name, Email, IdentityNo, PasswordHash, CreatedAt)
  │    ├── Accounts (Id, CustomerId, AccountNumber, Balance, Currency, Status)
  │    └── Transactions (Id, SenderAccountId, ReceiverAccountId, Amount, Status, CreatedAt)
  └── Kısıtlar (SQL Constraints):
       └── Balance >= 0 (Eksi bakiye kontrolü / Constraint)

[ADIM 1.2] Hesap Servisleri (Account Services - C# .NET Core)
  ├── İşlem: Temel CRUD ve İş Mantığı API Controller'larının Hazırlanması
  ├── Sorumluluklar:
  │    ├── Hesap Oluşturma / Bakiye Sorgulama
  │    └── Transfer İsteklerini Karşılama (Endpoint Entry)
  └── Aktarım: İşlem mantığı ve güvenliği doğrulandıktan sonra Transfer adımına geçer.

[ADIM 1.3] Güvenli Transfer & ACID Mimarisi (Secure Transfer Execution)
  ├── Başlangıç: BEGIN DbContextTransaction (İşlem Bloğu Başlatılır)
  ├── İşlemler:
  │    1. Gönderen hesaptan tutar düşülür (Debit).
  │    2. Alıcı hesaba tutar eklenir (Credit).
  │    3. Log/Transaction kaydı oluşturulur.
  ├── Karar Düğümü: İŞLEM BAŞARILI MI?
  │    ├── [EVET / OK]  --> COMMIT yapılır (Veriler kalıcı olarak kaydedilir).
  │    └── [HAYIR / ERR] --> ROLLBACK yapılır (Tüm değişiklikler geri alınır).
  └── Çıktı: İşlem sonucu müşteriye yanıt (HTTP Status Code) olarak dönülür.

--------------------------------------------------------------------------------
KATMAN 2: GÜVENLİK VE FRAUD KALKANI (SECURITY & FRAUD SHIELD)
--------------------------------------------------------------------------------

[ADIM 2.1] Kimlik Doğrulama & JWT Mimarisi (Auth & JWT System)
  ├── İşlem: Giriş (Login) ve Kayıt (Register) Servisi
  ├── Mantık:
  │    1. Kullanıcı Giriş Yapar (Email + Parola).
  │    2. Başarılı ise 15 Dakika ömürlü Access Token (JWT) üretilir.
  │    3. Uzun ömürlü Refresh Token üretilir ve Redis DB'de saklanır.
  └── Token İptali (Revocation/Logout):
       └── Çıkış yapıldığında JWT, Redis "Blacklist" listesine eklenir ve geçersiz kılınır.

[ADIM 2.2] Roller ve Yetkilendirme (RBAC - Role-Based Access Control)
  ├── İşlem: Kullanıcı Rol Kontrolü ([Authorize(Roles = "...")])
  ├── Roller:
  │    ├── Customer     --> Sadece kendi hesaplarını/transferlerini görebilir.
  │    ├── Teller       --> Müşteri adına para yatırma/çekme yapabilir.
  │    └── FraudAnalyst --> Şüpheli işlemleri ve logları inceleyebilir/bloklayabilir.
  └── Kontrol: Yetkisi olmayan istekler HTTP 403 Forbidden döner.

[ADIM 2.3] Kural Tabanlı Sahtekarlık Analizi (Fraud Detection Engine)
  ├── İşlem: Transfer isteği veritabanına işlenmeden ÖNCE çalışacak C# Kuralları
  │
  ├── KURAL 1: Velocity Rule (Frekans Kontrolü)
  │    ├── Soru: Aynı hesaptan son 60 saniye içinde 3'ten fazla transfer yapıldı mı?
  │    └── Eylem: EVET ise işlemi DURDUR (Block) ve hesabı şüpheliye al (`Flagged`).
  │
  └── KURAL 2: Amount Outlier Rule (Tutar Anomali Kontrolü)
       ├── Soru: Transfer tutarı, müşterinin son 30 günlük harcama ortalamasının 5 katından fazla mı?
       └── Eylem: EVET ise SMS / Email doğrulaması (2FA) iste.

================================================================================
[BÖLÜM 3] SENARYO İLE SİSTEMİN ÇALIŞMA AKIŞI (STEP-BY-STEP RUNTIME)
================================================================================

1. Kullanıcı `/api/auth/login` isteği atar -> Sistem JWT Access Token ve Refresh Token üretir.
2. Kullanıcı `/api/transfer` isteği atar (Header: `Authorization: Bearer <JWT>`).
3. C# Middleware JWT'yi doğrular + Redis Blacklist kontrolü yapar.
4. RBAC katmanı kullanıcının "Customer" rolünü doğrular.
5. Fraud Engine devreye girer:
   - Son 60 saniyedeki işlem sayısı kontrol edilir (Velocity Rule).
   - Tutar, 30 günlük ortalama ile kıyaslanır (Amount Outlier Rule).
6. Fraud kontrolünden geçen işlem için `DbContextTransaction` başlatılır (`BEGIN`).
7. Bakiye kontrol edilir, gönderen düşülür, alıcıya eklenir.
8. Bir hata oluşmazsa `COMMIT` edilir, hata oluşursa `ROLLBACK` yapılır.
9. Sonuç yanıtı JSON olarak istemciye iletilir.
================================================================================
