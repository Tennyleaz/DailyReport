using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DailyReport
{
    /// <summary>
    /// ReportControl.xaml 的互動邏輯
    /// </summary>
    public partial class ReportControl : UserControl
    {
        public event EventHandler OnRemoveProject;
        private Report _report;

        public ReportControl(Report report)
        {
            InitializeComponent();
            Loaded += ReportControl_Loaded;

            _report = report;
            _report.PropertyChanged += Report_PropertyChanged;
        }

        private void Report_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ProjectName")
                tbProjectName.Text = _report.ProjectName;
            if (e.PropertyName == "ProjectVersion")
                tbProjectVersion.Text = _report.ProjectVersion;
        }

        private void ReportControl_Loaded(object sender, RoutedEventArgs e)
        {
            UserList.ItemsSource = _report.DailyProgresses;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            _report.DailyProgresses.Add(new MyProgress());
        }

        private void tbProjectName_TextChanged(object sender, TextChangedEventArgs e)
        {
            _report.ProjectName = tbProjectName.Text;
        }

        private void tbProjectVersion_TextChanged(object sender, TextChangedEventArgs e)
        {
            _report.ProjectVersion = tbProjectVersion.Text;
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            object dc = btn?.DataContext;
            if (dc != null)
            {
                MyProgress mp = dc as MyProgress;
                if (mp != null)
                {
                    _report.DailyProgresses.Remove(mp);
                }
            }
        }

        private void btnRemoveProj_Click(object sender, RoutedEventArgs e)
        {
            OnRemoveProject?.Invoke(this, null);
        }
    }
}
