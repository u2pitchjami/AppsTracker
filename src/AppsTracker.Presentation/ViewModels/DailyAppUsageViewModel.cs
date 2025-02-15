#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AppsTracker.Common.Communication;
using AppsTracker.Data.Repository;
using AppsTracker.Domain;
using AppsTracker.Domain.Apps;
using AppsTracker.Domain.Tracking;
using AppsTracker.MVVM;

namespace AppsTracker.ViewModels
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public sealed class DailyAppUsageViewModel : ViewModelBase
    {
        private readonly Mediator mediator;
        private readonly IUseCase<AppDurationOverview> useCase;
        private readonly IRepository repository;
        private readonly ITrackingService trackingService;




        public override string Title
        {
            get { return "DAILY APP USAGE"; }
        }


        public object SelectedItem { get; set; }


        private readonly AsyncProperty<IEnumerable<AppDurationOverview>> appsList;



        public AsyncProperty<IEnumerable<AppDurationOverview>> AppsList
        {
            get { return appsList; }
        }


        [ImportingConstructor]
        public DailyAppUsageViewModel(IUseCase<AppDurationOverview> useCase,
                              Mediator mediator,
                              IRepository repository,
                              ITrackingService trackingService)
        {
            this.useCase = useCase;
            this.mediator = mediator;
            this.repository = repository;  // ✅ On stocke la référence
            this.trackingService = trackingService;  // ✅ On stocke la référence

            appsList = new TaskRunner<IEnumerable<AppDurationOverview>>(useCase.Get, this);

            this.mediator.Register(MediatorMessages.REFRESH_LOGS, new Action(appsList.Reload));

           
        }


        



    }
}
