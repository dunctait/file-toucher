using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using Microsoft.Win32;
using System.Linq;
using System.IO;
using System.Windows;
using System.Windows.Media.Animation;
using Ookii.Dialogs.Wpf;

namespace FileToucher
{
    class FileToucherViewModel : INotifyPropertyChanged
    {
        #region Variables

        public event PropertyChangedEventHandler PropertyChanged;

        // Create Observable Collection for all files that are selected by the user and shown in DataGrid
        private readonly ObservableCollection<TouchFiles> _selectedFiles = new ObservableCollection<TouchFiles>();

        // Create list of items that are selected in DataGrid
        private IList _selectedRowsList = new ArrayList();

        // Create array that holds booleans to determine whether the different attributes checkboxes are ticked or not
        private readonly bool[] _attributesToChangeBools = new bool[3];

        // Create array that holds booleans to determine whether the different DateTime pickers are enabled or not
        private readonly bool[] _attributeDateTimeBools = new bool[3];

        // Create array that holds the actual strings shown in the DateTime picker
        // n.b. strings here should really only be "" (blank) or parseable using DateTime.Parse()
        private readonly string[] _attributeDateTimes = new string[3];

        // _attributesNowBools describes whether checkboxes are ticked
        private readonly bool[] _attributesNowBools = new bool[3];

        // _attributesNowEnabledBools describes whether the checkboxes are enabled
        private readonly bool[] _attributesNowEnabledBools = new bool[3];

        // Create string for holding the status bar message
        private string _statusBarText = "";

        #endregion Variables

        #region Properties
        // The following 3 properties are "Attributes To Set" checking and unchecking functionality
        public bool AccessedCheck
        {
            get { return _attributesToChangeBools[0]; }
            set
            {
                if (_attributesToChangeBools[0] == value) { return; }
                _attributesToChangeBools[0] = value;
                RaisePropertyChanged("AccessedCheck");
                AccessedNowCheckboxEnable = value;
                AccessedNowCheck = value;

                // If we are unchecking the attribute, we want the DateTimePicker disabled
                if (!value)
                {
                    AccessedDateTimeEnable = false;
                }
            }
        }

        public bool ModifiedCheck
        {
            get { return _attributesToChangeBools[1]; }
            set
            {
                if (_attributesToChangeBools[1] == value) { return; }
                _attributesToChangeBools[1] = value;
                RaisePropertyChanged("ModifiedCheck");
                ModifiedNowCheckboxEnable = value;
                ModifiedNowCheck = value;

                // If we are unchecking the attribute, we want the DateTimePicker disabled
                if (!value)
                {
                    ModifiedDateTimeEnable = false;
                }
            }
        }

        public bool CreatedCheck
        {
            get { return _attributesToChangeBools[2]; }
            set
            {
                if (_attributesToChangeBools[2] == value) { return; }
                _attributesToChangeBools[2] = value;
                RaisePropertyChanged("CreatedCheck");
                CreatedNowCheckboxEnable = value;
                CreatedNowCheck = value;

                // If we are unchecking the attribute, we want the DateTimePicker disabled
                if (!value)
                {
                    CreatedDateTimeEnable = false;
                }
            }
        }

        // The following 3 properties are "Current Time" checkboxes checking and unchecking functionality
        public bool AccessedNowCheck
        {
            get { return _attributesNowBools[0]; }
            set
            {
                _attributesNowBools[0] = value;
                RaisePropertyChanged("AccessedNowCheck");

                // if unchecking/checking "Now" but still touching Accessed attribute: enable/disable DatePicker respectively
                if (AccessedCheck) { AccessedDateTimeEnable = !value; }
            }
        }

        public bool ModifiedNowCheck
        {
            get { return _attributesNowBools[1]; }
            set
            {
                _attributesNowBools[1] = value;
                RaisePropertyChanged("ModifiedNowCheck");

                // if unchecking/checking "Now" but still touching Modified attribute: enable/disable DatePicker respectively
                if (ModifiedCheck) { ModifiedDateTimeEnable = !value; }
            }
        }

        public bool CreatedNowCheck
        {
            get { return _attributesNowBools[2]; }
            set
            {
                _attributesNowBools[2] = value;
                RaisePropertyChanged("CreatedNowCheck");

                // if unchecking/checking "Now" but still touching Created attribute: enable/disable DatePicker respectively
                if (CreatedCheck) { CreatedDateTimeEnable = !value ;}
            }
        }

        // The following 3 properties are "Current Time" checkbox enabling and disabling functionality
        public bool AccessedNowCheckboxEnable
        {
            get { return _attributesNowEnabledBools[0]; }
            set { _attributesNowEnabledBools[0] = value; RaisePropertyChanged("AccessedNowCheckboxEnable"); }
        }

        public bool ModifiedNowCheckboxEnable
        {
            get { return _attributesNowEnabledBools[1]; }
            set { _attributesNowEnabledBools[1] = value; RaisePropertyChanged("ModifiedNowCheckboxEnable"); }
        }

        public bool CreatedNowCheckboxEnable
        {
            get { return _attributesNowEnabledBools[2]; }
            set { _attributesNowEnabledBools[2] = value; RaisePropertyChanged("CreatedNowCheckboxEnable"); }
        }

        // The following 3 properties are the DateTimePicker enabling/disabling functionality
        public bool AccessedDateTimeEnable
        {
            get { return _attributeDateTimeBools[0]; }
            set
            {
                // if enabling set to current time, else set to ""
                AccessedDateTime = value ? DateTime.Now.ToString() : "";

                _attributeDateTimeBools[0] = value;
                RaisePropertyChanged("AccessedDateTimeEnable");
            }
        }

        public bool ModifiedDateTimeEnable
        {
            get { return _attributeDateTimeBools[1]; }
            set
            {
                // if enabling set to current time, else set to ""
                ModifiedDateTime = value ? DateTime.Now.ToString() : "";

                _attributeDateTimeBools[1] = value;
                RaisePropertyChanged("ModifiedDateTimeEnable");

            }
        }

        public bool CreatedDateTimeEnable
        {
            get { return _attributeDateTimeBools[2]; }
            set
            {
                // if enabling set to current time, else set to ""
                CreatedDateTime = value ? DateTime.Now.ToString() : "";

                _attributeDateTimeBools[2] = value;
                RaisePropertyChanged("CreatedDateTimeEnable");
                
            }
        }

        // The following 3 properties control the setting and getting of the string showing the DateTime selected
        public string AccessedDateTime
        {
            get { return _attributeDateTimes[0]; }
            set { _attributeDateTimes[0] = value; RaisePropertyChanged("AccessedDateTime"); }
        }

        public string ModifiedDateTime
        {
            get { return _attributeDateTimes[1]; }
            set { _attributeDateTimes[1] = value; RaisePropertyChanged("ModifiedDateTime"); }
        }

        public string CreatedDateTime
        {
            get { return _attributeDateTimes[2]; }
            set { _attributeDateTimes[2] = value; RaisePropertyChanged("CreatedDateTime"); }
        }

        // Create property that returns the _selectedFiles collection to allow binding to DataGrid
        public ObservableCollection<TouchFiles> SelectedTouchFiles => _selectedFiles;

        // Create property that returns or sets the _selectedRows list to allow us to remove them
        public IList SelectedRows
        {
            get { return _selectedRowsList; }
            set
            {
                _selectedRowsList = value;
                RaisePropertyChanged("TestSelected");
            }
        }

        // Create property that returns or sets the _statusBarText string
        public string StatusBarText
        {
            get { return _statusBarText; }
            set { _statusBarText = value; RaisePropertyChanged("StatusBarText"); }
        }

        // The following ICommands are for binding button clicks to methods
        public ICommand AddFilesClicked => new DelegateCommand(AddFiles);

        public ICommand AddDirectoryClicked => new DelegateCommand(AddDirectory);
        public ICommand RemoveSelectedClicked => new DelegateCommand(RemoveSelected);
        public ICommand RemoveAllClicked => new DelegateCommand(RemoveAll);

        public ICommand TouchFilesClicked => new DelegateCommand(TouchFiles);

        public ICommand ExitClicked => new DelegateCommand(Exit);
        public ICommand AboutClicked => new DelegateCommand(ShowAbout);

        #endregion Properties

        /// <summary>
        /// FileToucherViewModel Constructor
        /// </summary>
        public FileToucherViewModel()
        {
            OpeningSetup();
        }

        /// <summary>
        /// When program begins, load up default settings or saved settings from previous session
        /// </summary>
        private void OpeningSetup()
        {
            // TODO: Load up previous session settings here once they are saved somewhere

            // Set the checkboxes we want ticked to true at startup
            AccessedCheck = false;
            ModifiedCheck = true;
            CreatedCheck = false;

            StatusBarText = "Program started.";

        }

        /// <summary>
        /// Notifies a bound view that a property has changed, instigating an update on the view
        /// </summary>
        /// <param name="propertyName"></param>
        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Creates file browser dialog, and passes selected files to AddFile() method
        /// </summary>
        public void AddFiles()
        {
            var dialog = new OpenFileDialog
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Multiselect = true
            };

            if (dialog.ShowDialog() != true) return;

            var successfulAdds = 0;

            foreach (var filename in dialog.FileNames)
            {
                if (AddFile(filename)) successfulAdds++;
            }

            switch (successfulAdds)
            {
                case 0:
                    StatusBarText = "No files were added to list.";
                    break;
                case 1:
                    StatusBarText = "1 file added to list.";
                    break;
                default:
                    StatusBarText = successfulAdds.ToString() + " files added to list.";
                    break;
            }

        }

        /// <summary>
        /// Creates folder browser dialog, passes each file in folder and subfolder to AddFile()
        /// </summary>
        public void AddDirectory()
        {
            var dialog = new VistaFolderBrowserDialog
            {
                Description = "Please select a folder.",
                UseDescriptionForTitle = true
            };

            object showDialog = dialog.ShowDialog();

            if (showDialog == null || !(bool) showDialog) return;

            var successfulAdds = RecursiveFolderSearch(dialog.SelectedPath, 0);

            switch (successfulAdds)
            {
                case 0:
                    StatusBarText = "No files were added to list.";
                    break;
                case 1:
                    StatusBarText = "1 file added to list.";
                    break;
                default:
                    StatusBarText = successfulAdds.ToString() + " files added to list.";
                    break;
            }
        }

        private int RecursiveFolderSearch(string path, int totalAddedSoFar)
        {
            var totalAdded = totalAddedSoFar;

            foreach (string file in Directory.GetFiles(path))
            {
                if (AddFile(file)) { totalAdded++; };
            }
            foreach (string subDir in Directory.GetDirectories(path))
            {
                try
                {
                    totalAdded += RecursiveFolderSearch(subDir, 0);
                }
                catch
                {
                    // ignore files/folders that can't be accessed
                }
            }
            return totalAdded;
        }

        /// <summary>
        /// Takes file path/name and retrieves certain information about it before adding to ObservableCollection
        /// </summary>
        /// <param name="path"></param>
        private bool AddFile(string path)
        {
            // Check if file is already in list
            if (_selectedFiles.Any(t => path == t.Fullpath ) )
            {
                return false;
            }

            try
            {
                var toAdd = new TouchFiles
                {
                    AccessedOn = File.GetLastAccessTime(path),
                    ModifiedOn = File.GetLastWriteTime(path),
                    CreatedOn = File.GetCreationTime(path),
                    Filename = Path.GetFileName(path),
                    Directory = Path.GetDirectoryName(path),
                    Extension = Path.GetExtension(path),
                    Fullpath = path
                };

                _selectedFiles.Add(toAdd);
                return true;
            }
            catch (Exception errorException)
            {
                var fileError = "Error adding file " + path + "\n" + errorException.ToString().Split('\n')[0];
                Xceed.Wpf.Toolkit.MessageBox.Show(fileError, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

        }

        /// <summary>
        /// Reads which files are selected in the UI DataGrid and removes them
        /// </summary>
        public void RemoveSelected()
        {
            if (SelectedRows.Count == 0)
            {
                StatusBarText = "No items selected to remove.";
                return;
            }

            var totalToRemove = 0;

            while (SelectedRows.Count > 0)
            {
                _selectedFiles.Remove((TouchFiles)SelectedRows[0]);
                totalToRemove++;
            }

            SelectedRows.Clear();
            StatusBarText = totalToRemove.ToString() + " files removed from list.";

        }

        /// <summary>
        /// Removes all TouchFiles currently selected
        /// </summary>
        public void RemoveAll()
        {
            var totalCleared = _selectedFiles.Count;
            if (totalCleared == 0)
            {
                StatusBarText = "No files in list to remove.";
                return;
            }

            _selectedFiles.Clear();
            StatusBarText = totalCleared.ToString() + " files removed from list.";
        }

        /// <summary>
        /// Loops through list of files and changes the 3 date stamps
        /// </summary>
        public void TouchFiles()
        {

            // Check that an attribute to edit has actually been selected
            if (!AccessedCheck && !ModifiedCheck && !CreatedCheck)
            {
                StatusBarText = "Please select an attribute to touch.";
                return;
            }

            var erroredFiles = "";
            var anyErrors = false;

            var touches = 0;

            #region Setting Times
            var current = DateTime.Now;

            var touchAccessedTime = current;
            var touchModifiedTime = current;
            var touchCreatedTime = current;

            // Check if a custom date time has been specified for accessed
            if (_attributeDateTimeBools[0])
            {
                try
                {
                    touchAccessedTime = DateTime.Parse(_attributeDateTimes[0]);
                }
                catch
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("Unable to read specified Accessed On date and time, make sure it is typed correctly", "Error Parsing Date", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            // Check if a custom date time has been specified for modified
            if (_attributeDateTimeBools[1])
            {
                try
                {
                    touchModifiedTime = DateTime.Parse(_attributeDateTimes[1]);
                }
                catch
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("Unable to read specified Modified On date and time, make sure it is typed correctly", "Error Parsing Date", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            // Check if a custom date time has been specified for created
            if (_attributeDateTimeBools[2])
            {
                try
                {
                    touchCreatedTime = DateTime.Parse(_attributeDateTimes[2]);
                }
                catch
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("Unable to read specified Created On date and time, make sure it is typed correctly", "Error Parsing Date", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            #endregion Setting Times


            foreach (TouchFiles files in _selectedFiles)
            {

                // Check to see if file already has been deemed FileNotFound etc, if so don't touch it
                if (files.Error) { continue; }

                var anyTouches = false;

                if (AccessedCheck)
                {
                    try
                    {
                        File.SetLastAccessTime(files.Fullpath, touchAccessedTime);
                        anyTouches = true;
                    }
                    catch (Exception errorException)
                    {

                        erroredFiles += "Error: " + files.Fullpath + " - " + errorException.ToString().Split('\n')[0] + "\n";
                        anyErrors = true;
                    }
                }

                if (ModifiedCheck && !anyErrors)
                {
                    try
                    {
                        File.SetLastWriteTime(files.Fullpath, touchModifiedTime);
                        anyTouches = true;
                    }
                    catch (Exception errorException)
                    {
                        erroredFiles += "Error: " + files.Fullpath + " - " + errorException.ToString().Split('\n')[0] + "\n";
                        anyErrors = true;
                    }
                }

                if (CreatedCheck && !anyErrors)
                {
                    try
                    {
                        File.SetCreationTime(files.Fullpath, touchCreatedTime);
                        anyTouches = true;
                    }
                    catch (Exception errorException)
                    {
                        erroredFiles += "Error: " + files.Fullpath + " - " + errorException.ToString().Split('\n')[0] + "\n";
                        anyErrors = true;
                    }
                }

                if (anyTouches)
                {
                    touches++;
                }
            }

            StatusBarText = touches.ToString() + " files touched.";

            if (anyErrors)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(erroredFiles, "Touch Errors", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            RefreshFileDatestamps();

        }

        /// <summary>
        /// Loops through file list refreshing all 3 datestamps, called after files have been touched to update DataGrid
        /// </summary>
        private void RefreshFileDatestamps()
        {
            for (var i = 0; i < _selectedFiles.Count; i++)
            {
                TouchFiles tempFile = _selectedFiles[i];
                tempFile.AccessedOn = File.GetLastAccessTime(tempFile.Fullpath);
                tempFile.ModifiedOn = File.GetLastWriteTime(tempFile.Fullpath);
                tempFile.CreatedOn = File.GetCreationTime(tempFile.Fullpath);

                _selectedFiles[i] = tempFile;
            }

        }

        /// <summary>
        /// Closes the program. Currently only linked to File>Exit
        /// </summary>
        public void Exit()
        {
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Shows the "About FileToucher" window. Currently only linked to Help>About
        /// </summary>
        public void ShowAbout()
        {    
            Xceed.Wpf.Toolkit.MessageBox.Show("Tile Toucher 0.2 created by Duncan Tait.\nGithub repository: https://github.com/dunctait/file-toucher", "About file-toucher", MessageBoxButton.OK, MessageBoxImage.Information);
        }

    }
}
