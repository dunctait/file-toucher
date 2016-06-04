using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileToucher.Model;
using NUnit.Framework;
using FileToucher.ViewModel;
using System.IO;


namespace FileToucherTests
{
    [TestFixture]
    public class FileToucherViewModelTests
    {
        public string GetAbsolutePath(string file)
        {
            var cd = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var absoluteFile = cd + @"\TestFiles\" + file;
            return absoluteFile;
        }

        [Test]
        public void AddFiles_FakePaths_DoesntAdd()
        {
            var vm = new FileToucherViewModel();

            var files = new string[] {"X", "X:", "X:/ ", "X:/fwfw/", "C:/e/file.test", "C:/E", "", "C:/notreal.test"};

            vm.AddFiles(files);

            Assert.That(vm.SelectedTouchFiles.Count(), Is.EqualTo(0));
        }

        [Test]
        public void AddFiles_OneFile_Adds()
        {
            var vm = new FileToucherViewModel();
            
            var files = new string[] { GetAbsolutePath("file1.txt") };

            vm.AddFiles(files);
            var fileList = vm.GetFileList();

            Assert.That(fileList.Count, Is.EqualTo(1));
            Assert.That(fileList[0].Filename, Is.EqualTo("file1.txt"));
            Assert.That(fileList[0].Extension, Is.EqualTo(".txt"));
        }
    }
}