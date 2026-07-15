using MPCRS.Services;

namespace MPCRS.Services
{
    public class EmailSyncBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EmailSyncBackgroundService> _logger;
        private readonly TimeSpan _period = TimeSpan.FromMinutes(5); // Sync every 5 minutes

        public EmailSyncBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<EmailSyncBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Email Sync Background Service started");

            // Wait 30 seconds before first run to allow application to fully start
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Starting email sync cycle");

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                        await emailService.SyncEmailsAsync();
                    }

                    _logger.LogInformation("Email sync cycle completed");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during email sync cycle");
                }

                // Wait for the next cycle
                await Task.Delay(_period, stoppingToken);
            }

            _logger.LogInformation("Email Sync Background Service stopped");
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Email Sync Background Service is stopping");
            await base.StopAsync(stoppingToken);
        }
    }
}