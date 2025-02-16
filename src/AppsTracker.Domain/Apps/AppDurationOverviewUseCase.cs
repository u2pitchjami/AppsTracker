using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using AppsTracker.Data.Models;
using AppsTracker.Data.Repository;
using AppsTracker.Domain.Tracking;

namespace AppsTracker.Domain.Apps
{
    [Export(typeof(IUseCase<AppDurationOverview>))]
    public sealed class AppDurationOverviewUseCase : IUseCase<AppDurationOverview>
    {
        private readonly IRepository repository;
        private readonly ITrackingService trackingService;

        [ImportingConstructor]
        public AppDurationOverviewUseCase(IRepository repository, ITrackingService trackingService)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
            this.trackingService = trackingService; // Peut être null, on le gère
        }

        public IEnumerable<AppDurationOverview> Get()
        {
            Debug.WriteLine(">>> useCase.Get() a bien été appelé !");

            try
            {
                if (repository == null)
                {
                    Debug.WriteLine(">>> ERREUR : repository est NULL !");
                    return Enumerable.Empty<AppDurationOverview>();
                }

                Debug.WriteLine(">>> Récupération de TOUS les logs du jour sans filtre utilisateur.");

                // 1️⃣ Récupération des logs du jour avec chargement de Window
                var logs = repository.GetFiltered<Log>(
                    l => l.DateCreated >= DateTime.Today,
                    l => l.Window // 🔥 Charge explicitement la fenêtre pour éviter NULL
                ).ToList();

                Debug.WriteLine($">>> Nombre de logs récupérés : {logs.Count}");

                if (logs.Count == 0)
                {
                    Debug.WriteLine(">>> Aucun log trouvé, arrêt du traitement.");
                    return Enumerable.Empty<AppDurationOverview>();
                }

                // 2️⃣ Transformation des logs en entrées Recap avec conversion UsageID -> UserID
                var recapEntries = logs
                    .SelectMany(log => SplitLogIntoTimeSlots(log)
                        .Select(slot =>
                        {
                            Debug.WriteLine($">>> Tranche générée : Timestamp = {slot.Slot}, Duration = {slot.Duration}");
                            return new { log, slot };
                        })
                    )
                    .Select(data =>
                    {
                        Debug.WriteLine($">>> Vérification avant recherche UserID : UsageID = {data.log.UsageID}");

                        var userQuery = repository.GetFiltered<Usage>(u => u.UsageID == data.log.UsageID);
                        int userID = (userQuery != null && userQuery.Any())
                            ? userQuery.Select(u => u.UserID).FirstOrDefault()
                            : -1; // 🔥 Valeur par défaut si aucun UserID trouvé
                        string userName = repository.GetFiltered<Uzer>(u => u.ID == userID)  // ✅ Corrigé avec "Uzer"
                           .Select(u => u.Name)  // ✅ Correct
                           .FirstOrDefault() ?? "Unknown";
                        Debug.WriteLine($">>> UserID trouvé : {userID}");
                        Debug.WriteLine($">>> UserName trouvé : {userName}");
                        Debug.WriteLine($">>> Vérification avant recherche ApplicationID : WindowID = {data.log.WindowID}");
                        var windowQuery = repository.GetFiltered<Window>(w => w.ID == data.log.WindowID);
                        if (windowQuery == null || !windowQuery.Any())
                        {
                            Debug.WriteLine($">>> ⚠️ PROBLÈME : Aucune entrée trouvée dans Windows pour WindowID = {data.log.WindowID}");
                        }
                        string windowTitle = repository.GetFiltered<Window>(w => w.ID == data.log.WindowID)
                           .Select(w => w.Title)  // ✅ Correct
                           .FirstOrDefault() ?? "Unknown";
                        int appID = (windowQuery != null && windowQuery.Any())
                            ? windowQuery.Select(w => w.ApplicationID).FirstOrDefault()
                            : -1; // 🔥 Valeur par défaut si aucun ApplicationID trouvé
                        string appName = repository.GetFiltered<Aplication>(a => a.ID == appID)  // ✅ Corrigé avec "Aplication"
                           .Select(a => a.Name)  // ✅ Correct
                           .FirstOrDefault() ?? "Unknown";
                        Debug.WriteLine($">>> ApplicationID trouvé : {appID}");
                        Debug.WriteLine($">>> ApplicationName trouvé : {appName}");

                        return new Recap
                        {
                            Timestamp = data.slot.Slot.AddMinutes(10),
                            UserID = userID,
                            UserName = userName,  // ✅ Corrigé avec les bons champs
                            ApplicationID = appID,
                            ApplicationName = appName,  // ✅ Corrigé avec les bons champs
                            WindowID = data.log.WindowID,
                            WindowTitle = windowTitle,  // ✅ Corrigé avec les bons champs
                            Duration = data.slot.Duration
                        };
                    })
                    .GroupBy(r => new { r.Timestamp, r.UserID, r.ApplicationID, r.WindowID }) // 🔥 GroupBy incluant WindowID
                    .Select(g => new Recap
                    {
                        Timestamp = g.Key.Timestamp,
                        UserID = g.Key.UserID,
                        UserName = g.First().UserName,  // ✅ On prend le premier UserName du groupe
                        ApplicationID = g.Key.ApplicationID,
                        ApplicationName = g.First().ApplicationName,  // ✅ On prend le premier ApplicationName du groupe
                        WindowID = g.Key.WindowID,
                        WindowTitle = g.First().WindowTitle,
                        Duration = g.Sum(r => r.Duration) // 🔥 Consolidation des durées
                    })
                    .ToList();

                Debug.WriteLine($">>> Nombre d'entrées prêtes pour Recap : {recapEntries.Count}");
                // Juste avant d'insérer dans Recap, on regroupe bien par tranche de 10 minutes
                recapEntries = recapEntries
    .GroupBy(r => new {
        Tranche = new DateTime(r.Timestamp.Year, r.Timestamp.Month, r.Timestamp.Day, r.Timestamp.Hour, (r.Timestamp.Minute / 10) * 10, 0),
        r.UserID,
        r.ApplicationID,
        r.WindowID
    })
    .Select(g => new Recap
    {
        Timestamp = g.Key.Tranche,
        UserID = g.Key.UserID,
        UserName = g.First().UserName,
        ApplicationID = g.Key.ApplicationID,
        ApplicationName = g.First().ApplicationName,
        WindowID = g.Key.WindowID,
        WindowTitle = g.First().WindowTitle,
        Duration = g.Select(r => r.Duration).Distinct().Sum() // 🔥 Évite les cumuls de périodes
    })
    .ToList();

                // 3️⃣ Insertion des entrées dans Recap
                foreach (var entry in recapEntries)
                {
                    bool exists = repository.GetFiltered<Recap>(
                        r => r.Timestamp == entry.Timestamp
                        && r.UserID == entry.UserID
                        && r.ApplicationID == entry.ApplicationID
                        && r.WindowID == entry.WindowID
                        && !string.IsNullOrEmpty(r.UserName)
                        && !string.IsNullOrEmpty(r.ApplicationName)
                        && !string.IsNullOrEmpty(r.WindowTitle)
                    ).Any();
                    Debug.WriteLine($">>> 🏆 Recap : Timestamp = {entry.Timestamp.ToString("yyyy-MM-dd HH:mm:ss")}, WindowID = {entry.WindowID}, Duration = {entry.Duration} sec");

                    if (!exists)
                    {
                        Debug.WriteLine($">>> 🔥 Tentative d'insertion : {entry.Timestamp}, {entry.UserID}, {entry.ApplicationID}, {entry.WindowID}, {entry.Duration}");
                        repository.SaveNewEntity(entry);
                        Debug.WriteLine($">>> ✅ Insertion réussie pour : {entry.Timestamp}, {entry.UserID}, {entry.ApplicationID}, {entry.WindowID}, {entry.Duration}");
                    }
                    else
                    {
                        Debug.WriteLine($">>> ❌ Entrée déjà existante : {entry.Timestamp}, {entry.UserID}, {entry.ApplicationID}, {entry.WindowID}");
                    }
                }

                return recapEntries.GroupBy(r => r.Timestamp.Date)
                    .Select(g => new AppDurationOverview
                    {
                        Date = g.Key.ToShortDateString(),
                        AppCollection = g.Select(r => new AppDuration
                        {
                            Name = repository.GetFiltered<Aplication>(a => a.ID == r.ApplicationID)
                                             .FirstOrDefault()?.Name ?? "Unknown",
                            Duration = Math.Round(TimeSpan.FromSeconds(r.Duration).TotalHours, 1)
                        }).ToList()
                    }).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($">>> ERREUR DANS Get() : {ex.Message}");
                Debug.WriteLine($">>> STACK TRACE : {ex.StackTrace}");
                return Enumerable.Empty<AppDurationOverview>();
            }
        }

        /// <summary>
        /// Fonction qui découpe un log en plusieurs tranches de 10 minutes
        /// </summary>
        private List<(DateTime Slot, long Duration)> SplitLogIntoTimeSlots(Log log)
        {
            List<(DateTime Slot, long Duration)> timeSlots = new List<(DateTime, long)>();

            DateTime start = log.DateCreated;
            DateTime end = log.DateEnded;

            while (start < end)
            {
                // Déterminer la fin de la tranche de 10 minutes
                DateTime nextSlot;
                if (start.Minute >= 50) // Si on est proche de la fin de l'heure
                {
                    nextSlot = new DateTime(start.Year, start.Month, start.Day, start.Hour, 59, 59).AddSeconds(1); // Passe à l'heure suivante
                }
                else
                {
                    nextSlot = new DateTime(start.Year, start.Month, start.Day, start.Hour, (start.Minute / 10 * 10) + 10, 0);
                }

                if (nextSlot > end) nextSlot = end; // Ajustement si le log finit avant la tranche

                long durationInThisSlot = (long)(nextSlot - start).TotalSeconds;

                timeSlots.Add((new DateTime(start.Year, start.Month, start.Day, start.Hour, (start.Minute / 10 * 10), 0), durationInThisSlot));

                // Passer à la tranche suivante
                start = nextSlot;
            }

            return timeSlots;
        }
    
    }
}
