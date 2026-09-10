using Hangfire;
using StockFlow.Interfaces;

namespace StockFlow.Jobs
{
    public class ReservationExpirationJob
    {
        private readonly IStockReservationService _stockReservationService;

        public ReservationExpirationJob(IStockReservationService stockReservationService)
        {
            _stockReservationService = stockReservationService;
        }


        [DisableConcurrentExecution(timeoutInSeconds: 300)]
        [AutomaticRetry(Attempts = 3)]
        public async Task RunAsync()
        {
            await _stockReservationService.ExpireReservationsAsync();
        }
    }
}
