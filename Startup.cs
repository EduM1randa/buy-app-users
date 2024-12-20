using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

public class Startup {
  public IConfiguration Configuration { get;}

  public Startup(IConfiguration configuration) {
    Configuration = configuration;
  }

  public void ConfigureServices(IServiceCollection services) {
    services.Configure<MongoDBSettings>(Configuration.GetSection("MongoDB"));
    services.AddSingleton<UserContext>();

    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
      .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters {
          ValidateIssuer = true,
          ValidateAudience = true,
          ValidateLifetime = true,
          ValidateIssuerSigningKey = true,
          ValidIssuer = Configuration["JWT:Issuer"],
          ValidAudience = Configuration["JWT:Audience"],
          IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:Key"]))
        };
      });
    
    services.AddControllers();
  }

  public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
    if (env.IsDevelopment()) {
      app.UseDeveloperExceptionPage();
    }

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseEndpoints(endpoints => {
      endpoints.MapControllers();
    });
  }
}