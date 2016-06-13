using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using FileToucher.ViewModel;
using System.IO;
using System.Threading;

namespace FileToucherTests
{
    [TestFixture]
    public class FileToucherViewModelTests
    {
        public string GetAbsoluteFilePath(string file)
        {
            var cd = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var absoluteFile = Path.Combine(cd, "TestFiles", file);
            return absoluteFile;
        }

        public string GetAbsoluteDirectoryPath(string subdirectoriesString)
        {
            var cd = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var absoluteDirectory = cd + subdirectoriesString;
            return absoluteDirectory;
        }

        private FileToucherViewModel MakeViewModel()
        {
            return new FileToucherViewModel();
        }

        [Test]
        public void AddFiles_FakePaths_DoesntAdd()
        {
            var vm = MakeViewModel();
            var files = new string[] {"X", "X:", "X:/ ", "X:/fwfw/", "C:/e/file.test", "C:/E", "", "C:/notreal.test"};

            vm.AddFiles(files);

            Assert.That(vm.SelectedTouchFiles.Count(), Is.EqualTo(0));
        }

        [Test]
        public void AddFiles_OneFile_Adds()
        {
            var vm = MakeViewModel();
            var files = new string[] { GetAbsoluteFilePath("file1.txt") };

            vm.AddFiles(files);
            var fileList = vm.GetFileList();

            Assert.That(fileList.Count, Is.EqualTo(1));
            Assert.That(fileList[0].Filename, Is.EqualTo("file1.txt"));
            Assert.That(fileList[0].Extension, Is.EqualTo(".txt"));
        }

        [Test]
        public void AddDirectory_TestFilesDir_Adds()
        {
            var vm = MakeViewModel();
            vm.AddDirectory(GetAbsoluteDirectoryPath(@"\TestFiles\TestDirectory\"));

            while (vm.ThreadRunning) { Thread.Sleep(10); }
            var fileList = vm.GetFileList();          
            Assert.That(fileList.Count, Is.EqualTo(3));
        }

        [Test]
        public void AddDirectory_OneFilePath_DoesntAdd()
        {
            var vm = MakeViewModel();
            vm.AddDirectory(GetAbsoluteFilePath("file1.txt"));

            while (vm.ThreadRunning) { Thread.Sleep(10); }
            var fileList = vm.GetFileList();
            Assert.That(fileList.Count, Is.EqualTo(0));
        }

        [Test]
        public void RemoveSelected_OneFile_Removes()
        {
            var vm = MakeViewModel();
            var files = new string[] { GetAbsoluteFilePath("file1.txt") };

            vm.AddFiles(files);
            var fileList = vm.GetFileList();

            Assert.That(fileList.Count, Is.EqualTo(1));

            // now we know there is one file in the list

            ArrayList selectedRowsList = new ArrayList();
            selectedRowsList.Add(vm.SelectedTouchFiles[0]);
            vm.SelectedRows = selectedRowsList;

            vm.RemoveSelectedClicked.Execute(null);

            fileList = vm.GetFileList();
            Assert.That(fileList.Count, Is.EqualTo(0));

        }

        public void RemoveSelected_OneFileNoneSelected_DoesntRemove()
        {
            var vm = MakeViewModel();
            var files = new string[] { GetAbsoluteFilePath("file1.txt") };

            vm.AddFiles(files);
            var fileList = vm.GetFileList();

            Assert.That(fileList.Count, Is.EqualTo(1));

            // now we know there is one file in the list

            vm.RemoveSelectedClicked.Execute(null);

            fileList = vm.GetFileList();
            Assert.That(fileList.Count, Is.EqualTo(1));
        }

        [Test]
        public void RemoveSelected_TwoFilesOneSelected_Removes()
        {
            var vm = MakeViewModel();
            var files = new string[] { GetAbsoluteFilePath("file1.txt"), GetAbsoluteFilePath("file2.txt"), };

            vm.AddFiles(files);
            var fileList = vm.GetFileList();

            Assert.That(fileList.Count, Is.EqualTo(2));

            // now we know there is one file in the list

            ArrayList selectedRowsList = new ArrayList();
            selectedRowsList.Add(vm.SelectedTouchFiles[0]);
            vm.SelectedRows = selectedRowsList;

            vm.RemoveSelectedClicked.Execute(null);

            fileList = vm.GetFileList();
            Assert.That(fileList.Count, Is.EqualTo(1));
        }

        [Test]
        public void RemoveAll_NoFiles_NoCrash()
        {
            var vm = MakeViewModel();

            vm.RemoveAllClicked.Execute(null);

            var fileList = vm.GetFileList();
            Assert.That(fileList.Count, Is.EqualTo(0));
        }

        [Test]
        public void RemoveAll_OneFile_Removes()
        {
            var vm = MakeViewModel();
            var files = new string[] { GetAbsoluteFilePath("file1.txt") };

            vm.AddFiles(files);
            var fileList = vm.GetFileList();

            Assert.That(fileList.Count, Is.EqualTo(1));

            // now we know there is one file in the list

            vm.RemoveAllClicked.Execute(null);

            fileList = vm.GetFileList();
            Assert.That(fileList.Count, Is.EqualTo(0));
        }

        [Test]
        public void RemoveAll_TwoFiles_Removes()
        {
            var vm = MakeViewModel();
            var files = new string[] { GetAbsoluteFilePath("file1.txt"), GetAbsoluteFilePath("file2.txt") };

            vm.AddFiles(files);
            var fileList = vm.GetFileList();

            Assert.That(fileList.Count, Is.EqualTo(2));

            // now we know there is one file in the list

            vm.RemoveAllClicked.Execute(null);

            fileList = vm.GetFileList();
            Assert.That(fileList.Count, Is.EqualTo(0));
        }

        [Test]
        public void TouchFiles_TwoFilesSpecificDate_Sets()
        {
            var vm = MakeViewModel();
            var files = new string[] { GetAbsoluteFilePath("file1.txt"), GetAbsoluteFilePath("file2.txt") };
            vm.AddFiles(files);
            vm.AccessedCheck = true;
            vm.ModifiedCheck = true;
            vm.CreatedCheck = true;
            vm.AccessedNowCheck = false;
            vm.ModifiedNowCheck = false;
            vm.CreatedNowCheck = false;
            var timeToSet = "01 June 2016 04:04:04";
            var timeToSetParsed = DateTime.Parse(timeToSet);
            vm.AccessedDateTime = timeToSet;
            vm.ModifiedDateTime = timeToSet;
            vm.CreatedDateTime = timeToSet;

            vm.TouchFilesClicked.Execute(null);

            var fileList = vm.GetFileList();
            foreach (FileToucher.Model.TouchFiles t in fileList)
            {
                Assert.That(t.AccessedOn, Is.EqualTo(timeToSetParsed));
                Assert.That(t.ModifiedOn, Is.EqualTo(timeToSetParsed));
                Assert.That(t.CreatedOn, Is.EqualTo(timeToSetParsed));
            }

        }

        [Test]
        public void TouchFiles_TwoFilesNow_Sets()
        {
            var vm = MakeViewModel();
            var files = new string[] { GetAbsoluteFilePath("file1.txt"), GetAbsoluteFilePath("file2.txt") };
            vm.AddFiles(files);
            vm.AccessedCheck = true;
            vm.ModifiedCheck = true;
            vm.CreatedCheck = true;
            var timePreTouch = DateTime.Now;

            vm.TouchFilesClicked.Execute(null);

            var timePostTouch = DateTime.Now;
            var fileList = vm.GetFileList();
            foreach (FileToucher.Model.TouchFiles t in fileList)
            {
                Assert.That(t.AccessedOn, Is.GreaterThan(timePreTouch));
                Assert.That(t.AccessedOn, Is.LessThan(timePostTouch));
                Assert.That(t.ModifiedOn, Is.GreaterThan(timePreTouch));
                Assert.That(t.ModifiedOn, Is.LessThan(timePostTouch));
                Assert.That(t.CreatedOn, Is.GreaterThan(timePreTouch));
                Assert.That(t.CreatedOn, Is.LessThan(timePostTouch));
            }
        }

    }
}