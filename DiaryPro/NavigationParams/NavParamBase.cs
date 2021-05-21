using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace DiaryPro.NavigationParams
{
    class NavParamBase
    {
        public Type SourcePage;
        public Type TargetPage;
        public string ExtraCommand;

        public NavParamBase()
        {
            SourcePage = typeof(MainPage);
            TargetPage = typeof(MainPage);
            ExtraCommand = "none";
        }
    }
}
