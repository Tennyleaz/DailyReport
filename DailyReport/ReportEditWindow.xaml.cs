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
        private readonly string serverUrl;

        public ReportEditWindow(PeriodReport pr, string serverUrl)
        {
            InitializeComponent();
            this.PeriodReport = pr;
            this.serverUrl = serverUrl;
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

        private async void BtnEditName_OnClick(object sender, RoutedEventArgs e)
        {
            if (btnEditCancel.Visibility != Visibility.Visible)
            {
                // Show edit UI
                tbProjectNameEdit.Text = (string)lbProjectName.Content;
                btnEditName.Content = "OK";
                lbProjectName.Visibility = Visibility.Collapsed;
                tbProjectNameEdit.Visibility = Visibility.Visible;
                btnEditCancel.Visibility = Visibility.Visible;
            }
            else
            {
                // Update db
                bool updateResult;
                ProjectReport pr = new ProjectReport();
                pr.ProjectName = tbProjectNameEdit.Text;
                pr.Version = PeriodReport.Version;
                pr.Id = PeriodReport.ProjectID;
                // update server first
                using (WebDBManager webm = new WebDBManager(serverUrl))
                {
                    updateResult = await webm.Update(pr, "Project", pr.Id);
                }
                if (!updateResult)
                {
                    MessageBox.Show("Update to server failed!");
                    return;
                }
                // update local
                using (DatabaseManager dbm = new DatabaseManager())
                {
                    await dbm.Init();
                    if (dbm.Open())
                    {
                        updateResult = await dbm.UpdateAsync(pr);
                    }
                    else
                    {
                        MessageBox.Show("Database open failed!");
                        return;
                    }
                }
                if (!updateResult)
                {
                    MessageBox.Show("Update to database failed!");
                    return;
                }
                // Update project name
                lbProjectName.Content = PeriodReport.ProjectName = tbProjectNameEdit.Text;
                // Hide edit UI
                BtnEditCancel_OnClick(null, null);
            }
        }

        private void BtnEditCancel_OnClick(object sender, RoutedEventArgs e)
        {
            lbProjectName.Visibility = Visibility.Visible;
            tbProjectNameEdit.Visibility = Visibility.Collapsed;
            btnEditCancel.Visibility = Visibility.Collapsed;
            btnEditName.Content = "Edit";
        }
    }
}
