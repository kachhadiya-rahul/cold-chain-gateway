using System.Threading.Channels;
using ColdChain.Gateway;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var queue = Channel.CreateBounded<Reading>(256);
builder.Services.AddSingleton(queue);
builder.Services.AddSingleton(queue.Writer);

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
