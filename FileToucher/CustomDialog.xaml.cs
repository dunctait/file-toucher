using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FileToucher
{
    /// <summary>
    /// Interaction logic for CustomDialog.xaml
    /// </summary>
    public partial class CustomDialog : Window
    {

        private bool _result;

        public CustomDialog()
        {
            InitializeComponent();
        }

        public CustomDialog(string message, string title, bool showCancel)
        {
            InitializeComponent();

            TitleLabel.Content = title;
            MessageTextBox.Text = message;

            if (showCancel)
            {
                var cancelButton = new Button();

                var buttonStyle = FindResource("ButtonStyle") as Style;
                cancelButton.Style = buttonStyle;
                cancelButton.FontSize = 18;
                cancelButton.Margin = new Thickness(8);
                cancelButton.Click += CloseWindow;
                cancelButton.Content = "Cancel";
                ButtonStackPanel.Children.Add(cancelButton);
                
            }
            
        }

        public void CloseWindow(object sender, RoutedEventArgs routedEventArgs)
        {
            _result = false;
            Close();
            //CustomDialogWindow.Visibility = Visibility.Hidden;
        }

        public bool getResult()
        {
            return _result;
        }

        public void OkayClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            _result = true;
            Close();
        }

        private void TitleBarMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
