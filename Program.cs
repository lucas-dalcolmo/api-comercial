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

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>(optional: true);
}

builder.Services.AddControllers();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "api-comercial", Version = "v1" });
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Provide only the token (Swagger adds the Bearer prefix automatically)."
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
builder.Services.AddScoped<IScoreCategoryWeightService, ScoreCategoryWeightService>();
builder.Services.AddScoped<IScoreValueWeightService, ScoreValueWeightService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IEmployeeContractService, EmployeeContractService>();
builder.Services.AddScoped<IEmployeeDocumentService, EmployeeDocumentService>();
builder.Services.AddScoped<IEmployeeBenefitService, EmployeeBenefitService>();
builder.Services.AddScoped<IEmployeeContractBenefitService, EmployeeContractBenefitService>();
builder.Services.AddScoped<IBenefitFormulaEvaluator, BenefitFormulaEvaluator>();
builder.Services.AddScoped<IBenefitFormulaVariableResolver, BenefitFormulaVariableResolver>();
builder.Services.AddScoped<IBenefitFormulaVariableService, BenefitFormulaVariableService>();
builder.Services.AddScoped<IHtmlSanitizerService, HtmlSanitizerService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IOpportunityService, OpportunityService>();
builder.Services.AddScoped<IProposalService, ProposalService>();
builder.Services.AddScoped<IProposalDocumentService, ProposalDocumentService>();
builder.Services.AddScoped<ICommercialDashboardService, CommercialDashboardService>();
builder.Services.AddScoped<IOperationalHrDashboardService, OperationalHrDashboardService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
