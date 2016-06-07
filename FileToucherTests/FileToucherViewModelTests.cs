using System.Linq;
using NUnit.Framework;
using FileToucher.ViewModel;
using System.IO;
using System.Threading;
using FileToucher.View;

namespace FileToucherTests
{
    [TestFixture]
    public class FileToucherViewModelTests
    {
        private FileToucherViewModel _vm;

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

        [SetUp]
        public void SetUp()
        {
            _vm = new FileToucherViewModel();
        }

        [Test]
        public void AddFiles_FakePaths_DoesntAdd()
        {
            var files = new string[] {"X", "X:", "X:/ ", "X:/fwfw/", "C:/e/file.test", "C:/E", "", "C:/notreal.test"};

            _vm.AddFiles(files);

            Assert.That(_vm.SelectedTouchFiles.Count(), Is.EqualTo(0));
        }

        [Test]
        public void AddFiles_OneFile_Adds()
        {
            var files = new string[] { GetAbsoluteFilePath("file1.txt") };

            _vm.AddFiles(files);
            var fileList = _vm.GetFileList();

            Assert.That(fileList.Count, Is.EqualTo(1));
            Assert.That(fileList[0].Filename, Is.EqualTo("file1.txt"));
            Assert.That(fileList[0].Extension, Is.EqualTo(".txt"));
        }

        [Test]
        public void AddDirectory_TestFilesDir_Adds()
        {
            _vm.AddDirectory(GetAbsoluteDirectoryPath(@"\TestFiles\"));

            while (_vm.ThreadRunning) { Thread.Sleep(10); }
            var fileList = _vm.GetFileList();          
            Assert.That(fileList.Count, Is.EqualTo(1));
            Assert.That(fileList[0].Filename, Is.EqualTo("file1.txt"));
            Assert.That(fileList[0].Extension, Is.EqualTo(".txt"));
        }

        [Test]
        public void AddDirectory_OneFilePath_DoesntAdd()
        {
            _vm.AddDirectory(GetAbsoluteFilePath("file1.txt"));
            
            //while (_vm.ThreadRunning) { }
            var fileList = _vm.GetFileList();
            Assert.That(fileList.Count, Is.EqualTo(0));
        }
    }
}