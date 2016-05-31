using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using FileToucher.ViewModel;
using GalaSoft.MvvmLight.Messaging;


namespace FileToucher.View
{
    /// <summary>
    /// Interaction logic for FileToucherView.xaml
    /// </summary>
    public partial class FileToucherView
    {
        private bool _restoreIfMove;

        private readonly BitmapImage _restoreButtonImage = new BitmapImage(new Uri("/FileToucher;component/Resources/RestoreButton.png", UriKind.Relative));
        private readonly BitmapImage _maximizeButtonImage = new BitmapImage(new Uri("/FileToucher;component/Resources/MaximizeButton.png", UriKind.Relative));

        public static RoutedCommand OpenCommand = new RoutedCommand();
        public static RoutedCommand SaveCommand = new RoutedCommand();

        private readonly FileToucherViewModel _viewModel;

        public FileToucherView()
        {
            InitializeComponent();

            Messenger.Default.Register<NotificationMessage>(this, NotificationMessageReceived);

            OpenCommand.InputGestures.Add(new KeyGesture(Key.O, ModifierKeys.Control));
            SaveCommand.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));

            _viewModel = (FileToucherViewModel)DataContext;
        }

        /// <summary>
        /// Minimize Button in title bar clicked
        /// </summary>
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Restore Button in title bar clicked
        /// </summary>
        private void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchWindowState();
        }

        /// <summary>
        /// Handles Maximize/Restore logic (switches from maximized to normal and vice versa)
        /// </summary>
        private void SwitchWindowState()
        {
            switch (WindowState)
            {
                case WindowState.Normal:
                    {
                        WindowState = WindowState.Maximized;
                        RestoreButtonImage.Source = _restoreButtonImage;
                        break;
                    }
                case WindowState.Maximized:
                    {
                        WindowState = WindowState.Normal;
                        RestoreButtonImage.Source = _maximizeButtonImage;
                        break;
                    }
            }
        }

        /// <summary>
        /// Enables double-clicking on title bar to maximize and also click-to-drag on title bar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TitleBarMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if ((ResizeMode == ResizeMode.CanResize) || (ResizeMode == ResizeMode.CanResizeWithGrip))
                {
                    SwitchWindowState();
                }

                return;
            }

            if (WindowState == WindowState.Maximized)
            {
                _restoreIfMove = true;
                return;
            }

            DragMove();
        }

        private void TitleBarMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _restoreIfMove = false;
        }

        private void TitleBarMouseMove(object sender, MouseEventArgs e)
        {
            if (!_restoreIfMove) return;
            _restoreIfMove = false;

            var percentHorizontal = e.GetPosition(this).X / ActualWidth;
            var targetHorizontal = RestoreBounds.Width * percentHorizontal;

            var percentVertical = e.GetPosition(this).Y / ActualHeight;
            var targetVertical = RestoreBounds.Height * percentVertical;

            WindowState = WindowState.Normal;

            POINT lMousePosition;
            GetCursorPos(out lMousePosition);

            Left = lMousePosition.X - targetHorizontal;
            Top = lMousePosition.Y - targetVertical;

            DragMove();
        }

        private void AddDirectoryClicked(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog
            {
                AllowNonFileSystemItems = true,
                IsFolderPicker = true,
                Title = "Select folder"
            };

            if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
            {
                //MessageBox.Show("No Folder selected");
                return;
            }

            // get all the directories in selected dirctory
            var directory = dialog.FileName;

            _viewModel.AddDirectory(directory);
        }

        private void AddFilesClicked(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = true
            };

            if (dialog.ShowDialog() != true) return;

            _viewModel.AddFiles(dialog.FileNames);
        }

        private void OpenClicked(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedTouchFiles.Count > 0)
            {
                var warningDialog = new CustomDialog("If you open a saved list you will lose the files currently loaded into FileToucher", "Warning", true);
                warningDialog.ShowDialog();

                // if the user clicked cancel then don't open the file dialog
                if (!warningDialog.getResult())
                {
                    return;
                }
            }
            

            var dialog = new OpenFileDialog { Filter = "FileToucher file (*.ftfl) | *.ftfl" };
            if (dialog.ShowDialog() != true) return;

            // clear list

            var fileList = File.ReadAllLines(dialog.FileName);

            _viewModel.OpenFileList(fileList);
        }

        private void SaveClicked(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog { Filter = "FileToucher file (*.ftfl) | *.ftfl" };
            if (dialog.ShowDialog() != true) return;

            var saveLocation = dialog.FileName;

            _viewModel.SaveFileList(saveLocation);
        }

        /// <summary>
        /// This method will handle all messages received and call appropriate methods
        /// </summary>
        /// <param name="msg"></param>
        private void NotificationMessageReceived(NotificationMessage msg)
        {
            if (msg.Notification == "ShowDialog")
            {
                var newDialog = new CustomDialog {DataContext = DataContext};
                newDialog.ShowDialog();
            }
        }

        #region MaximizeCode

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            IntPtr mWindowHandle = (new WindowInteropHelper(this)).Handle;
            HwndSource.FromHwnd(mWindowHandle)?.AddHook(new HwndSourceHook(WindowProc));
        }

        private static System.IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x0024:
                    WmGetMinMaxInfo(hwnd, lParam);
                    break;
            }

            return IntPtr.Zero;
        }

        private static void WmGetMinMaxInfo(System.IntPtr hwnd, System.IntPtr lParam)
        {
            POINT lMousePosition;
            GetCursorPos(out lMousePosition);

            IntPtr lPrimaryScreen = MonitorFromPoint(new POINT(0, 0), MonitorOptions.MONITOR_DEFAULTTOPRIMARY);
            MONITORINFO lPrimaryScreenInfo = new MONITORINFO();
            if (GetMonitorInfo(lPrimaryScreen, lPrimaryScreenInfo) == false)
            {
                return;
            }

            IntPtr lCurrentScreen = MonitorFromPoint(lMousePosition, MonitorOptions.MONITOR_DEFAULTTONEAREST);

            MINMAXINFO lMmi = (MINMAXINFO) Marshal.PtrToStructure(lParam, typeof (MINMAXINFO));

            if (lPrimaryScreen.Equals(lCurrentScreen) == true)
            {
                lMmi.ptMaxPosition.X = lPrimaryScreenInfo.rcWork.Left;
                lMmi.ptMaxPosition.Y = lPrimaryScreenInfo.rcWork.Top;
                lMmi.ptMaxSize.X = lPrimaryScreenInfo.rcWork.Right - lPrimaryScreenInfo.rcWork.Left;
                lMmi.ptMaxSize.Y = lPrimaryScreenInfo.rcWork.Bottom - lPrimaryScreenInfo.rcWork.Top;
            }
            else
            {
                lMmi.ptMaxPosition.X = lPrimaryScreenInfo.rcMonitor.Left;
                lMmi.ptMaxPosition.Y = lPrimaryScreenInfo.rcMonitor.Top;
                lMmi.ptMaxSize.X = lPrimaryScreenInfo.rcMonitor.Right - lPrimaryScreenInfo.rcMonitor.Left;
                lMmi.ptMaxSize.Y = lPrimaryScreenInfo.rcMonitor.Bottom - lPrimaryScreenInfo.rcMonitor.Top;
            }

            Marshal.StructureToPtr(lMmi, lParam, true);
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out POINT lpPoint);


        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr MonitorFromPoint(POINT pt, MonitorOptions dwFlags);

        private enum MonitorOptions : uint
        {
            MONITOR_DEFAULTTONULL = 0x00000000,
            MONITOR_DEFAULTTOPRIMARY = 0x00000001,
            MONITOR_DEFAULTTONEAREST = 0x00000002
        }


        [DllImport("user32.dll")]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);


        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        };


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class MONITORINFO
        {
            public int cbSize = Marshal.SizeOf(typeof (MONITORINFO));
            public RECT rcMonitor = new RECT();
            public RECT rcWork = new RECT();
            public int dwFlags = 0;
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left, Top, Right, Bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                this.Left = left;
                this.Top = top;
                this.Right = right;
                this.Bottom = bottom;
            }
        }

        #endregion MaximizeCode
    }
}