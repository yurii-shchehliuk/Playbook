using API.Scrapping.Core;
using API.Scrapping.Services;
using Microsoft.EntityFrameworkCore;
using Web.Persistance;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(
        options => options.JsonSerializerOptions.PropertyNamingPolicy = null);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// register mongodb
builder.Services.Configure<MongoSettings>(
    builder.Configuration.GetSection("PlaybookDatabase"));
// register service
builder.Services.AddSingleton(typeof(MongoService<>));

//builder.Services.AddScoped<IMatchService, MatchService>();

Consts._conf = builder.Configuration;
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseRouting();

app.UseHttpsRedirection();

app.UseAuthorization();

//app.UseEndpoints(endpoints =>
//{
//    endpoints.MapControllers();
//});
app.MapControllers();

app.Run();
