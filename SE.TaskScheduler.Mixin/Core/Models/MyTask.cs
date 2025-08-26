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
    }
}
