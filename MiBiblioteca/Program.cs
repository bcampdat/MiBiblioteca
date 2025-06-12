using Microsoft.EntityFrameworkCore;
using MiBiblioteca.Models;
using System.Text.Json.Serialization;
using MiBiblioteca.Services;
using WebApiStore.Services;
using MiBiblioteca.Middlewares;
using MiBiblioteca.Filters;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Capturamos del app.settings la cadena de conexión a la base de datos
// Configuration.GetConnectionString va directamente a la propiedad ConnectionStrings y de ahí tomamos el valor de DefaultConnection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// Nuestros servicios resolverán dependencias de otras clases
// Registramos en el sistema de inyección de dependencias de la aplicación el ApplicationDbContext
builder.Services.AddDbContext<MiBibliotecaContext>(options =>
            options.UseSqlServer(connectionString));

// Sin tracking
builder.Services.AddDbContext<MiBibliotecaContext>(options =>
{
    options.UseSqlServer(connectionString);
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
}
);

builder.Services.AddControllers(options =>
{
    options.Filters.Add(typeof(FiltroDeExcepcion));// instancia de los filtros
    //options.Filters.Add(typeof(exceptionFiltrado));
    

}).AddJsonOptions(options => options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles); // evita ref circular en peticiones DTO

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        // builder.WithOrigins("https://www.apirequest.io").AllowAnyMethod().AllowAnyHeader();
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

builder.Services.AddTransient<OperacionesService>();
//builder.Services.AddTransient<GestorArchivos>();
builder.Services.AddTransient<IGestorImagenes, GestorImagenes>();
builder.Services.AddTransient<HashService, HashService > ();
//builder.Services.AddHostedService<TareaProgramadaService>();

builder.Services.AddTransient<TokenService, TokenService>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
               .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
               {
                   ValidateIssuer = false,
                   ValidateAudience = false,
                   ValidateLifetime = true,
                   ValidateIssuerSigningKey = true,
                   IssuerSigningKey = new SymmetricSecurityKey(
                     Encoding.UTF8.GetBytes(builder.Configuration["ClaveJWT"]))
               });


builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[]{}
                    }
                });

});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();

//app.UseMiddleware<BloquearPostMiddleware>();

app.MapControllers();

app.UseStaticFiles();

app.Run();
