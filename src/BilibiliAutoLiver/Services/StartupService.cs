namespace BilibiliAutoLiver.Services
{
    public class StartupService : IStartupService
    {
        private readonly ILogger<StartupService> _logger;

        public StartupService(ILogger<StartupService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public  Task Start()
        {
            
            return Task.CompletedTask;
        }
    }
}
