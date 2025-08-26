using System;
using System.Collections;
using System.Collections.Generic;

namespace IngameScript
{
    public static class TaskSchedulerExtensions
    {
        public static SEApplicationBuilder AddTaskScheduler(this SEApplicationBuilder builder, IEnumerable<MyTask> tasks = null)
        {
            builder.Services.AddSingleton<ISchedulerService, SchedulerService>(sp =>
            {
                var scheduler = new SchedulerService(sp.GetService<ILogger>());
                var logger = sp.GetService<ILogger>();
                if (tasks != null)
                {
                    foreach (var task in tasks)
                    {
                        var taskId = scheduler.AddTask(task);
                        if (taskId == -1)
                        {
                            logger.Error("Failed to add initial task to scheduler.");
                        }
                        else
                        {
                            logger.Info($"Added initial task to scheduler with ID: {taskId}");
                        }
                    }
                }
                return scheduler;
            });

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