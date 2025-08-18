using System;

namespace IngameScript
{
    public static class TaskSchedulerExtensions
    {

        public static SEApplicationBuilder AddTaskScheduler(this SEApplicationBuilder builder)
        {
            builder.Services.AddSingleton<ISchedulerService, SchedulerService>(sp =>
                new SchedulerService(sp.GetService<ILogger>())
            );
          
            builder.OnMain((arg, update, sp) =>
            {
                var scheduler = sp.GetService<ISchedulerService>();
                if (scheduler != null)
                {
                    scheduler.Tick();
                }
            });

            return builder;
        }
    }
}