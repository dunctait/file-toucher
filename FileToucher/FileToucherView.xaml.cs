using System;
using System.Windows;
using WPF.Themes;

namespace FileToucher
{
    /// <summary>
    /// Interaction logic for FileToucherView.xaml
    /// </summary>
    public partial class FileToucherView : Window
    {
        public FileToucherView()
        {

            // Use nicer Aero theme
            var uri = new Uri("PresentationFramework.Aero;V3.0.0.0;31bf3856ad364e35;component\\themes/aero.normalcolor.xaml", UriKind.Relative);
            try
            {
                Resources.MergedDictionaries.Add(Application.LoadComponent(uri) as ResourceDictionary);
            }
            catch
            {
                // couldn't apply theme... leave it to stay as default
            }

            InitializeComponent();
        }
    }
}
