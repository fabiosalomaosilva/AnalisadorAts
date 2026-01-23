using AnalisadorAts.Core.Interfaces;
using AnalisadorAts.Infrastructure.Parsers;
using AnalisadorAts.Infrastructure.Extraction;
using AnalisadorAts.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configurar Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Analisador ATS API",
        Version = "v1",
        Description = "API para análise de currículos compatível com sistemas ATS (Applicant Tracking System)",
        Contact = new()
        {
            Name = "Analisador ATS"
        }
    });

    // Incluir comentários XML (opcional)
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Registrar dependências
builder.Services.AddSingleton<SkillsDictionary>();
builder.Services.AddSingleton<DataExtractor>();

// Registrar parsers
builder.Services.AddScoped<IDocumentParser, PdfParser>();
builder.Services.AddScoped<IDocumentParser, DocxParser>();
builder.Services.AddScoped<DocumentParserFactory>();

// Registrar serviços
builder.Services.AddScoped<IAnalysisService, AnalysisService>();

// Configurar CORS (se necessário)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Analisador ATS API v1");
        options.RoutePrefix = string.Empty; // Swagger na raiz
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.MapControllers();

app.Run();
