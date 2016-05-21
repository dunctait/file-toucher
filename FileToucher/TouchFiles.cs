using System;
using System.ComponentModel;

namespace FileToucher
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
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChangedEvent(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
