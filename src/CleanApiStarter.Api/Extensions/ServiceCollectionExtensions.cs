using System.Reflection;
using System.Text;
using CleanApiStarter.Api.Middleware;
using CleanApiStarter.Application.Abstractions;
using CleanApiStarter.Application.Common;
using CleanApiStarter.Domain.Constants;
using CleanApiStarter.Infrastructure.Authentication;
using CleanApiStarter.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

namespace CleanApiStarter.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IRequestContext, Services.HttpRequestContext>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        services.AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .ToDictionary(
                            x => string.IsNullOrEmpty(x.Key) ? "request" : char.ToLowerInvariant(x.Key[0]) + x.Key[1..],
                            x => x.Value!.Errors.Select(error =>
                                string.IsNullOrWhiteSpace(error.ErrorMessage) ? "The value is invalid." : error.ErrorMessage).ToArray());
                    return new BadRequestObjectResult(ApiResponse<object>.Fail(
                        "Request validation failed.", context.HttpContext.TraceIdentifier, errors));
                };
            });

        AddAuthentication(services);
        services.AddAuthorizationBuilder()
            .AddPolicy("AuthenticatedUser", policy => policy.RequireAuthenticatedUser())
            .AddPolicy("SuperAdminOnly", policy => policy.RequireRole(AppRoles.SuperAdmin));

        services.AddHealthChecks().AddDbContextCheck<AppDbContext>("database");
        AddSwagger(services);
        return services;
    }

    private static void AddAuthentication(IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((options, jwtOptions) =>
            {
                var jwt = jwtOptions.Value;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsJsonAsync(ApiResponse<object>.Fail(
                            "Authentication is required.", context.HttpContext.TraceIdentifier,
                            new Dictionary<string, string[]> { ["code"] = ["authentication_required"] }));
                    },
                    OnForbidden = async context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        await context.Response.WriteAsJsonAsync(ApiResponse<object>.Fail(
                            "You do not have permission to access this resource.", context.HttpContext.TraceIdentifier,
                            new Dictionary<string, string[]> { ["code"] = ["forbidden"] }));
                    }
                };
            });
    }

    private static void AddSwagger(IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "CleanApiStarter API",
                Version = "v1",
                Description = "A generic, reusable ASP.NET Core API starter."
            });
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter the JWT access token."
            });
            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });
            options.TagActionsBy(description =>
                [description.ActionDescriptor.EndpointMetadata.OfType<TagsAttribute>().FirstOrDefault()?.Tags.FirstOrDefault() ?? "General"]);
            options.DocInclusionPredicate((_, _) => true);

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath)) options.IncludeXmlComments(xmlPath);
        });
    }
}
