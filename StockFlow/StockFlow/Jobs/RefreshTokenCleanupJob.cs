using Hangfire;
using StockFlow.Interfaces;

namespace StockFlow.Jobs
{
    public class RefreshTokenCleanupJob
    {
        private readonly IAuthService _authService;


        public RefreshTokenCleanupJob(IAuthService authService)
        {
            _authService = authService;
        }


        [DisableConcurrentExecution(timeoutInSeconds: 300)]
        [AutomaticRetry(Attempts = 3)]
        public async Task RunAsync()
        {
            await _authService.CleanupRefreshTokensAsync();
        }
    }
}

