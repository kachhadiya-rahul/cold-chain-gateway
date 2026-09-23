using System.Threading.Channels;
using ColdChain.Gateway;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Services.AddSignalR();
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .WithOrigins("http://localhost:4200")
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));

var store = $"cold-chain-{Guid.NewGuid()}";
builder.Services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(store));

var queue = Channel.CreateBounded<Reading>(256);
builder.Services.AddSingleton(queue);
builder.Services.AddSingleton(queue.Writer);
builder.Services.AddSingleton(queue.Reader);
builder.Services.AddHostedService<ReadingWorker>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Seed();

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();

app.UseStaticFiles();
app.MapControllers();
app.MapHub<AlertsHub>("/hubs/alerts");

app.Run();

public partial class Program;
