using Sandbox.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using VRage.Game.GUI.TextPanel;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        private readonly ILogger _logger;
        private readonly ISchedulerService _scheduler;
        private readonly SystemMonitorService _monitorService;

        public Program()
        {
            _monitorService = new SystemMonitorService(this);

            _logger = new Logger(LogLevel.Debug);

            var logSurface = Me.GetSurface(0);
            logSurface.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
            logSurface.FontSize = 0.5f;
            logSurface.Alignment = VRage.Game.GUI.TextPanel.TextAlignment.LEFT;
            _logger.AddTarget(new SurfaceTarget(logSurface));

            _logger.AddTarget(new EchoTarget(Echo));

            _scheduler = new SchedulerService(_logger);
            _logger.Info("Scheduler service initialized.");

            SetupTasks();

            Runtime.UpdateFrequency = UpdateFrequency.Update10;

            Echo("Scheduler is running. 100 parallel tasks with fake load initiated.");
            _logger.Info("--- Script startup complete ---");
        }

        public void Main(string argument, UpdateType updateSource)
        {
            _monitorService.PrintStatsToEcho();

            _scheduler.Tick();

            _logger.Flush();
        }

        private void SetupTasks()
        {
            _logger.Info("Setting up initial tasks...");

            for (int i = 0; i < 100; i++)
            {
                _scheduler.AddTask(TaskType.Parallel, OneOffParallelTaskWithLoad($"Parallel-{i}", 3));
            }

            _logger.Info("100 parallel tasks with fake load have been scheduled.");
        }

        private IEnumerator OneOffParallelTaskWithLoad(string name, int steps)
        {
            for (int i = 0; i < steps; i++)
            {
                _logger.Info($"<{name}> Parallel step {i + 1}/{steps}.");

                yield return CoroutineHelpers.Wait(1);
            }
            _logger.Info($"<{name}> finished.");
        }
    }
}