using DiaryPro.NavigationParams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace DiaryPro
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomePage : Page
    {
        public HomePage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                NavParamBase navParam = (NavParamBase)e.Parameter;
                if(navParam.SourcePage.Equals(typeof(MainPage)) &&
                    navParam.TargetPage.Equals(typeof(HomePage)))
                {
                    string request = ((NavParamMainToHome)navParam).ExtraCommand;
                }
            }
        }

        private void btnNote_Click(object sender, RoutedEventArgs e)
        {
            NavParamHomeToNote navParam = new NavParamHomeToNote();
            navParam.SourcePage = typeof(HomePage);
            navParam.TargetPage = typeof(NotePage);
            Frame.Navigate(typeof(NotePage), navParam);
        }

        private void btnFile_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
