using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace Simon.CustomTriggers
{
    class OrientationTrigger : StateTriggerBase
    {
		public OrientationTrigger()
		{
			Window.Current.SizeChanged += (s, e) =>
			{
				SetActive(ApplicationView.GetForCurrentView().Orientation.Equals(this.Orientation));
			};
			SetActive(ApplicationView.GetForCurrentView().Orientation.Equals(this.Orientation));
		}
		
		
	public ApplicationViewOrientation Orientation { get; set; }


    }


}
