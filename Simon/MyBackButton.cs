using Simon.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Simon
{
    public sealed class MyBackButton : Button
    {
        public MyBackButton()
        {
            this.DefaultStyleKey = this.GetType();
            this.Click += GoBack_Click;
        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            var _Frame = Window.Current.Content as Frame;
          //  _Frame.Navigate(typeof(About));
            if (_Frame.CanGoBack)
            {
                
                _Frame.GoBack();
            }
        }
    }

    public sealed class HomeButton : Button
    {
        public HomeButton()
        {
            this.DefaultStyleKey = this.GetType();
            this.Click += Home_Click;
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            var _Frame = Window.Current.Content as Frame;
            _Frame.Navigate(typeof(MainPage));

        }
    }
}
