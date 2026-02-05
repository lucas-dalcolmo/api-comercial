using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.OpenApi;
using Api.Comercial.Data;
using Api.Comercial.Auth;
using Api.Comercial.Swagger;
using Api.Comercial.Repositories;
using Api.Comercial.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "api-comercial", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe apenas o token (o Swagger adiciona o prefixo Bearer automaticamente)."
    });

    options.OperationFilter<AuthorizeOperationFilter>();
});

if (builder.Environment.IsDevelopment())
{
    builder.Services
        .AddAuthentication("DevBearer")
        .AddScheme<AuthenticationSchemeOptions, DevBearerAuthenticationHandler>("DevBearer", _ => { });
}
else
{
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
}

builder.Services.AddAuthorization();

builder.Services.AddDbContext<ApeironDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
});

builder.Services.AddScoped(typeof(IRepository<,>), typeof(EfRepository<,>));
builder.Services.AddScoped(typeof(IIntLookupService<>), typeof(IntLookupService<>));
builder.Services.AddScoped(typeof(ICodeNameService<>), typeof(CodeNameService<>));
builder.Services.AddScoped<IStateService, StateService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
