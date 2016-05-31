using System;
using System.ComponentModel;

namespace FileToucher.Model
{
    class TouchFiles : INotifyPropertyChanged
    {

        private DateTime _accessedOn;
        private DateTime _modifiedOn;
        private DateTime _createdOn;

        public string Filename { get; set; }
        public string Directory { get; set; }
        public string Fullpath { get; set; }
        public string Extension { get; set; }

        public DateTime AccessedOn
        {
            get { return _accessedOn; }
            set
            {
                _accessedOn = value;
                RaisePropertyChangedEvent("AccessedOn");
            }
        }
        public DateTime ModifiedOn
        {
            get { return _modifiedOn; }
            set
            {
                _modifiedOn = value;
                RaisePropertyChangedEvent("ModifiedOn");
            }
        }
        public DateTime CreatedOn
        {
            get { return _createdOn; }
            set
            {
                _createdOn = value;
                RaisePropertyChangedEvent("CreatedOn");

                if (_createdOn.Year == 1601)
                {
                    RaisePropertyChangedEvent("Error");
                }
            }
        }

        /// <summary>
        /// Returns true if the CreatedOn date is 1601, since that implies FileNotFound
        /// </summary>
        public bool Error
        {
            get
            {
                return (AccessedOn.Year == 1601);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChangedEvent(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
