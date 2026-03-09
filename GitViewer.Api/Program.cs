

using GitViewer.Api.RabbitMQ;
using GitViewer.Api.Services;
using GitViewer.DataAccess.Models;
using Lucene.Net.Support;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace GitViewer.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.AddServiceDefaults();

            builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

            builder.Services.Configure<GitRepoSettings>(builder.Configuration.GetSection("GitRepo"));

            builder.Services.AddScoped<IMessageProducer, RabbitMQProducer>();
            builder.Services.AddScoped<IGitFileService, GitFileService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IRepositoryService, RepositoryService>();
            builder.Services.AddScoped<ILoggingService, LoggingService>();

            builder.Services.Configure<RabbitMQSettings>(builder.Configuration.GetSection("RabbitMQ"));

            builder.Services.AddMemoryCache();
            builder.Services.AddSingleton<ILogRateLimiter, LogRateLimiter>();
            builder.Services.AddSingleton<IGitRepoManager, GitRepoManager>();

            //logging GitRepo paths
            Console.WriteLine("ReposRoot: " + builder.Configuration["GitRepo:ReposRoot"]);
            Console.WriteLine("KeysRoot: " + builder.Configuration["GitRepo:KeysRoot"]);

            var allowedOrigins = builder.Configuration
            .GetSection("AllowedOrigins")
            .Get<string[]>();

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["AppSettings:Issuer"],
                        ValidAudience = builder.Configuration["AppSettings:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:Token"]!)),
                        ClockSkew = TimeSpan.Zero,


                    };
                });
            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddControllers()
                .AddNewtonsoftJson();

            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new() { Title = "GitViewer API", Version = "v1" });

                // Add JWT Authentication to Swagger
                options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "Enter 'Bearer' and token"
                });

                options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });


            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend",
                    builder =>
                    {
                        builder.WithOrigins(allowedOrigins!).AllowCredentials().AllowAnyMethod().AllowAnyHeader();
                    });
            });

            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddDbContext<GitViewerServiceContext>(options =>
                    options.UseNpgsql(builder.Configuration.GetConnectionString("postgres"),
                    npgsqlOptions => npgsqlOptions.CommandTimeout(100)));

            }

            else
            {
                builder.Services.AddDbContext<GitViewerServiceContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions => npgsqlOptions.CommandTimeout(100)));
            }

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors("AllowFrontend");
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
