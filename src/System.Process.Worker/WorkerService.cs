using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Process.Worker.Commands;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace System.Process.Worker
{
    public class WorkerService : BackgroundService
    {
        #region Properties

        private ICommand Command { get; }
        private ILogger<WorkerService> Logger { get; set; }

        #endregion

        #region Constructor

        public WorkerService(ICommand command,
            ILogger<WorkerService> logger)
        {
            Command = command;
            Logger = logger;
        }

        #endregion

        #region Base overrides

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await Command.ExecuteAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Critical error, worker is going to be restarted");
                throw;
            }
        }

        #endregion
    }
}
