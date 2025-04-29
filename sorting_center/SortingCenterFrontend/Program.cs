using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Подключение статических файлов
app.UseDefaultFiles(); // Указывает на использование index.html по умолчанию
app.UseStaticFiles();  // Включает поддержку статических файлов

app.Run();
