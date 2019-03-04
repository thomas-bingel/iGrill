using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace IGrill.App.Areas
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ProbeList Probes { get; set; } = new ProbeList();

        private string name;
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                this.name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name"));
            }
        }

        private string firmwareVersion;
        public string FirmwareVersion
        {
            get
            {
                return firmwareVersion;
            }
            set
            {
                this.firmwareVersion = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FirmwareVersion"));
            }
        }

        private int batteryLevel;
        public int BatteryLevel
        {
            get
            {
                return batteryLevel;
            }
            set
            {
                this.batteryLevel = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("BatteryLevel"));
            }
        }

    }

    public class ProbeList : ObservableCollection<ProbeViewModel>
    {
    }

    public class ProbeViewModel :INotifyPropertyChanged
    {

        public ProbeViewModel(int index)
        {
            this.Index = index;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private int? value;
        public int? Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Value"));
            }
        }

        private int Index { get; set; }

        private string name;
        public string Name
        {
            get {
                if (name == null)
                {
                    var resourceLoader = new ResourceLoader();
                    name = String.Format("{1}. {0} ", resourceLoader.GetString("ProbeTitle/Text"), this.Index + 1);
                }
                return name;
            }
            set
            {
                this.name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name"));
            }
        }



    }
}
