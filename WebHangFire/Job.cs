using Hangfire;
using Hangfire.Console;
using Hangfire.Dashboard.Management.Metadata;
using Hangfire.Server;
using System.ComponentModel;
using System.Reflection;

#pragma warning disable CS8625
#pragma warning disable CS8602

namespace WebHangFire
{
    public class Job
    {
        public Job()
        {
        }

        public static Type[] GetModuleTypes()
        {
            var assemblies = new[] { Assembly.GetEntryAssembly() };
            var moduleTypes = assemblies.SelectMany(f =>
            {
                try
                {
                    return f.GetTypes();
                }
                catch (Exception)
                {
                    return new Type[] { };
                }
            })
            .ToArray();

            return moduleTypes;
        }

        // ManagementPage 同名稱會被歸類在同一個群組裡面。
        // 但看不到第二個方法
        [ManagementPage("DemoJob_1")]
        public class DemoJob1
        {
            [Hangfire.Dashboard.Management.Support.Job]
            [DisplayName("呼叫內部方法")]
            public void Action(PerformContext context = null, IJobCancellationToken cancellationToken = null)
            {
                if (cancellationToken.ShutdownToken.IsCancellationRequested)
                {
                    return;
                }

                Console.WriteLine($"測試用v1，Now:{DateTime.Now}");
                Thread.Sleep(3000);
            }
        }

        [ManagementPage("DemoJob_2")]
        public class DemoJob2
        {
            [Hangfire.Dashboard.Management.Support.Job]
            [DisplayName("呼叫內部方法")]
            public void Action(PerformContext context = null, IJobCancellationToken cancellationToken = null)
            {
                if (cancellationToken.ShutdownToken.IsCancellationRequested)
                {
                    return;
                }

                Console.WriteLine($"測試用V2，Now:{DateTime.Now}");
                Thread.Sleep(3000);
            }
        }

        [ManagementPage("DemoJob_3")]
        public class DemoJob3
        {
            // [Queue("queue1")]
            [Hangfire.Dashboard.Management.Support.Job]
            [DisplayName("呼叫內部方法")]
            public void Action(PerformContext context = null, IJobCancellationToken cancellationToken = null)
            {
                if (cancellationToken.ShutdownToken.IsCancellationRequested)
                {
                    return;
                }

                Console.WriteLine($"測試用v3，Now:{DateTime.Now}");
                Thread.Sleep(3000);
            }
        }
    }
}