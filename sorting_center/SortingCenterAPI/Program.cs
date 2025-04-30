using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SortingCenterAPI.Controllers;
using SortingCenterAPI.Models;
using SortingCenterModel;
using System.Collections.Generic; // Добавлено для использования List<T>  
using System.Text.Json.Serialization.Metadata;

var builder = WebApplication.CreateBuilder(args);


// Добавляем CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5158") // Укажите адрес SortingCenterFrontend
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


builder.Services.AddSingleton(provider =>
{
    var config = new SortingCenterConfig
    {
        rowNumber = 5,
        columnNumber = 5,
        lineNumber = 10
    };
    return new SortCenterWrapper(config);
});

builder.Services.AddControllers()
   .AddJsonOptions(options =>
   {
       options.JsonSerializerOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver
       {
           Modifiers =
           {
               static (typeInfo) =>
               {
                   if (typeInfo.Type == typeof(GraphStateResponse))
                   {  
                       // Убедитесь, что все связанные типы зарегистрированы  
                       foreach (var property in typeInfo.Properties)
                       {
                           if (property.Name == "Nodes" && property.PropertyType == typeof(List<GraphNode>))
                           {
                               property.IsRequired = true; // Пример модификации свойства
                           }
                           else if (property.Name == "Edges" && property.PropertyType == typeof(List<GraphEdge>))
                           {
                               property.IsRequired = true; // Пример модификации свойства
                           }
                       }
                   }
               }
           }
       };
   });

var app = builder.Build();

app.UseCors("AllowFrontend");

// Подключаем маршрутизацию для контроллеров  
app.MapControllers();

app.Run();