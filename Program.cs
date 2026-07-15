using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using MPCRS.Models;
using MPCRS.Utilities;
using Microsoft.Extensions.FileProviders;
using MPCRS.Services;

namespace MPCRS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var connectionString = builder.Configuration.GetConnectionString("MPCRS");

            // Add services to the container.
            //builder.Services.AddResponseCompression(options =>
            //{
            //    options.EnableForHttps = true;
            //});
            //builder.Services.AddAutoMapper(typeof(Program));
            builder.Services.AddControllersWithViews();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowReactApp",
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:3000", "http://localhost:3001")
                              .AllowAnyHeader()
                              .AllowAnyMethod()
                              .AllowCredentials()
                             .SetIsOriginAllowed(origin => true); // Add this
                    });
            });
            
            builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(BinderOptions =>
                {
                    BinderOptions.LoginPath = "/Auth/Index";
                    BinderOptions.AccessDeniedPath = "/Auth/UnAuthorized";
                    BinderOptions.Cookie.SameSite = SameSiteMode.None; // Add this
                    BinderOptions.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Add this
                });
            builder.Services.AddSession(options => { options.IdleTimeout = TimeSpan.FromMinutes(240); });
            //builder.Services.AddWebOptimizer(minifyJavaScript: false, minifyCss: true);

            var currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var environmentName = builder.Environment.EnvironmentName;


            builder.Configuration
                .SetBasePath(currentDirectory)
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{environmentName}.json", true, true)
                .AddEnvironmentVariables();

            builder.Services.AddSingleton<MPDapperContext>();
            builder.Services.AddDbContext<DESI_STFE_PRODContext>(options =>
                options.UseSqlServer(connectionString)); ;

            //// Add a DbContext to store your Database Keys
            builder.Services.AddDbContext<MyKeysContext>(options =>
                options.UseSqlServer(connectionString));

            // using Microsoft.AspNetCore.DataProtection;
            builder.Services.AddDataProtection()
                .PersistKeysToDbContext<MyKeysContext>();

            // Register EmailService
            builder.Services.AddScoped<IEmailService, EmailService>();
            // Register Email Background Service
            builder.Services.AddHostedService<EmailSyncBackgroundService>();

            builder.Services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = long.MaxValue; // Set the desired maximum request body size
                options.Limits.MaxRequestBufferSize = long.MaxValue;
               
               
            });

            builder.Services.Configure<FormOptions>(options =>
            {
                options.ValueLengthLimit = int.MaxValue;
                options.MultipartBodyLengthLimit = long.MaxValue; // Set the desired maximum multipart body length
                options.BufferBodyLengthLimit = long.MaxValue;
            });

            builder.Services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
                options.MaxRequestBodyBufferSize = int.MaxValue;
                options.MaxRequestBodySize = long.MaxValue;
            });

            builder.Services.AddHttpContextAccessor();


            builder.Services.AddHttpClient();
            builder.Services.AddSingleton<AiAPIService>();

            builder.Services.Configure<BubbleDetectionOptions>(
                builder.Configuration.GetSection("BubbleDetection"));


            var app = builder.Build();

            PythonProcessManager.Start(app.Configuration, app.Environment);

            //app.UseResponseCompression();
            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
             

            app.UseHttpsRedirection();

            app.UseStaticFiles();
           
            app.UseRouting();
              
          
            app.UseCors("AllowReactApp"); 
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "v3modules")),
                RequestPath = "/v3modules"
            });

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Auth}/{action=Index}/{id?}");

            var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
            lifetime.ApplicationStopping.Register(() => PythonProcessManager.Stop());

           app.Run();

        }
    }
}