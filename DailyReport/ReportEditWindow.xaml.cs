using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace DailyReport
{
    /// <summary>
    /// ReportEditWindow.xaml 的互動邏輯
    /// </summary>
    public partial class ReportEditWindow : Window
    {
        public PeriodReport PeriodReport { get; }

        public ReportEditWindow(PeriodReport pr)
        {
            InitializeComponent();
            this.PeriodReport = pr;
        }

        private void ReportEditWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (PeriodReport == null)
            {
                MessageBox.Show("Input project report is null!");
                return;
            }

            lbProjectName.Content = PeriodReport.ProjectName;
            lbProjectVersion.Content = PeriodReport.Version;
            lbProjectId.Content = PeriodReport.ProjectID;
            lbReportId.Content = PeriodReport.ReoprtID;
            lbDate.Content = PeriodReport.DisplayDate;
            tbMessage.Text = PeriodReport.Message;
        }

        private void BtnSave_OnClick(object sender, RoutedEventArgs e)
        {
            PeriodReport.Message = tbMessage.Text;
            DialogResult = true;
            Close();
        }
    }
}
