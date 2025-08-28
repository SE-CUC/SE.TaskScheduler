using System;
using System.Collections;
using System.Text;

namespace IngameScript
{
    public class MyTask
    {
        public TaskType Type { get; set; }
        public IEnumerator Task { get; set; }
        public TaskPriority Priority { get; set; } = TaskPriority.Normal;

        public MyTask() { }

        public MyTask(TaskType type, IEnumerator task, TaskPriority priority = TaskPriority.Normal)
        {
            Type = type;
            Task = task;
            Priority = priority;
        }

        public MyTask(TaskType type, IEnumerable task, TaskPriority priority = TaskPriority.Normal)
        {
            Type = type;
            Task = task.GetEnumerator();
            Priority = priority;
        }
    }
}
