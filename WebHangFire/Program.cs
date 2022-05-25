using Hangfire;
using Hangfire.Console;
using Hangfire.Dashboard.Management;
using Hangfire.SqlServer;
using WebHangFire;

try
{
    var builder = WebApplication.CreateBuilder(args);
    var Configuration = builder.Configuration;

    #region Register Service

    builder.Services.AddHangfire(configuration => configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
        .UseDashboardMetric(Hangfire.Dashboard.DashboardMetrics.RecurringJobCount)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseColouredConsoleLogProvider()
        // 新增Job 到排程裡面。
        .UseManagementPages(p => p.AddJobs(() => Job.GetModuleTypes()))
        .UseConsole()

        // 各種圖式
        //.UseDashboarddMetric(Hangfire.Dashboard.DashboardMetrics.ServerCount)
        //.UseDashboardMetric(Hangfire.Dashboard.DashboardMetrics.RecurringJobCount)
        //.UseDashboardMetric(Hangfire.Dashboard.DashboardMetrics.RetriesCount)
        //.UseDashboardMetric(Hangfire.Dashboard.DashboardMetrics.EnqueuedAndQueueCount)
        //.UseDashboardMetric(Hangfire.Dashboard.DashboardMetrics.ScheduledCount)
        //.UseDashboardMetric(Hangfire.Dashboard.DashboardMetrics.ProcessingCount)
        //.UseDashboardMetric(Hangfire.Dashboard.DashboardMetrics.SucceededCount)
        //.UseDashboardMetric(Hangfire.Dashboard.DashboardMetrics.FailedCount)
        //.UseDashboardMetric(Hangfire.Dashboard.DashboardMetrics.DeletedCount)
        //.UseDashboardMetric(Hangfire.Dashboard.DashboardMetrics.AwaitingCount)

        // 會自動產生在Sql Server 裡面，但要先新增資料庫。
        .UseSqlServerStorage(Configuration.GetConnectionString("HangfireConnection"), new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.Zero,
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true
        }));

    // 設定queue的類型 但是沒有效果
    var options = new BackgroundJobServerOptions
    {
        Queues = new[] { "queue1", "queue2", "queue3" }
    };

    // Add the processing server as IHostedService
    builder.Services.AddHangfireServer(op => op = options);

    // Add framework services.
    builder.Services.AddMvc();

    builder.Services.AddSingleton<IBackgroundJobClient, BackgroundJobClient>();

    #endregion Register Service

    #region Add the Pipeline

    var app = builder.Build();

    using (var serviceScope = app.Services.CreateScope())
    {
        var services = serviceScope.ServiceProvider;
        var backgroundJobs = services.GetRequiredService<IBackgroundJobClient>();

        TimeSpan delay = TimeSpan.FromMinutes(1);
        // 新增到Queue裡面
        backgroundJobs.Schedule(() => Console.WriteLine("Function"), delay);

        // 新增排程到Schedule裡面
        RecurringJob.AddOrUpdate("Function_Cron", () => Console.WriteLine("Function_Cron"), "*/1 * * * *");
        RecurringJob.AddOrUpdate("Function_Cron2", () => Console.WriteLine("Function_Cron2"), "*/2 * * * *");
    }

    app.UseStaticFiles();

    // 已過時不用 改用 Services.AddHangfireServer
    // app.UseHangfireServer();

    // 設置功能視窗Url，但不能設置"/"
    app.UseHangfireDashboard("/hangfire",
                             new DashboardOptions
                             {
                                 //預設授權無法在線上環境使用 Hangfire.Dashboard.LocalRequestsOnlyAuthorizationFilter
                                 // Authorization = new[] { new IDashboardAuthorizationFilter() }

                                 //AppPath = System.Web.VirtualPathUtility.ToAbsolute("~/"),
                                 //DisplayStorageConnectionString = false,
                                 //IsReadOnlyFunc = f => true
                             }
                            );

    app.MapGet("/", () => "Hello World!");

    app.Run();

    #endregion Add the Pipeline
}
catch (Exception ex)
{
    Console.Write(ex.ToString());
}