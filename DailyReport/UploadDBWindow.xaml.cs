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
    /// UploadDBWindow.xaml 的互動邏輯
    /// </summary>
    public partial class UploadDBWindow : Window
    {
        private readonly string _serverUrl;

        public UploadDBWindow(string serverURL)
        {
            _serverUrl = serverURL;
            InitializeComponent();
            ContentRendered += UploadDBWindow_ContentRendered;
        }

        private async void UploadDBWindow_ContentRendered(object sender, EventArgs e)
        {
            List<ProjectReport> projects;
            List<Mantis> mantises;
            List<DailyReportModel> reports;
            using (DatabaseManager dbManager = new DatabaseManager())
            {
                await dbManager.Init();
                if (dbManager.Open())
                {
                    // get db project
                    projects = await dbManager.ReadAllProjectsAsync();
                    // get db mantis
                    mantises = await dbManager.ReadAllMantisAsync();
                    // get db report
                    reports = await dbManager.ReadAllReportsAsync();

                    progressBar.IsIndeterminate = false;
                    progressBar.Minimum = 0;
                    progressBar.Maximum = projects.Count + mantises.Count + reports.Count;

                    using (WebDBManager webManager = new WebDBManager(_serverUrl))
                    {
                        try
                        {
                            // upload project
                            foreach (ProjectReport p in projects)
                            {
                                await webManager.TryAddProjectAsync(p);
                                progressBar.Value++;
                            }
                            // upload mantis
                            foreach (Mantis m in mantises)
                            {
                                await webManager.AddNew(m, "Mantis");
                                progressBar.Value++;
                            }
                            // upload report
                            foreach (DailyReportModel d in reports)
                            {
                                await webManager.AddNew(d, "DailyReport");
                                progressBar.Value++;
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }
                }
            }
            Close();
        }
    }
}
