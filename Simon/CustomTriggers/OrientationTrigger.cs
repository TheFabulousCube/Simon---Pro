using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System.Profile;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace Simon.CustomTriggers
{
    class OrientationTrigger : StateTriggerBase
    {
		public OrientationTrigger()
		{
            var _orient = ApplicationView.GetForCurrentView().Orientation;
            var _device = AnalyticsInfo.VersionInfo.DeviceFamily;

            Window.Current.SizeChanged += (s, e) =>
			{
                SetActive(ApplicationView.GetForCurrentView().Orientation.Equals(this.Orientation) && AnalyticsInfo.VersionInfo.DeviceFamily.Equals("Windows.Mobile"));
            };
			SetActive(ApplicationView.GetForCurrentView().Orientation.Equals(this.Orientation));
		}
		
	
	public ApplicationViewOrientation Orientation { get; set; }


    }


}
