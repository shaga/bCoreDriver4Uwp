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
using LibBcore;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace bCoreDriver.Views.CustomViews
{
    public sealed partial class MotorControllerView : UserControl
    {
        public static DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(int),
            typeof(MotorControllerView), new PropertyMetadata(Bcore.StopMotorPwm));

        public int Value
        {
            get { return (int) GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static DependencyProperty IsEnabledControlProperty =
            DependencyProperty.Register(nameof(IsEnabledControl), typeof(bool), typeof(MotorControllerView),
                new PropertyMetadata(false));
        
        public bool IsEnabledControl
        {
            get { return (bool) GetValue(IsEnabledControlProperty); }
            set { SetValue(IsEnabledControlProperty, value); }
        }

        public static DependencyProperty LabelTextProperty =
            DependencyProperty.Register(nameof(LabelText), typeof(string), typeof(MotorControllerView),
                new PropertyMetadata(string.Empty));

        public string LabelText
        {
            get { return (string) GetValue(LabelTextProperty); }
            set { SetValue(LabelTextProperty, value); }
        }

        public MotorControllerView()
        {
            this.InitializeComponent();
        }
    }
}
