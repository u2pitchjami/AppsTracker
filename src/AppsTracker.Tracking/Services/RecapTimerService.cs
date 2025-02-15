using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Timers;
using AppsTracker.Domain.Tracking;
using AppsTracker.Data.Repository;  // 🔹 Assure que le Repository est bien accessible
using AppsTracker.Domain.Apps;  // 🔹 Pour AppDurationOverviewUseCase


namespace AppsTracker.Tracking.Services
{
    public class RecapTimerService
    {
        private readonly IRepository repository;
        private readonly ITrackingService trackingService;
        private Timer recapTimer;

        public RecapTimerService(IRepository repository, ITrackingService trackingService)
        {
            this.repository = repository;
            this.trackingService = trackingService;
            InitTimer(); // 🛠️ Démarrage immédiat du Timer
        }

        private void InitTimer()
        {
            recapTimer = new Timer();
            recapTimer.Elapsed += (sender, e) => RafraichirRecap();
            recapTimer.AutoReset = true;

            DateTime now = DateTime.Now;
            int minutesToNextSlot = 10 - (now.Minute % 10);
            TimeSpan initialDelay = TimeSpan.FromMinutes(minutesToNextSlot) - TimeSpan.FromSeconds(now.Second);

            Debug.WriteLine($">>> Recap Timer Service démarre dans {initialDelay.TotalSeconds} secondes.");

            Task.Delay(initialDelay).ContinueWith(_ =>
            {
                RafraichirRecap(); // 🔥 Première exécution immédiate
                recapTimer.Interval = 10 * 60 * 1000;
                recapTimer.Enabled = true;
            });
        }

        private void RafraichirRecap()
        {
            Debug.WriteLine(">>> Exécution automatique de Get() depuis RecapTimerService !");

            try
            {
                var useCase = new AppDurationOverviewUseCase(repository, trackingService);
                var result = useCase.Get(); // Exécution forcée de Get()

                if (repository == null)
                    Debug.WriteLine(">>> ERREUR : repository est NULL !");
                if (trackingService == null)
                    Debug.WriteLine(">>> ERREUR : trackingService est NULL !");

            }
            catch (Exception ex)
            {
                Debug.WriteLine($">>> ERREUR DANS Get() : {ex.Message}");
            }
        }
    }
}
