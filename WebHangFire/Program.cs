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
        // �s�WJob ��Ƶ{�̭��C
        .UseManagementPages(p => p.AddJobs(() => Job.GetModuleTypes()))
        .UseConsole()

        // �U�عϦ�
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

        // �|�۰ʲ��ͦbSql Server �̭��A���n���s�W��Ʈw�C
        .UseSqlServerStorage(Configuration.GetConnectionString("HangfireConnection"), new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.Zero,
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true
        }));

    // �]�wqueue������ ���O�S���ĪG
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
        // �s�W��Queue�̭�
        backgroundJobs.Schedule(() => Console.WriteLine("Function"), delay);

        // �s�W�Ƶ{��Schedule�̭�
        RecurringJob.AddOrUpdate("Function_Cron", () => Console.WriteLine("Function_Cron"), "*/1 * * * *");
        RecurringJob.AddOrUpdate("Function_Cron2", () => Console.WriteLine("Function_Cron2"), "*/2 * * * *");
    }

    app.UseStaticFiles();

    // �w�L�ɤ��� ��� Services.AddHangfireServer
    // app.UseHangfireServer();

    // �]�m�\�����Url�A������]�m"/"
    app.UseHangfireDashboard("/hangfire",
                             new DashboardOptions
                             {
                                 //�w�]���v�L�k�b�u�W���Ҩϥ� Hangfire.Dashboard.LocalRequestsOnlyAuthorizationFilter
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