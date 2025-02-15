#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion


using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using AppsTracker.Tracking.Services;
using AppsTracker.Common.Logging;
using AppsTracker.Controllers;
using AppsTracker.Service;
using AppsTracker.Data.Repository;
using AppsTracker.Domain.Tracking;

[assembly: InternalsVisibleTo("AppsTracker.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace AppsTracker
{
    public partial class App : Application
    {
        private readonly ApplicationController applicationController;
        private readonly CompositionContainer container;

        private RecapTimerService recapTimerService;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            IRepository repository = new Repository(); // Adapter en fonction de ton projet
            ITrackingService trackingService = null; // Adapter en fonction de ton projet

            // 🛠️ Lancement automatique du Timer
            recapTimerService = new RecapTimerService(repository, trackingService);
        }

        public App(ReadOnlyCollection<string> args)
        {
            Debug.WriteLine(">>> AppsTracker a démarré !");
            ProfileOptimization.SetProfileRoot(Assembly.GetEntryAssembly().Location);
            ProfileOptimization.StartProfile("AppsTrackerProfile");

            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            InitializeComponent();

            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement)
                                                                , new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

            Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline)
                                                                , new PropertyMetadata() { DefaultValue = 40 });

            var context = new DispatcherSynchronizationContext(Application.Current.Dispatcher);
            SynchronizationContext.SetSynchronizationContext(context);

            container = GetCompositionContainer();
            ServiceLocation.ServiceLocator.Instance.Initialize(container);

            bool autostart = args.Where(a => a.ToUpper().Contains(Constants.CMD_ARGS_AUTOSTART)).Count() > 0;
            applicationController = container.GetExportedValue<ApplicationController>();
            applicationController.Initialize(autostart);

            this.SessionEnding += (s, e) => ShutdownApp();
        }


        private CompositionContainer GetCompositionContainer()
        {
            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly()));
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(AppsTracker.Data.Repository.IRepository).Assembly));
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(AppsTracker.Common.Communication.Mediator).Assembly));
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(AppsTracker.Tracking.ITrackingModule).Assembly));
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(AppsTracker.Views.MainWindow).Assembly));
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(AppsTracker.Domain.IUseCase<>).Assembly));
            var container = new CompositionContainer(catalog);
            var batch = new CompositionBatch();
            batch.AddExportedValue(container);
            container.Compose(batch);
            return container;
        }


        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var fail = (Exception)e.ExceptionObject;
            container.GetExportedValue<ILogger>().Log(fail);
            container.GetExportedValue<IWindowService>().ShowMessageDialog(fail);

            ShutdownApp();
        }


        internal void ShutdownApp()
        {
            container.Dispose();
            applicationController.Shutdown();
            Application.Current.Shutdown();
        }

        public App() { }
    }
}
