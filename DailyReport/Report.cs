using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DailyReport
{
    public class Report : INotifyPropertyChanged
    {
        private string _projectName;
        private string _projectVersion;
        public ObservableCollection<MyProgress> DailyProgresses;
        public HashSet<string> MantisList = new HashSet<string>();

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string ProjectName
        {
            get { return _projectName; }
            set
            {
                if (_projectName != value)
                {
                    _projectName = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string ProjectVersion
        {
            get { return _projectVersion; }
            set
            {
                if (_projectVersion != value)
                {
                    _projectVersion = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public Report()
        {
            DailyProgresses = new ObservableCollection<MyProgress>();
            ProjectName = string.Empty;
        }

        public override string ToString()
        {
            if (DailyProgresses.Count > 0)
            {
                string result = "今天" + _projectName;
                if (!string.IsNullOrWhiteSpace(_projectVersion))
                    result += " " + _projectVersion;
                result += "進度：\n";
                int count = 1;
                foreach (MyProgress mp in DailyProgresses)
                {
                    if (!string.IsNullOrWhiteSpace(mp.MyValue))
                    {
                        mp.MyValue = mp.MyValue.TrimStart(' ');
                        result += count + ". " + mp.MyValue + Environment.NewLine;
                        count++;
                    }
                }
                return result;
            }
            else
                return string.Empty;
        }
    }

    public class MyProgress
    {
        public string MyValue { get; set; }
    }
}
