using Prism.Modularity;
using Prism.Regions;
using System;

namespace LBManager.Modules.ScheduleManage
{
    public class ScheduleManageModule : IModule
    {
        IRegionManager _regionManager;

        public ScheduleManageModule(RegionManager regionManager)
        {
            _regionManager = regionManager;
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }
    }
}