using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace bCoreDriver.Views.CustomViews
{
    class MyMessageDialog : ContentDialog
    {
        public MyMessageDialog(string message, string primaryText = null) : base()
        {
            Content = message;
            PrimaryButtonText = primaryText ?? "OK";
            IsPrimaryButtonEnabled = true;
            Title = "bCore Driver";
            HorizontalAlignment = HorizontalAlignment.Center;
            VerticalAlignment = VerticalAlignment.Center;
        }
    }
}
