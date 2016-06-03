using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using System.Linq;
using System.IO;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using FileToucher.Model;

namespace FileToucher.ViewModel
{
    class FileToucherViewModel : ViewModelBase
    {
        #region Variables

        private readonly ObservableCollection<TouchFiles> _selectedFiles = new ObservableCollection<TouchFiles>();

        // Create list of items that are selected in DataGrid

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

        // Create boolean to control showing generic dialog
        private bool _dialogVisible;

        // Create string to hold dialog message
        private string _dialogText = "";

        // Create string to hold dialog title
        private string _dialogTitle = "";

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
                RaisePropertyChanged();
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
                RaisePropertyChanged();
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
                RaisePropertyChanged();
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
                RaisePropertyChanged();

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
                RaisePropertyChanged();

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
                RaisePropertyChanged();

                // if unchecking/checking "Now" but still touching Created attribute: enable/disable DatePicker respectively
                if (CreatedCheck) { CreatedDateTimeEnable = !value ;}
            }
        }

        // The following 3 properties are "Current Time" checkbox enabling and disabling functionality
        public bool AccessedNowCheckboxEnable
        {
            get { return _attributesNowEnabledBools[0]; }
            set { _attributesNowEnabledBools[0] = value; RaisePropertyChanged(); }
        }

        public bool ModifiedNowCheckboxEnable
        {
            get { return _attributesNowEnabledBools[1]; }
            set { _attributesNowEnabledBools[1] = value; RaisePropertyChanged(); }
        }

        public bool CreatedNowCheckboxEnable
        {
            get { return _attributesNowEnabledBools[2]; }
            set { _attributesNowEnabledBools[2] = value; RaisePropertyChanged(); }
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
                RaisePropertyChanged();
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
                RaisePropertyChanged();

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
                RaisePropertyChanged();
                
            }
        }

        // The following 3 properties control the setting and getting of the string showing the DateTime selected
        public string AccessedDateTime
        {
            get { return _attributeDateTimes[0]; }
            set { _attributeDateTimes[0] = value; RaisePropertyChanged(); }
        }

        public string ModifiedDateTime
        {
            get { return _attributeDateTimes[1]; }
            set { _attributeDateTimes[1] = value; RaisePropertyChanged(); }
        }

        public string CreatedDateTime
        {
            get { return _attributeDateTimes[2]; }
            set { _attributeDateTimes[2] = value; RaisePropertyChanged(); }
        }

        // Create property that returns the _selectedFiles collection to allow binding to DataGrid
        public ObservableCollection<TouchFiles> SelectedTouchFiles => _selectedFiles;

        // Create property that returns or sets the _selectedRows list to allow us to remove them
        public IList SelectedRows { get; set; } = new ArrayList();

        // Create property that returns or sets the _statusBarText string
        public string StatusBarText
        {
            get { return _statusBarText; }
            set { _statusBarText = value; RaisePropertyChanged(); }
        }

        // Create property that signifies when dialog should be visible
        public bool DialogVisible
        {
            get { return _dialogVisible; }
            set
            {
                if (_dialogVisible == value) return;
                _dialogVisible = value;
                RaisePropertyChanged();
            }
        }

        // Create property that holds the dialog text
        public string DialogText
        {
            get { return _dialogText; }
            set
            {
                if (_dialogText == value) return;
                _dialogText = value;
                RaisePropertyChanged();
            }
        }

        // Create property that holds the dialog title
        public string DialogTitle
        {
            get { return _dialogTitle; }
            set
            {
                if (_dialogTitle == value) return;
                _dialogTitle = value;
                RaisePropertyChanged();
            }
        }

        // Property to set boolean flag to stop thread operations
        public bool StopThread { get; set; }

        // The following ICommands are for binding button clicks to methods
        public ICommand RemoveSelectedClicked => new RelayCommand(RemoveSelected);
        public ICommand RemoveAllClicked => new RelayCommand(RemoveAll);

        public ICommand TouchFilesClicked => new RelayCommand(TouchFiles);

        public ICommand ExitClicked => new RelayCommand(Exit);
        public ICommand AboutClicked => new RelayCommand(ShowAbout);


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

            // Set the checkboxes we want ticked to true at startup
            AccessedCheck = false;
            ModifiedCheck = true;
            CreatedCheck = false;

            StatusBarText = "Program started.";

        }

        /// <summary>
        /// Receives filenames from UI and passes each file to AddFile() method
        /// </summary>
        public void AddFiles(string[] filenameStrings)
        {

            var successfulAdds = 0;

            foreach (var filename in filenameStrings)
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
                    StatusBarText = successfulAdds + " files added to list.";
                    break;
            }

        }
        
        /// <summary>
        /// Receives directory to search from UI, creates background worker to do so
        /// </summary>
        public void AddDirectory(string directory)
        {
            
            var worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += (obj, e) => AddDirectoryWorker(directory);
            worker.RunWorkerAsync(10000);
            
        }

        /// <summary>
        /// Contains directory adding logic in a thread to stop UI locking
        /// </summary>
        /// <param name="directory"></param>
        public void AddDirectoryWorker(string directory)
        {
            ShowThreadDialog("Adding Files", "Adding files now...");

            var successfulAdds = RecursiveFolderSearch(directory, 0);

            switch (successfulAdds)
            {
                case 0:
                    StatusBarText = "No files were added to list.";
                    break;
                case 1:
                    StatusBarText = "1 file added to list.";
                    break;
                default:
                    StatusBarText = successfulAdds + " files added to list.";
                    break;
            }

            Application.Current.Dispatcher.BeginInvoke((Action)(() => { WorkCompletedCommandExecute(); }));
            StopThread = false;

        }

        /// <summary>
        /// Searches recursively through directories adding all files to list, stops if requested from UI
        /// </summary>
        /// <param name="path"></param>
        /// <param name="totalAddedSoFar"></param>
        /// <returns></returns>
        private int RecursiveFolderSearch(string path, int totalAddedSoFar)
        {
            var totalAdded = totalAddedSoFar;

            foreach (string file in Directory.GetFiles(path))
            {
                if (StopThread) { return 0; }
                DialogText = "Adding " + file;
                if (AddFile(file)) { totalAdded++; }
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
            if (_selectedFiles.Any(t => path == t.Fullpath))
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

                Application.Current.Dispatcher.Invoke((Action) (() =>
                {
                    _selectedFiles.Add(toAdd);
                }));

                return true;
            }
            catch (Exception errorException)
            {
                var fileError = "Error adding file " + path + "\n" + errorException.ToString().Split('\n')[0];


                ShowThreadDialog("Error", fileError);

                return false;
            }

        }

        /// <summary>
        /// Reads which files are selected in the UI DataGrid and removes them
        /// </summary>
        private void RemoveSelected()
        {
            if (SelectedRows.Count == 0)
            {
                ShowDialog("Error", "There are no items selected to remove.");
                return;
            }

            var totalToRemove = 0;

            while (SelectedRows.Count > 0)
            {
                _selectedFiles.Remove((TouchFiles)SelectedRows[0]);
                totalToRemove++;
            }

            SelectedRows.Clear();
            StatusBarText = totalToRemove + " files removed from list.";

        }

        /// <summary>
        /// Removes all TouchFiles currently selected
        /// </summary>
        private void RemoveAll()
        {
            var totalCleared = _selectedFiles.Count;
            if (totalCleared == 0)
            {
                ShowDialog("Error", "No files in list to remove.");

                return;
            }

            _selectedFiles.Clear();
            StatusBarText = totalCleared + " files removed from list.";
        }

        /// <summary>
        /// Loops through list of files and changes the 3 date stamps
        /// </summary>
        private void TouchFiles()
        {

            // Check that an attribute to edit has actually been selected
            if (!AccessedCheck && !ModifiedCheck && !CreatedCheck)
            {
                ShowDialog("Error","Please select an attribute to touch first.");
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
                    ShowDialog("Error", "Unable to read specified Accessed On date and time, make sure it is typed correctly");
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
                    ShowDialog("Error", "Unable to read specified Modified On date and time, make sure it is typed correctly");
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
                    ShowDialog("Error", "Unable to read specified Created On date and time, make sure it is typed correctly");
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

            StatusBarText = touches + " files touched.";

            if (anyErrors)
            {
                ShowDialog("Error Touching Some Files", erroredFiles);
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
        /// Open a saved file list and load all files
        /// </summary>
        public void OpenFileList(string[] fileList)
        {
            
            if (fileList.Any()) { _selectedFiles.Clear(); }

            var successfulAdds = 0;

            foreach (var fileToBeAdded in fileList.Where(fileToBeAdded => fileToBeAdded != ""))
            {
                if (AddFile(fileToBeAdded))
                {
                    successfulAdds++;
                }
            }

            switch (successfulAdds)
            {
                case 0:
                    StatusBarText = "No files were loaded.";
                    break;
                case 1:
                    StatusBarText = "1 file loaded.";
                    break;
                default:
                    StatusBarText = successfulAdds + " files loaded.";
                    break;
            }

        }

        /// <summary>
        /// Save list of files to file
        /// </summary>
        public void SaveFileList(string saveLocation)
        {

            string allFilesString = "";

            foreach (TouchFiles file in _selectedFiles)
            {
                allFilesString += file.Fullpath + Environment.NewLine;
            }

            File.WriteAllText(saveLocation, allFilesString);

            StatusBarText = "File list saved.";

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
            ShowDialog("About", "File Toucher 0.3 created by Duncan Tait.\nGithub repository: https://github.com/dunctait/file-toucher");
        }

        /// <summary>
        /// Show normal, non-thread, modal Dialog to user (with Okay button)
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        public void ShowDialog(string title, string message)
        {
            DialogTitle = title;
            DialogText = message;

            // Send message to any connected view requesting dialog is shown
            Messenger.Default.Send(new NotificationMessage("ShowDialog"));
        }

        /// <summary>
        /// Show progress modal dialog to user, requested from thread operations
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        public void ShowThreadDialog(string title, string message)
        {
            DialogTitle = title;
            DialogText = message;

            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                Messenger.Default.Send(new NotificationMessage("ShowProgressDialog"));
            }));
        }

        /// <summary>
        /// Inform the View that threaded work is completed (so progress dialog is closed)
        /// </summary>
        public void WorkCompletedCommandExecute()
        {
            Messenger.Default.Send(new NotificationMessage("WorkCompleted"));
        }
    }
}
