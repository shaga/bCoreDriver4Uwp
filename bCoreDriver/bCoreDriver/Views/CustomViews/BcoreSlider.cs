using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using bCoreDriver.Models;
using Prism.Commands;

namespace bCoreDriver.Views.CustomViews
{
    class BcoreSlider : Slider
    {
        public static readonly DependencyProperty CommandValueUpdatedProperty =
            DependencyProperty.Register(nameof(CommandValueUpdated), typeof(DelegateCommand<BcoreSliderParam>),
                typeof(BcoreSlider), new PropertyMetadata(null));

        public DelegateCommand<BcoreSliderParam> CommandValueUpdated
        {
            get { return GetValue(CommandValueUpdatedProperty) as DelegateCommand<BcoreSliderParam>; }
            set { SetValue(CommandValueUpdatedProperty, value); }
        }

        public static readonly DependencyProperty IsAutoResetProperty =
            DependencyProperty.Register(nameof(IsAutoReset), typeof(bool), typeof(BcoreSlider),
                new PropertyMetadata(false));

        public bool IsAutoReset
        {
            get { return (bool) GetValue(IsAutoResetProperty); }
            set { SetValue(IsAutoResetProperty, value); }
        }

        public static readonly DependencyProperty IndexProperty =
            DependencyProperty.Register(nameof(Index), typeof(int), typeof(BcoreSlider), new PropertyMetadata(0));

        public int Index
        {
            get { return (int) GetValue(IndexProperty); }
            set { SetValue(IndexProperty, value); }
        }

        public static readonly DependencyProperty FuncTypeProperty =
            DependencyProperty.Register(nameof(FuncType), typeof(ESliderType), typeof(BcoreSlider),
                new PropertyMetadata(ESliderType.Motor));

        public ESliderType FuncType
        {
            get { return (ESliderType) GetValue(FuncTypeProperty); }
            set { SetValue(FuncTypeProperty, value); }
        }

        public BcoreSlider() : base()
        {
            PointerCaptureLost += OnPoiterCaptureLost;
        }

        private async void OnPoiterCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            if (CommandValueUpdated == null) return;

            if (IsAutoReset) Value = 128;

            var param = new BcoreSliderParam(FuncType, Index);

            await CommandValueUpdated?.Execute(param);
        }
    }
}
