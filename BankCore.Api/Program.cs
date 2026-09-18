using BankCore.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. BankDbContext Servisinin Eklenmesi (Tercümanın Sisteme Tanıtılması)
// UseSqlServer yerine UseNpgsql yazıyoruz
builder.Services.AddDbContext<BankDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Controller ve Swagger Servislerinin Eklenmesi
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 3. HTTP Request Pipeline Yapılandırması
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// 4. Controller Endpoint'lerinin Eşleştirilmesi
app.MapControllers();

app.Run();