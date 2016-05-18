using System;
using System.Collections;
using System.Diagnostics;
using System.Windows;
using Ookii.Dialogs.Wpf;
using System.Windows.Controls;
using System.IO;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Win32;

namespace file_toucher
{
    public partial class MainWindow : Window
    {
        struct TouchFiles
        {
            public string filename { get; set; }
            public string directory { get; set; }
            public string fullpath { get; set; }
            public string extension { get; set; }
            public DateTime accessedOn { get; set; }
            public DateTime modifiedOn { get; set; }
            public DateTime createdOn { get; set; }
        }

        ObservableCollection<TouchFiles> selectedFiles = new ObservableCollection<TouchFiles>();

        public MainWindow()
        {
            // Use nicer Aero theme
            Uri uri = new Uri("PresentationFramework.Aero;V3.0.0.0;31bf3856ad364e35;component\\themes/aero.normalcolor.xaml", UriKind.Relative);
            Resources.MergedDictionaries.Add(Application.LoadComponent(uri) as ResourceDictionary);

            InitializeComponent();
            OpeningSetup();
            CreateTicker();
        }

        // Set up which options should be selected on boot-up
        private void OpeningSetup()
        {

            // Since "Set Last Written To Now" is probably the most common function, set up program for that

            CheckboxModified.IsChecked = true;

            ModifiedDate.IsEnabled = false;

            AccessedDate.IsEnabled = false;
            CheckboxAccessedNow.IsEnabled = false;

            CreatedDate.IsEnabled = false;
            CheckboxCreatedNow.IsEnabled = false;

            BindGrid();

        }

        // Create a timer to allow constant updating of UI elements
        private void CreateTicker()
        {
            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }

        // Every tick of timer update UI elements that feature the current time
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            // update times on screen
            StatusBarText.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

            if (CheckboxAccessedNow.IsChecked == true)
            {
                AccessedDate.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            }

            if (CheckboxModifiedNow.IsChecked == true)
            {
                ModifiedDate.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            }

            if (CheckboxCreatedNow.IsChecked == true)
            {
                CreatedDate.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            }


        }

        // Listener for when a new option (such as "Update Created On" etc) is ticked
        private void AttributeChecked(object sender, RoutedEventArgs e)
        {
            CheckBox lSender = (CheckBox)sender;
            switch (int.Parse((String)lSender.Tag))
            {
                case 0: // CheckboxAccessed was ticked
                    CheckboxAccessedNow.IsEnabled = true;
                    CheckboxAccessedNow.IsChecked = true;
                    break;
                case 1: // CheckboxModified was ticked
                    CheckboxModifiedNow.IsEnabled = true;
                    CheckboxModifiedNow.IsChecked = true;
                    break;
                case 2: // CheckboxCreated was ticked
                    CheckboxCreatedNow.IsEnabled = true;
                    CheckboxCreatedNow.IsChecked = true;
                    break;
            }
        }

        // Listener for when an option (such as "Update Created On" etc) is unticked
        private void AttributeUnchecked(object sender, RoutedEventArgs e)
        {
            CheckBox lSender = (CheckBox)sender;
            switch (int.Parse((String)lSender.Tag))
            {
                case 0: // CheckboxAccessed was unticked
                    CheckboxAccessedNow.IsEnabled = false;
                    CheckboxAccessedNow.IsChecked = false;
                    AccessedDate.Text = "";
                    AccessedDate.IsEnabled = false;
                    break;
                case 1: // CheckboxModified was unticked
                    CheckboxModifiedNow.IsEnabled = false;
                    CheckboxModifiedNow.IsChecked = false;
                    ModifiedDate.Text = "";
                    ModifiedDate.IsEnabled = false;
                    break;
                case 2: // CheckboxCreated was unticked
                    CheckboxCreatedNow.IsEnabled = false;
                    CheckboxCreatedNow.IsChecked = false;
                    CreatedDate.Text = "";
                    CreatedDate.IsEnabled = false;
                    break;
            }
        }

        // Listener for when user chooses to use the current time as the value to update an attribute
        private void CurrentTimeChecked(object sender, RoutedEventArgs e)
        {
            CheckBox lSender = (CheckBox)sender;
            switch (int.Parse((String)lSender.Tag))
            {
                case 0: // AccessedNow was ticked
                    AccessedDate.IsEnabled = false;
                    AccessedDate.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                    break;
                case 1: // ModifiedNow was ticked
                    ModifiedDate.IsEnabled = false;
                    ModifiedDate.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                    break;
                case 2: // CreatedNow was ticked
                    CreatedDate.IsEnabled = false;
                    CreatedDate.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                    break;
            }
        }

        // Listener for when user chooses to use a custom time as the value to update an attribute 
        private void CurrentTimeUnchecked(object sender, RoutedEventArgs e)
        {

            CheckBox lSender = (CheckBox)sender;
            switch (int.Parse((String)lSender.Tag))
            {
                case 0: // AccessedNow was unticked
                    if (CheckboxAccessed.IsChecked == true)
                    {
                        AccessedDate.IsEnabled = true;
                    }
                    break;
                case 1: // ModifiedNow was unticked
                    if (CheckboxModified.IsChecked == true)
                    {
                        ModifiedDate.IsEnabled = true;
                    }
                    break;
                case 2: // CreatedNow was unticked
                    if (CheckboxCreated.IsChecked == true)
                    {
                        CreatedDate.IsEnabled = true;
                    }
                    break;
            }
        }

        // Listener for when user requests to add a single file
        private void ButtonAddFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();

            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            dialog.Multiselect = true;

            if (dialog.ShowDialog() == true)
            {
                string errorFiles = "";
                int errors = 0;

                foreach (string filename in dialog.FileNames)
                {
                    AddFile(filename);
                    // if (certain errors) errorfiles += filename + "\n";
                    // errors++;
                }

                if (errors > 0) {
                    Xceed.Wpf.Toolkit.MessageBox.Show(this, string.Format("The following files couldn't be added:\n{0}", errorFiles),
                        "Error adding certain files");
                }
            }

        }

        // Listener for when user requests to add a directory
        private void ButtonAddDirectory_Click(object sender, RoutedEventArgs e)
        {

            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
            dialog.Description = "Please select a folder.";
            dialog.UseDescriptionForTitle = true; // This applies to the Vista style dialog only, not the old dialog.
            if (!VistaFolderBrowserDialog.IsVistaFolderDialogSupported)
                Xceed.Wpf.Toolkit.MessageBox.Show(this, "Because you are not using Windows Vista or later, the regular folder browser dialog will be used. Please use Windows Vista or later to see the new dialog.", "Sample folder browser dialog");
                // put in old dialog style

            object showDialog = dialog.ShowDialog(this);
            if (showDialog != null && (bool) showDialog)
            {

                //Xceed.Wpf.Toolkit.MessageBox.Show(this, "The selected folder was: " + dialog.SelectedPath, "Sample folder browser dialog");
                String[] allfiles = System.IO.Directory.GetFiles(dialog.SelectedPath, "*.*",
                    System.IO.SearchOption.AllDirectories);

                foreach (string foundFiles in allfiles)
                {
                    AddFile(foundFiles);
                }
            }
        }

        private void AddFile(string path)
        {
            // Check if file is already in list
            if (selectedFiles.Any(t => path == t.fullpath))
            {
                return;
            }

            TouchFiles toAdd = new TouchFiles();
            toAdd.accessedOn = File.GetLastAccessTime(path);
            toAdd.modifiedOn = File.GetLastWriteTime(path);
            toAdd.createdOn = File.GetCreationTime(path);

            toAdd.filename = Path.GetFileName(path);
            toAdd.directory = Path.GetDirectoryName(path);
            toAdd.extension = Path.GetExtension(path);
            toAdd.fullpath = path;

            selectedFiles.Add(toAdd);
        }

        // Bind DataGrid so that it automatically updates when files are added to the ObservableCollection
        private void BindGrid()
        {
            FilesDataGrid.ItemsSource = selectedFiles;
            FilesDataGrid.DataContext = selectedFiles;
        }

        // Listener for when user requests actual touch function to be carried out
        private void ButtonTouch_Click(object sender, RoutedEventArgs e)
        {
            if (CheckboxAccessed.IsChecked == false && CheckboxModified.IsChecked == false &&
                CheckboxCreated.IsChecked == false)
            {
                // Touch button clicked but no time is set to touch with. Show error
                MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show("You must select a time before touching.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DateTime current = DateTime.Now;

            DateTime touchAccessed = current;
            DateTime touchModified = current;
            DateTime touchCreated = current;

            string erroredFiles = "";
            bool anyErrors = false;

            int touches = 0;

            if (CheckboxAccessed.IsChecked == true)
            {
                if (CheckboxAccessedNow.IsChecked == false)
                {
                    touchAccessed = DateTime.Parse(AccessedDate.Text);
                }
            }

            if (CheckboxModified.IsChecked == true)
            {
                if (CheckboxModifiedNow.IsChecked == false)
                {
                    touchModified = DateTime.Parse(ModifiedDate.Text);
                }
            }

            if (CheckboxCreated.IsChecked == true)
            {
                if (CheckboxCreatedNow.IsChecked == false)
                {
                    touchCreated = DateTime.Parse(CreatedDate.Text);
                }
            }

            foreach (TouchFiles files in selectedFiles)
            {
                bool anyTouches = false;

                if (CheckboxAccessed.IsChecked == true)
                {
                    try
                    {
                        File.SetLastAccessTime(files.fullpath, touchAccessed);
                        anyTouches = true;
                    }
                    catch (Exception errorException)
                    {

                        erroredFiles += "Error: " + files.fullpath + " - " + errorException.ToString().Split('\n')[0] + "\n";
                        anyErrors = true;
                    }
                }

                if (CheckboxModified.IsChecked == true && !anyErrors)
                {
                    try
                    {
                        File.SetLastWriteTime(files.fullpath, touchModified);
                        anyTouches = true;
                    }
                    catch (Exception errorException)
                    {
                        erroredFiles += "Error: " + files.fullpath + " - " + errorException.ToString().Split('\n')[0] + "\n";
                        anyErrors = true;
                    }
                }

                if (CheckboxCreated.IsChecked == true && !anyErrors)
                {
                    try
                    {
                        File.SetCreationTime(files.fullpath, touchCreated);
                        anyTouches = true;
                    }
                    catch (Exception errorException)
                    {
                        erroredFiles += "Error: " + files.fullpath + " - " + errorException.ToString().Split('\n')[0] + "\n";
                        anyErrors = true;
                    }
                }

                if (anyTouches)
                {
                    touches++;
                }
            }

            string stringToShow = touches.ToString() + " Files touched.";

            if (anyErrors)
            {
                stringToShow += "\n\n" + erroredFiles;
            }

            MessageBoxResult touchResultMessageBox = Xceed.Wpf.Toolkit.MessageBox.Show(stringToShow, "Touch Results", MessageBoxButton.OK, MessageBoxImage.Information);

            refreshFileDatestamps();

        }

        // After touch function is carried out, file information needs updated so changes show in the DataGrid
        private void refreshFileDatestamps()
        {
            for (var i = 0; i<selectedFiles.Count; i++)
            {
                TouchFiles tempFile = selectedFiles[i];
                tempFile.accessedOn = File.GetLastAccessTime(tempFile.fullpath);
                tempFile.modifiedOn = File.GetLastWriteTime(tempFile.fullpath);
                tempFile.createdOn = File.GetCreationTime(tempFile.fullpath);

                selectedFiles[i] = tempFile;
            }
        }

        // Listener for when user asks to remove all selected items in DataGrid
        private void ButtonRemove_Click(object sender, RoutedEventArgs e)
        {

            // check at least one row selected
            if (FilesDataGrid.SelectedItems.Count < 1) return;

            // create a temporary list to house items that will be removed
            var toBeRemoved = new ArrayList();

            foreach (TouchFiles item in FilesDataGrid.SelectedItems) // for each of the files selected in the window
            {
                // add any files with a matching path to selected files in the toBeRemoved pile
                toBeRemoved.Add(selectedFiles.SingleOrDefault(i => i.fullpath == item.fullpath));
            }

            // iterate through all items to be removed and remove them
            foreach (TouchFiles item in toBeRemoved)
            {
                selectedFiles.Remove(item);
            }
        }

        // Listener for when user requests to remove all items from DataGrid
        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            selectedFiles.Clear();
        }

        private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuItem_About_Click(object sender, RoutedEventArgs e)
        {
            Xceed.Wpf.Toolkit.MessageBox.Show(this, "file-toucher 0.1 created by Duncan Tait.\nGithub repository: https://github.com/dunctait/file-toucher", "About file-toucher", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
