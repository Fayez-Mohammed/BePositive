using Base.API.Authorization;
using Base.API.Filters;
using Base.API.Hubs;
using Base.API.MiddleWare;
using Base.API.Services;
using Base.DAL.Contexts;
using Base.DAL.Models.BaseModels;
using Base.DAL.Seeding;
using Base.Services.HangFireJobs;
using Base.Services.Implementations;
using Base.Shared.Responses.Exceptions;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using TimeZoneConverter;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = args,
            WebRootPath = "wwwroot"
        });

        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddApplicationServices(builder.Configuration);
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        #region Seeding
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var loggerFactory = services.GetRequiredService<ILoggerFactory>();
            try
            {
                var dbContext = services.GetRequiredService<AppDbContext>();
                await dbContext.Database.MigrateAsync();

                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                await IdentitySeeder.SeedAdminAsync(userManager, roleManager);
                await IdentitySeeder.SeedLocationsAsync(dbContext);
            }
            catch (Exception ex)
            {
                var logger = loggerFactory.CreateLogger<Program>();
                logger.LogError(ex, "An error occurred during migration.");
            }
        }
        #endregion

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.RoutePrefix = string.Empty;
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "BePositive API V1");
        });

        app.UseStaticFiles();
        app.UseHttpsRedirection();
        app.UseResponseCompression();
        app.UseResponseCaching();
        app.UseRouting();
        app.UseCors("AllowAll");
        app.UseMiddleware<TokenBlacklistMiddleware>();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMiddleware<ErrorHandlingMiddleware>();
        app.UseMiddleware<SuccessResponseMiddleware>();

        app.MapControllers();
        app.MapHub<MessagingHub>("/hubs/messaging", options =>
        {
            options.Transports =
                Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets |
                Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling |
                Microsoft.AspNetCore.Http.Connections.HttpTransportType.ServerSentEvents;
        });

        app.MapHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = new[] { new AllowAllDashboardAuthorizationFilter() }
        });

        var cairoTimeZone = TZConvert.GetTimeZoneInfo("Africa/Cairo");

        RecurringJob.AddOrUpdate<CleanupBlacklistedTokensService>(
            "CleanupBlacklistedTokens",
            job => job.ExecuteAsync(),
            Cron.Hourly,
            new RecurringJobOptions { TimeZone = cairoTimeZone }
        );

        RecurringJob.AddOrUpdate<BloodInventoryExpiryJob>(
            "CheckExpiredBloodBatches",
            job => job.ExecuteAsync(),
            Cron.Daily,
            new RecurringJobOptions { TimeZone = cairoTimeZone }
        );

        app.MapFallback(async context =>
        {
            throw new NotFoundException("The requested endpoint does not exist.");
        });

        app.Run();
    }
}


////using Base.API.Authorization;
////using Base.API.Controllers;
////using Base.API.Filters;
////using Base.API.Hubs;
////using Base.API.MiddleWare;
////using Base.API.Services;
////using Base.DAL.Contexts;
////using Base.DAL.Models.BaseModels;
////using Base.DAL.Seeding;
////using Base.Services.HangFireJobs;
////using Base.Services.Implementations;
////using Base.Shared.Responses.Exceptions;
////using Hangfire;
////using Microsoft.AspNetCore.Identity;
////using Microsoft.EntityFrameworkCore;
////using System.IdentityModel.Tokens.Jwt;
////using TimeZoneConverter;
////internal class Program
////{
////    private static async Task Main(string[] args)
////    {
////        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
////        {
////            Args = args,
////            WebRootPath = "wwwroot" // هنا تحددي WebRoot قبل إنشاء الـApp
////        });

////        // 💡 إضافة الخطوة الوقائية لتعطيل تحويل المطالبات
////        // تمنع إعادة تسمية مطالبات 'sub' إلى 'nameidentifier' في ClaimsPrincipal
////        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
////        builder.Services.AddHttpContextAccessor();

////        // 💡 إضافة خدمات التحكم في الوصول عبر الأصول (CORS)
////        // 💡 إضافة خدمات الهوية
////        builder.Services.AddApplicationServices(builder.Configuration);

////        builder.Services.AddEndpointsApiExplorer();

////        builder.Services.AddSwaggerGen();


////        var app = builder.Build();

////        #region Seeding
////        // 💡 تنفيذ التهيئة الأولية للبيانات عند بدء التشغيل
////        using (var scope = app.Services.CreateScope())
////        {
////            var services = scope.ServiceProvider;

////            var LoggerFactory = services.GetRequiredService<ILoggerFactory>();

////            try
////            {
////                //await StoreContextSeeding.SeedAsync(dbContext);
////                var dbContext = services.GetRequiredService<AppDbContext>();
////                await dbContext.Database.MigrateAsync();//Apply Migration

////                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
////                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

////                await IdentitySeeder.SeedAdminAsync(userManager, roleManager);

////                await IdentitySeeder.SeedLocationsAsync(dbContext);
////                //await IdentitySeeder.SeedDataAsync(dbContext);

////            }
////            catch (Exception ex)
////            {
////                var logger = LoggerFactory.CreateLogger<Program>();
////                logger.LogError(ex, "an error occured during apply Migration");
////            }
////        }
////        #endregion
////        // 💡 تكوين الـ Middleware في الـ HTTP Request Pipeline
////        //  if (app.Environment.IsDevelopment())
////        //   {
////        app.UseSwagger();
////        // Inside Program.cs, after app.UseSwagger();

////        app.UseSwaggerUI(options =>
////        {
////            // 1. Set the root path for the UI (so the swagger page opens at the root)
////            options.RoutePrefix = string.Empty;

////            // 2. CRITICAL: Explicitly set the location of the swagger.json file.
////            // The default endpoint path is usually '/swagger/v1/swagger.json'.
////            // Your error is looking for '/v1/swagger.json' which is wrong.
////            options.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartWarehouse API V1");
////        });
////        //  }
////        //app.UseMiddleware<TokenBlacklistMiddleware>();

////        //app.UseStaticFiles();

////        ////// 🛡️ فرض HTTPS (أفضل ممارسة)
////        ////app.UseHttpsRedirection();

////        ////// 🌐 استخدام CORS
////        //////  app.UseCors("AllowSpecificOrigin");
////        ////app.UseCors("AllowAll");

////        ////// 💡 تفعيل Response Compression في الـ Pipeline
////        ////app.UseResponseCompression();

////        ////// 💡 تفعيل Response Caching
////        ////app.UseResponseCaching();
////        ////app.UseRouting();

////        ////// 🛡️ تفعيل المصادقة
////        ////app.UseAuthentication();

////        ////// 🛡️ تفعيل التفويض
////        ////app.UseAuthorization();
////        //app.UseHttpsRedirection();
////        //app.UseResponseCompression();
////        //app.UseResponseCaching();
////        //app.UseRouting();
////        //app.UseCors("AllowAll");      // ← moved AFTER UseRouting
////        //app.UseAuthentication();
////        //app.UseAuthorization();
////        //// 💡 إضافة Middleware لمعالجة الأخطاء
////        //app.UseMiddleware<ErrorHandlingMiddleware>();

////        //// 💡 إضافة Middleware لتغليف الاستجابات الناجحة
////        //app.UseMiddleware<SuccessResponseMiddleware>();





////        //// 💡 تعيين الخرائط للمتحكمات
////        //app.MapControllers();
////        //app.MapHub<MessagingHub>("/hubs/messaging");
////        app.UseStaticFiles();
////        app.UseHttpsRedirection();
////        app.UseResponseCompression();
////        app.UseResponseCaching();
////        app.UseRouting();
////        app.UseCors("AllowAll");                        // ← CORS first
////        app.UseMiddleware<TokenBlacklistMiddleware>();   // ← then blacklist
////        app.UseAuthentication();
////        app.UseAuthorization();
////        app.UseMiddleware<ErrorHandlingMiddleware>();
////        app.UseMiddleware<SuccessResponseMiddleware>();
////        app.MapControllers();
////        app.MapHub<MessagingHub>("/hubs/messaging");
////        //app.UseHangfireDashboard("/hangfire/index.html");
////        app.MapHangfireDashboard("/hangfire", new DashboardOptions
////        {
////            Authorization = new[] { new AllowAllDashboardAuthorizationFilter() }
////        });

////        // Cairo timezone
////        var cairoTimeZone = TZConvert.GetTimeZoneInfo("Africa/Cairo");

////        // 2) 💥 Job تنظيف الـ Blacklist (كل ساعة)
////        RecurringJob.AddOrUpdate<CleanupBlacklistedTokensService>(
////            "CleanupBlacklistedTokens",
////            job => job.ExecuteAsync(),
////            Cron.Hourly, // لو عايزة كل ساعتين: "0 */2 * * *"
////            new RecurringJobOptions
////            {
////                TimeZone = cairoTimeZone
////            }
////        );
////        RecurringJob.AddOrUpdate<BloodInventoryExpiryJob>(
////    "CheckExpiredBloodBatches",
////    job => job.ExecuteAsync(),
////    Cron.Daily,
////    new RecurringJobOptions { TimeZone = cairoTimeZone }
////);
////        // 💡 تعيين نقطة النهاية الافتراضية للتعامل مع الطلبات غير المعروفة
////        app.MapFallback(async context =>
////        {
////            throw new NotFoundException("The requested endpoint does not exist.");
////        });

////        // Hangfire dashboard

////        app.Run();
////    }
////}


