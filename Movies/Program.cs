using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Movies.Models;
using Movies.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var connectionStrings = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options=>
    options.UseSqlServer(connectionStrings));



builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddTransient<IGenresService,GenresService>();
builder.Services.AddTransient<IMoviesService,MoviesService>();

//AutoMapper

builder.Services.AddAutoMapper(typeof(Program));

//Enable CORS
builder.Services.AddCors();
builder.Services.AddSwaggerGen(option => {
    option.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "TestApi",
        Description="MyFirstApi",
        TermsOfService=new Uri("https://www.google.com"),
        Contact=new OpenApiContact
        {
            Name="Omar",
            Email="omarmohammed1642003@gmail.com",
            Url = new Uri("https://www.google.com"),
        },
        License=new OpenApiLicense
        {
            Name="My License",
            Url = new Uri("https://www.google.com"),
        }
    });

    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter Your JWT key",
    });

    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference=new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                },
                Name="Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
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

app.UseCors(c=>c.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

app.UseAuthorization();

app.MapControllers();

app.Run();
