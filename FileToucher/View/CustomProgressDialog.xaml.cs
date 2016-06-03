using System.Windows;
using System.Windows.Input;
using FileToucher.ViewModel;

namespace FileToucher.View
{
    /// <summary>
    /// Interaction logic for CustomProgressDialog.xaml
    /// </summary>
    public partial class CustomProgressDialog : Window
    {

        private bool _result;

        public CustomProgressDialog()
        {
            InitializeComponent();
        }

        public bool GetResult()
        {
            return _result;
        }

        public void WorkCompleted()
        {
            _result = true;
            Close();
        }

        public void OkayClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            _result = true;
            Close();
        }

        public void StopClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            FileToucherViewModel _viewModel = (FileToucherViewModel) DataContext;
            _viewModel.StopThread = true;
            _result = true;
            Close();
        }

        private void TitleBarMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
