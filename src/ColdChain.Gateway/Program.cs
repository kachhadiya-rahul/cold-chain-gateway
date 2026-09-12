using System.Threading.Channels;
using ColdChain.Gateway;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var store = $"cold-chain-{Guid.NewGuid()}";
builder.Services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(store));

var queue = Channel.CreateBounded<Reading>(256);
builder.Services.AddSingleton(queue);
builder.Services.AddSingleton(queue.Writer);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Seed();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program;
