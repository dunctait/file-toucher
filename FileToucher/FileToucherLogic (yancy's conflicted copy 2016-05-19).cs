using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileToucher
{
    internal class FileToucherLogic
    {
        private ObservableCollection<TouchFiles> selectedFiles = new ObservableCollection<TouchFiles>();

        public FileToucherLogic()
        {
            CreateTicker();
        }

        /// <summary>
        /// Creates a ticker and names a method to call every tik
        /// </summary>
        private void CreateTicker()
        {
            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += DispatcherTimerTick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }

        /// <summary>
        /// Logic to carry out every tick of timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DispatcherTimerTick(object sender, EventArgs e)
        {
            // Set a DateTime value to now
        }

        public void AddFiles(string[] dialogFiles)
        {
            foreach (string filename in dialogFiles)
            {
                AddFiles(filename);
            }
        }

        public void AddFiles(string filename)
        {
            // add file to observablecollection
        }

    }
}
