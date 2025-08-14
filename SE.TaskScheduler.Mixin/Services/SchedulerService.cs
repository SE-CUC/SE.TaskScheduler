using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace IngameScript
{
    public class SchedulerService : ISchedulerService
    {
        private readonly ILogger _logger;
        private long _nextTaskId = 0;

        private readonly LinkedList<KeyValuePair<long, IEnumerator>> _sequentialTasks = new LinkedList<KeyValuePair<long, IEnumerator>>();
        private readonly Dictionary<long, IEnumerator> _parallelTasks = new Dictionary<long, IEnumerator>();
        private readonly Dictionary<long, IEnumerator> _repetitiveTasks = new Dictionary<long, IEnumerator>();
        
        public IDictionary<long, TaskType> ActiveTasks
        {
            get
            {
                var allTasks = new Dictionary<long, TaskType>();
                foreach (var task in _sequentialTasks) { allTasks[task.Key] = TaskType.Sequential; }
                foreach (var task in _parallelTasks) { allTasks[task.Key] = TaskType.Parallel; }
                foreach (var task in _repetitiveTasks) { allTasks[task.Key] = TaskType.RepetitiveParallel; }
                return allTasks;
            }
        }

        public SchedulerService(ILogger logger)
        {
            _logger = logger;
        }

        public long AddTask(TaskType type, IEnumerator task, TaskPriority priority = TaskPriority.Normal)
        {
            if (task == null) return -1;
            
            var taskId = _nextTaskId++;
            
            if (priority == TaskPriority.Immediate)
            {
                _logger.Info($"Executing immediate task ID: {taskId}...");
                ExecuteImmediate(task);
                _logger.Info($"Immediate task ID: {taskId} finished.");
                return taskId;
            }

            switch (type)
            {
                case TaskType.Sequential:
                    var taskPair = new KeyValuePair<long, IEnumerator>(taskId, task);
                    if (priority == TaskPriority.High)
                        _sequentialTasks.AddFirst(taskPair);
                    else
                        _sequentialTasks.AddLast(taskPair);
                    break;
                case TaskType.Parallel:
                    _parallelTasks[taskId] = task;
                    break;
                case TaskType.RepetitiveParallel:
                    _repetitiveTasks[taskId] = task;
                    break;
            }
            _logger.Debug($"Task ID: {taskId} ({type}) added.");
            return taskId;
        }

        public bool RemoveTask(long taskId)
        {
            var sequentialTaskNode = _sequentialTasks.FirstOrDefault(pair => pair.Key == taskId);
            if (sequentialTaskNode.Value != null && sequentialTaskNode.Key == taskId)
            {
                _sequentialTasks.Remove(sequentialTaskNode);
                _logger.Info($"Sequential task ID: {taskId} removed.");
                return true;
            }
            if (_parallelTasks.Remove(taskId))
            {
                _logger.Info($"Parallel task ID: {taskId} removed.");
                return true;
            }
            if (_repetitiveTasks.Remove(taskId))
            {
                _logger.Info($"Repetitive task ID: {taskId} removed.");
                return true;
            }
            
            _logger.Error($"Failed to remove task. ID not found: {taskId}.");
            return false;
        }

        public bool IsTaskRunning(long taskId)
        {
            return _sequentialTasks.Any(pair => pair.Key == taskId) ||
                   _parallelTasks.ContainsKey(taskId) ||
                   _repetitiveTasks.ContainsKey(taskId);
        }

        public void Tick()
        {
            if (_sequentialTasks.Count > 0)
            {
                var taskPair = _sequentialTasks.First.Value;
                if (!ProcessTask(taskPair.Value))
                {
                    _sequentialTasks.RemoveFirst();
                    _logger.Debug($"Sequential task ID: {taskPair.Key} finished.");
                }
            }

            var parallelIdsToRemove = new List<long>();
            foreach (var taskPair in _parallelTasks.ToList())
            {
                if (!ProcessTask(taskPair.Value))
                {
                    parallelIdsToRemove.Add(taskPair.Key);
                    _logger.Debug($"Parallel task ID: {taskPair.Key} finished.");
                }
            }
            parallelIdsToRemove.ForEach(id => _parallelTasks.Remove(id));

            var repetitiveIds = _repetitiveTasks.Keys.ToList();
            foreach (var taskId in repetitiveIds)
            {
                if (_repetitiveTasks.ContainsKey(taskId))
                {
                    ProcessTask(_repetitiveTasks[taskId]);
                }
            }
        }

        private void ExecuteImmediate(IEnumerator task)
        {
            while (ProcessTask(task)) { }
        }

        private bool ProcessTask(IEnumerator task)
        {
            var nestedTask = task.Current as IEnumerator;
            if (nestedTask != null && ProcessTask(nestedTask))
            {
                return true;
            }
            return task.MoveNext();
        }
    }
}
