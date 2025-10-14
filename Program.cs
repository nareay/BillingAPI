using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ Enable CORS for front-end access
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// ✅ Enable Swagger for all environments (so you can test live)
app.UseSwagger();
app.UseSwaggerUI();

// ❌ Remove HTTPS redirection on Render
// app.UseHttpsRedirection();

// ✅ Serve static files from wwwroot (for index.html etc.)
app.UseStaticFiles();

app.UseAuthorization();

// ✅ Enable CORS
app.UseCors();

// ✅ Map controllers
app.MapControllers();

// ✅ Run the app (Render uses port 8080 by default)
app.Run();
