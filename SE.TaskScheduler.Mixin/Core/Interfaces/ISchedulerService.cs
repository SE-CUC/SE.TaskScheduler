using System.Collections;
using System.Collections.Generic;

namespace IngameScript
{
    public interface ISchedulerService
    {
        long AddTask(MyTask myTask);
        long AddTask(TaskType type, IEnumerator task, TaskPriority priority = TaskPriority.Normal);
        bool RemoveTask(long taskId);
        bool IsTaskRunning(long taskId);
        void Tick();
        IDictionary<long, TaskType> ActiveTasks { get; }
    }
}
