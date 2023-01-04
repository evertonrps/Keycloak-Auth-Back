using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Keycloak.AuthServices.Authorization;
using Keycloak.AuthServices.Authentication;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

var configuration = builder.Configuration;
var host = builder.Host;
var services = builder.Services;

var key = BuildRSAKey(configuration.GetSection("Keycloak")["ClientSecret"]);

builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowOrigin", x =>
              {
                  x
                   .AllowAnyHeader()
                   .AllowAnyMethod()
                   .AllowCredentials()
                   .SetIsOriginAllowed((host) => true);
              }
             );
    });


builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme; //added
})
    .AddJwtBearer(x =>
    {
        x.RequireHttpsMetadata = false;
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidIssuers = new[] { "http://localhost:8080/realms/development" },
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ClockSkew = TimeSpan.Zero,
           // ValidateLifetime = true,
        };
        #region === Event Authentification Handlers ===

        x.Events = new JwtBearerEvents()
        {
            OnTokenValidated = c =>
            {
                Console.WriteLine("User successfully authenticated");
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = c =>
            {
                c.NoResult();

                c.Response.StatusCode = 500;
                c.Response.ContentType = "text/plain";

                //if (IsDevelopment)
                // {
                return c.Response.WriteAsync(c.Exception.ToString());
                // }
                // return c.Response.WriteAsync("An error occured processing your authentication.");
            }
        };
        //.AddJwtBearer(o =>
        //{
        //    o.RequireHttpsMetadata = false;
        //    o.Authority = configuration["Jwt:Authority"];
        //    o.Audience = configuration["Jwt:Audience"];
        //    o.Events = new JwtBearerEvents()
        //    {
        //        OnAuthenticationFailed = c =>
        //        {
        //            c.NoResult();

        //        //    c.Response.StatusCode = 500;
        //          //  c.Response.ContentType = "text/plain";
        //            // if (Environment.IsDevelopment())
        //            // {
        //            return c.Response.WriteAsync(c.Exception.ToString());
        //            //}
        //            // return c.Response.WriteAsync("An error occured processing your authentication.");
        //        }
        //    };


        #endregion
    });
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "keycloack", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                  {

                    Id = JwtBearerDefaults.AuthenticationScheme, //The name of the previously defined security scheme.
                    Type = ReferenceType.SecurityScheme
                    },
            } ,
            new List<string>()
        }
    });
});

builder.Services.AddAuthorization(options =>
{
    //Create policy with more than one claim
    options.AddPolicy("users", policy => policy.RequireAssertion(context => context.User.HasClaim(c => (c.Value == "user") || (c.Value == "admin"))));

    //Create policy with only one claim
    options.AddPolicy("admins", policy => policy.RequireClaim("my_roles", "admin"));

    //Create a policy with a claim that doesn't exist or you are unauthorized to
    options.AddPolicy("noaccess", policy => policy.RequireClaim("my_roles", "noaccess"));
});

//builder.Services.AddAuthorization(options =>
//{
//    //Create policy with more than one claim
//    options.AddPolicy("users", policy => policy.RequireAssertion(context => context.User.HasClaim(c => (c.Value == "user") || (c.Value == "admin"))));

//    //Create policy with only one claim
//    options.AddPolicy("admins", policy => policy.RequireClaim("user_roles", "admin"));

//    //Create a policy with a claim that doesn't exist or you are unauthorized to
//    options.AddPolicy("noaccess", policy => policy.RequireClaim("user_roles", "noaccess"));
//})
//    .AddKeycloakAuthorization(configuration);

var app = builder.Build();

app.UseCors("AllowOrigin");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();



app.Run();

static RsaSecurityKey BuildRSAKey(string publicKeyJWT)
{
    RSA rsa = RSA.Create();

    rsa.ImportSubjectPublicKeyInfo(

        source: Convert.FromBase64String(publicKeyJWT),
        bytesRead: out _
    );

    var IssuerSigningKey = new RsaSecurityKey(rsa);

    return IssuerSigningKey;
}