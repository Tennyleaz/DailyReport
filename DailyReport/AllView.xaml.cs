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
using System.Windows.Shapes;

namespace DailyReport
{
    /// <summary>
    /// AllView.xaml 的互動邏輯
    /// </summary>
    public partial class AllView : Window
    {
        private ObservableCollection<PeriodReport> reportList;
        private readonly string serverUrl;

        /// <summary>
        /// Show all period report.
        /// </summary>
        /// <param name="serverUrl">If serverUrl is null, only read local DB.</param>
        /// <param name="useLocal">Only read local DB.</param>
        public AllView(string serverUrl, bool useLocal)
        {
            InitializeComponent();
            reportList = new ObservableCollection<PeriodReport>();
            reportListView.ItemsSource = reportList;
            this.serverUrl = serverUrl;
            if (useLocal)
                Loaded += AllView_Loaded;
            else
                Loaded += AllView_Loaded2;
        }

        private async void AllView_Loaded2(object sender, RoutedEventArgs e)
        {
            using (WebDBManager webm = new WebDBManager(serverUrl))
            {
                try
                {
                    var prList = await webm.ReadAllPeriodReportAsync();
                    if (prList != null)
                    {
                        prList.Sort();
                        foreach (PeriodReport pr in prList)
                        {
                            reportList.Add(pr);
                        }
                    }
                    else
                    {
                        PeriodReport testPR = new PeriodReport()
                        {
                            Date = DateTime.MinValue,
                            ProjectName = "---",
                            Message = "No report"
                        };
                        reportList.Add(testPR);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private async void AllView_Loaded(object sender, RoutedEventArgs e)
        {
            using (DatabaseManager dbm = new DatabaseManager())
            {
                await dbm.Init();
                if (dbm.Open())
                {
                    var prList = await dbm.ReadAllPeriodReportAsync();
                    if (prList != null)
                    {
                        foreach (PeriodReport pr in prList)
                        {
                            reportList.Add(pr);
                        }
                    }
                    else
                    {
                        PeriodReport testPR = new PeriodReport()
                        {
                            Date = DateTime.MinValue,
                            ProjectName = "---",
                            Message = "No report"
                        };
                        reportList.Add(testPR);
                    }
                }
                else
                {
                    MessageBox.Show("database open failed.");
                    Close();
                }
            }
        }

        private void RoundedButton_Click(object sender, RoutedEventArgs e)
        {
            List<PeriodReport> vPr = new List<PeriodReport>();
            foreach (var item in reportListView.SelectedItems)
            {
                PeriodReport pr = (PeriodReport)item;
                vPr.Add(pr);
            }
            vPr.Sort();

            // pipe to notepad
            string strText = "";
            foreach (PeriodReport pr in vPr)
            {
                strText += pr.ToString() + Environment.NewLine;
            }
            NotepadEditor.PipeTextToNotepad(strText);
        }

        private async void ListViewItem_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListViewItem item && item.IsSelected)
            {
                if (item.Content is PeriodReport pr)
                {
                    ReportEditWindow editWindow = new ReportEditWindow(pr);
                    editWindow.Owner = this;
                    bool? result = editWindow.ShowDialog();
                    if (result == true)
                    {
                        bool updateResult;
                        DailyReportModel updated = new DailyReportModel();
                        updated.Message = editWindow.PeriodReport.Message;
                        updated.ProjectId = pr.ProjectID;
                        updated.Id = pr.ReoprtID;
                        updated.Date = pr.Date;
                        // update server first
                        using (WebDBManager webm = new WebDBManager(serverUrl))
                        {
                            updateResult = await webm.Update(updated, "DailyReport", updated.Id);
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
                                updateResult = await dbm.UpdateAsync(updated);
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
                        // update UI last
                        pr.Message = editWindow.PeriodReport.Message;
                    }
                }
            }
        }

        private async void ListViewItem_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Delete)
                return;
            if (sender is ListViewItem item && item.IsSelected)
            {
                if (item.Content is PeriodReport pr)
                {
                    string text = "Report:\n\"" + pr.Message + "\"\n" + "Are you sure to delete?";
                    var msg = MessageBox.Show(this, text, pr.ProjectName, MessageBoxButton.OKCancel, MessageBoxImage.Question);
                    if (msg != MessageBoxResult.OK)
                        return;

                    bool deleteResult;
                    using (WebDBManager webm = new WebDBManager(serverUrl))
                    {
                        deleteResult = await webm.Delete("DailyReport", pr.ReoprtID);
                    }
                    if (!deleteResult)
                    {
                        MessageBox.Show("Delete from server failed!");
                        return;
                    }
                    using (DatabaseManager dbm = new DatabaseManager())
                    {
                        await dbm.Init();
                        if (dbm.Open())
                        {
                            deleteResult = await dbm.DeleteAsync<DailyReportModel>(pr.ReoprtID);
                        }
                        else
                        {
                            MessageBox.Show("Database open failed!");
                            return;
                        }
                    }
                    if (!deleteResult)
                    {
                        MessageBox.Show("Delete from database failed!");
                        return;
                    }
                    // update UI last
                    reportList.Remove(pr);
                }
            }
        }
    }
}
