using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using GitMessageParser;
using System.Text.RegularExpressions;
using System.Windows.Threading;

namespace DailyReport
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Report> reports = new List<Report>();
        //private HashSet<string> mantisNumbers = new HashSet<string>();
        private List<CommitReport> wctReports = new List<CommitReport>();
        private List<CommitReport> smReports = new List<CommitReport>();
        private DateTime sinceDate, endDate;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            sinceDatePicker.SelectedDate = DateTime.Today;
            endDatePicker.SelectedDate = DateTime.Today;
            //tbSubject.Text = DateTime.Today.ToString("MM/dd", System.Globalization.CultureInfo.InvariantCulture) + " 進度報告";
            tbAddress.Text = "Julio.Huang@penpower.com.tw";
            tbCC.Text = "Joshua.Zhan@penpower.com.tw";

            //await LoadWCTCommits();
            //await LoadScannerManagerCommits();
            //await TestDB();

            //await LoadCommits("WorldCardTeam", @"C:\Workspace\WorldCardTeam\.git");
            //await LoadCommits("WorldCard Enterprise", @"C:\Workspace\WorldCardEnterprice\.git");
            //await LoadCommits("Scanner Manager", @"C:\Workspace\ScannerManager\.git");
        }

        /*private async Task TestDB()
        {
            DatabaseManager dbm = new DatabaseManager();
            await dbm.Init();
            if (dbm.Open())
            {
                List<GitLog> wctLogs = LoadWCTGitReport("2018-10-18");
                foreach (GitLog log in wctLogs)
                {
                    CommitReport cr = GitMessageParser.GitMessageParser.ParseReport(log);
                    if (cr != null)
                        await WriteCommitReportToDB(dbm, log, cr);
                }
                var list = await dbm.GetViewAsync(DateTime.Today.AddDays(-3), DateTime.Today);
            }
            dbm.Dispose();
        }*/

        public async Task LoadCommits(string projectName, string gitPath, DateTime? sinceDate = null, DateTime? untilDate = null)
        {
            List<CommitReport> cReports = new List<CommitReport>();
            Report report = new Report();
            string projectVersion = "";
            Action readAction = () =>
            {
                // read today's WCT git commit                
                try
                {
                    List<GitLog> wctLogs;
                    if (sinceDate != null && untilDate != null)
                    {                        
                        string sinceDateString = sinceDate.Value.ToString("yyyy-MM-dd HH:mm");
                        string untilDateString = untilDate.Value.ToString("yyyy-MM-dd HH:mm");
                        wctLogs = LoadGitReport(sinceDateString, untilDateString, gitPath);
                    }
                    else if (sinceDate != null)
                    {
                        string sinceDateString = sinceDate.Value.ToString("yyyy-MM-dd HH:mm");
                        wctLogs = LoadGitReport(sinceDateString, gitPath);
                    }
                    else
                    {
                        DateTime yesterday = DateTime.Today.AddDays(-1);
                        string sinceDateString =
                            new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 23, 59, 59)
                            .ToString("yyyy-MM-dd HH:mm");
                        wctLogs = LoadGitReport(sinceDateString, gitPath);
                    }
                    foreach (GitLog log in wctLogs)
                    {
                        CommitReport cr = GitMessageParser.GitMessageParser.ParseReport(log);
                        if (cr != null)
                            cReports.Add(cr);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                reports.Add(report);

                if (cReports.Count > 0)
                {
                    HashSet<string> mantisSet = new HashSet<string>();
                    projectVersion = cReports.First().Version;
                    foreach (CommitReport cr in cReports)
                    {
                        // 移除所有帶mantis的行數
                        if (HasMantisBug(cr.Header))
                        {
                            cr.Body = FindMantisNumbers(cr.Body, ref mantisSet);
                            cr.Header = TrimMantisHeader(cr.Header);
                        }

                        if (cr.Body.Count > 0 || !string.IsNullOrWhiteSpace(cr.Header))
                        {
                            for (int i = 0; i < cr.Body.Count; i++)
                                cr.Body[i] = TrimIndexHeader(cr.Body[i]);
                            MyProgress myProgress = new MyProgress();
                            // 去空白
                            string s = cr.Header + string.Join("", cr.Body);
                            s = s.TrimStart(' ');
                            myProgress.MyValue = s;
                            report.DailyProgresses.Add(myProgress);
                        }
                    }

                    if (mantisSet.Count > 0)
                    {
                        MyProgress myProgress = new MyProgress();
                        string str = "修正 Mantis 上的問題：";
                        str += string.Join("、", mantisSet);
                        myProgress.MyValue = str;
                        report.DailyProgresses.Add(myProgress);
                        report.MantisList = mantisSet;
                        mantisSet.Clear();
                    }
                }
            };

            await Task.Run(readAction);

            if (cReports.Count > 0)
            {
                ReportControl rc = new ReportControl(report);
                rc.OnRemoveProject += OnRemoveProjectClicked;
                reportPanel.Children.Add(rc);
                report.ProjectName = projectName;
                report.ProjectVersion = projectVersion;
            }
        }

        [Obsolete("Use LoadCommits() instead!", true)]
        public async Task LoadScannerManagerCommits()
        {
            Report report = new Report();
            string projectName = "Scanner Manager";
            string projectVersion = "";
            Action readAction = () =>
            {
                // read today's WCT git commit                
                try
                {
                    DateTime yesterday = DateTime.Today.AddDays(-1);
                    string sinceDate =
                        new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 23, 59, 59)
                        .ToString("yyyy-MM-dd HH:mm");
                    List<GitLog> wctLogs = LoadScannerManagerGitReport(sinceDate);
                    foreach (GitLog log in wctLogs)
                    {
                        CommitReport cr = GitMessageParser.GitMessageParser.ParseReport(log);
                        if (cr != null)
                            smReports.Add(cr);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                reports.Add(report);

                if (smReports.Count > 0)
                {
                    HashSet<string> mantisSet = new HashSet<string>();
                    projectVersion = smReports.First().Version;
                    foreach (CommitReport cr in smReports)
                    {
                        // 移除所有帶mantis的行數
                        if (HasMantisBug(cr.Header))
                            cr.Body = FindMantisNumbers(cr.Body, ref mantisSet);

                        if (cr.Body.Count > 0 || !string.IsNullOrWhiteSpace(cr.Header))
                        {
                            for (int i = 0; i < cr.Body.Count; i++)
                                cr.Body[i] = TrimIndexHeader(cr.Body[i]);
                            MyProgress myProgress = new MyProgress();
                            // 去空白
                            string s = cr.Header + string.Join("", cr.Body);
                            s = s.TrimStart(' ');
                            myProgress.MyValue = s;                            
                            report.DailyProgresses.Add(myProgress);
                        }
                    }

                    if (mantisSet.Count > 0)
                    {
                        MyProgress myProgress = new MyProgress();
                        string str = "修正 Mantis 上的問題：";
                        str += string.Join("、", mantisSet);
                        myProgress.MyValue = str;
                        report.DailyProgresses.Add(myProgress);
                        report.MantisList = mantisSet;
                    }
                }
            };

            await Task.Run(readAction);

            if (smReports.Count > 0)
            {
                ReportControl rc = new ReportControl(report);
                rc.OnRemoveProject += OnRemoveProjectClicked;
                reportPanel.Children.Add(rc);
                report.ProjectName = projectName;
                report.ProjectVersion = projectVersion;
            }
        }

        [Obsolete("Use LoadCommits() instead!", true)]
        public async Task LoadWCTCommits()
        {
            Report report = new Report();
            string projectName = "WorldCardTeam";
            string projectVersion = "";
            Action readAction = () =>
            {
                // read today's WCT git commit                
                try
                {
                    DateTime yesterday = DateTime.Today.AddDays(-1);
                    string sinceDate =
                        new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 23, 59, 59)
                        .ToString("yyyy-MM-dd HH:mm");
                    List<GitLog> wctLogs = LoadWCTGitReport(sinceDate);
                    foreach (GitLog log in wctLogs)
                    {
                        CommitReport cr = GitMessageParser.GitMessageParser.ParseReport(log);
                        if (cr != null)
                            wctReports.Add(cr);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                reports.Add(report);

                if (wctReports.Count > 0)
                {
                    HashSet<string> mantisSet = new HashSet<string>();
                    projectVersion = wctReports.First().Version;
                    foreach (CommitReport cr in wctReports)
                    {
                        // 移除所有帶mantis的行數
                        if (HasMantisBug(cr.Header))
                        {
                            cr.Header = string.Empty;
                            cr.Body = FindMantisNumbers(cr.Body, ref mantisSet);
                        }

                        if (cr.Body.Count > 0 || !string.IsNullOrWhiteSpace(cr.Header))
                        {
                            for (int i = 0; i < cr.Body.Count; i++)
                                cr.Body[i] = TrimIndexHeader(cr.Body[i]);
                            MyProgress myProgress = new MyProgress();
                            // 去空白
                            string s = cr.Header + string.Join("", cr.Body);
                            s = s.TrimStart(' ');
                            myProgress.MyValue = s;
                            report.DailyProgresses.Add(myProgress);
                        }
                    }

                    if (mantisSet.Count > 0)
                    {
                        MyProgress myProgress = new MyProgress();
                        string str = "修正 Mantis 上的問題：";
                        str += string.Join("、", mantisSet);
                        myProgress.MyValue = str;
                        report.DailyProgresses.Add(myProgress);
                        report.MantisList = mantisSet;
                    }
                }
            };

            await Task.Run(readAction);

            ReportControl rc = new ReportControl(report);
            rc.OnRemoveProject += OnRemoveProjectClicked;
            reportPanel.Children.Add(rc);
            report.ProjectName = projectName;
            report.ProjectVersion = projectVersion;
        }

        private void OnRemoveProjectClicked(object sender, EventArgs e)
        {
            if (sender is ReportControl)
            {
                ReportControl rc = sender as ReportControl;
                reportPanel.Children.Remove(rc);
            }
        }

        private void btnAdder_Click(object sender, RoutedEventArgs e)
        {
            Report report = new Report();
            reports.Add(report);
            ReportControl rc = new ReportControl(report);
            rc.OnRemoveProject += OnRemoveProjectClicked;
            reportPanel.Children.Add(rc);
        }

        private void btnMail_Click(object sender, RoutedEventArgs e)
        {
            List<string> vAddress = new List<string>();
            List<string> vCC = new List<string>();
            string strBody = string.Empty;

            vAddress.Add(tbAddress.Text);
            vCC.Add(tbCC.Text);

            strBody += "Dear Julio:\n\n";
            foreach (Report r in reports)
            {
                strBody += r.ToString() + Environment.NewLine;                
            }

            string strResult = MailToUtility.SendMailToURI(vAddress, vCC, null, tbSubject.Text, strBody);
            if (!string.IsNullOrEmpty(strResult))
                MessageBox.Show(strResult);
        }

        private List<GitLog> LoadGitReport(string dateTimeArgument, string gitPath)
        {
            List<GitLog> logs = new List<GitLog>();
            GitMessageParser.GitMessageParser parser = new GitMessageParser.GitMessageParser(gitPath);
            if (GitMessageParser.GitMessageParser.IsGitInstalled())
            {
                return parser.ReadLogsDirect(dateTimeArgument, "tenny");
            }
            return logs;
        }

        private List<GitLog> LoadGitReport(string dateSinceArgument, string dateUntilArgument,  string gitPath)
        {
            List<GitLog> logs = new List<GitLog>();
            GitMessageParser.GitMessageParser parser = new GitMessageParser.GitMessageParser(gitPath);
            if (GitMessageParser.GitMessageParser.IsGitInstalled())
            {
                return parser.ReadLogsDirect(dateSinceArgument, dateUntilArgument, "tenny");
            }
            return logs;
        }

        private List<GitLog> LoadScannerManagerGitReport(string dateTimeArgument)
        {
            List<GitLog> logs = new List<GitLog>();
            string wctPath = @"C:\Workspace\ScannerManager\.git";
            GitMessageParser.GitMessageParser parser = new GitMessageParser.GitMessageParser(wctPath);
            if (GitMessageParser.GitMessageParser.IsGitInstalled())
            {
                return parser.ReadLogsDirect(dateTimeArgument, "tenny");
            }
            return logs;
        }

        private List<GitLog> LoadWCTGitReport(string dateTimeArgument)
        {
            List<GitLog> logs = new List<GitLog>();
            string wctPath = @"C:\Workspace\WorldCardTeam\.git";
            GitMessageParser.GitMessageParser parser = new GitMessageParser.GitMessageParser(wctPath);
            if (GitMessageParser.GitMessageParser.IsGitInstalled())
            {
                return parser.ReadLogsDirect(dateTimeArgument, "tenny");
            }
            return logs;
        }

        private bool HasMantisBug(string commitMessage)
        {
            // BUG：修正 Mantis 上的問題
            string pattern = "BUG(：|:)修正 *(M|m)antis *上的問題+";
            var regex = new Regex(pattern);
            var match = regex.Match(commitMessage);
            if (match.Success)
                return true;
            return false;
        }

        private string TrimMantisHeader(string header)
        {
            // BUG：修正 Mantis 上的問題
            string pattern = "BUG(：|:)修正 *(M|m)antis *上的問題(。|.)+";
            var regex = new Regex(pattern);
            var match = regex.Match(header);
            if (match.Success)
            {
                return header.Substring(match.Index + match.Length);
            }
            return header;
        }

        /// <summary>
        /// 回傳沒有mantis的行數，並加入mantis數字到輸入的mantisSet內。
        /// </summary>
        private List<string> FindMantisNumbers(List<string> commitMessageLines, ref HashSet<string> mantisSet)
        {
            List<string> remainLines = new List<string>();
            // 5~7位數字，前面有一個非數字字元，後面有一個非數字字元
            string pattern = @"[^\d]\d{5,7}[^\d]";
            Regex regex = new Regex(pattern);
            for (int i = 0; i< commitMessageLines.Count; i++)
            {
                string commitMessage = commitMessageLines[i];
                if (string.IsNullOrWhiteSpace(commitMessage))
                    continue;

                bool found = false;
                int lastMatchIndex = 0;
                var match = regex.Match(commitMessage);
                while (match.Success)
                {
                    found = true;
                    lastMatchIndex = match.Index + match.Length;
                    string matisNum = commitMessage.Substring(match.Index + 1, match.Length - 2);
                    mantisSet.Add(matisNum);

                    // 找下一個
                    commitMessage = commitMessage.Substring(match.Index + match.Length, commitMessage.Length - match.Index - match.Length);
                    if (string.IsNullOrEmpty(commitMessage))
                        break;
                    else
                        match = regex.Match(commitMessage);
                }

                // 此行沒有 mantis number，就不能省略
                if (!found)
                {
                    remainLines.Add(commitMessageLines[i]);
                }
                else if (commitMessage.Length > 0)
                {   // 剩下的文字資訊
                    remainLines.Add(commitMessage);
                }
            }
            return remainLines;
        }

        private string TrimIndexHeader(string commitLine)
        {
            // 開頭的數字 + . + 可能的空白
            string pattern = @"^\d+. *";
            Regex regex = new Regex(pattern);
            var match = regex.Match(commitLine);
            if (match.Success)
            {
                commitLine = commitLine.Substring(match.Length, commitLine.Length - match.Length);
            }
            return commitLine;
        }

        private async Task WriteCommitReportToDB(DatabaseManager dbm, GitLog gl, CommitReport cr)
        {
            ProjectReport pr = new ProjectReport();
            pr.Version = cr.Version;
            pr.ProjectName = "WorldCardTeam";
            int pid = await dbm.TryAddProject(pr);

            Commit c = new Commit();
            c.Author = gl.Author;
            c.Hash = gl.hash;
            c.Message = gl.CommitMessage;
            c.UnixTimestamp = gl.TimeStamp;
            c.ProjectId = pid;
            await dbm.WriteAsync(c);
        }

        private async void btnSaveDB_Click(object sender, RoutedEventArgs e)
        {
            btnSaveDB.IsEnabled = false;

            DatabaseManager dbm = new DatabaseManager();
            WebDBManager webM = new WebDBManager(serverUrl);
            await dbm.Init();
            if (dbm.Open())
            {
                foreach (Report report in reports)
                {
                    ProjectReport pr = new ProjectReport();
                    pr.Version = report.ProjectVersion;
                    pr.ProjectName = report.ProjectName;
                    int pid = await dbm.TryAddProject(pr);
                    int remotePid = await webM.TryAddProjectAsync(pr);

                    if (pid != remotePid)
                    {
                        string errorMessage = string.Format("local & remote pid not match!\n{0} != {1}", pid, remotePid);
                        MessageBox.Show(errorMessage);
                    }

                    foreach (MyProgress mp in report.DailyProgresses)
                    {
                        if (!string.IsNullOrEmpty(mp.MyValue))
                        {
                            DailyReportModel drm = new DailyReportModel();
                            drm.Date = DateTime.Today;
                            drm.Message = mp.MyValue;
                            drm.ProjectId = pid;
                            await dbm.WriteAsync(drm);
                            await webM.AddNew(drm, "DailyReport");
                        }
                    }
                    foreach (string number in report.MantisList)
                    {
                        Mantis m = new Mantis() { MantisNumber = number, Date = sinceDate, ProjectId = pr.Id };
                        await dbm.WriteAsync(m);
                        await webM.AddNew(m, "Mantis");
                    }
                }
                MessageBox.Show("OK");
            }
            else
                MessageBox.Show("DB open failed!");
            dbm.Dispose();
            webM.Dispose();

            btnSaveDB.IsEnabled = true;
        }

        private async Task<string> GenerateReport(DateTime startDay, DateTime endDay, bool useRemoteDb , bool mergeSameProject = false)
        {
            string strText = "";
            // get this week's DailyReportModel
            DatabaseManager dbm = new DatabaseManager();
            await dbm.Init();
            if (dbm.Open())
            {
                IEnumerable<PeriodReport> alist;
                if (useRemoteDb)
                {
                    using (WebDBManager wdManager = new WebDBManager(serverUrl))
                    {
                        alist = await wdManager.ReadPeriodReportAsync(startDay, endDate);
                    }
                }
                else
                    alist = await dbm.ReadPeriodReportAsync(startDay, endDay);
                //// get daily reports
                //List<DailyReportModel> dList = await dbm.ReadReportAsync(startDay, endDay);
                //// get all project list
                //List<ProjectReport> prList = await dbm.ReadAllProjectsAsync();
                //// filter unused projects
                //HashSet<int> projectIDs = new HashSet<int>(dList.Select(x => x.ProjectId));
                //prList = prList.Where(x => projectIDs.Contains(x.Id)).ToList();
                //// make reprt list
                List<AccumulatedReport> weeklyReport = new List<AccumulatedReport>();
                //foreach (ProjectReport pr in prList)
                //{
                //    AccumulatedReport ar = new AccumulatedReport();
                //    ar.ProjectId = pr.Id;
                //    ar.ProjectName = pr.ProjectName;
                //    ar.ProjectVersion = pr.Version;
                //    weeklyReport.Add(ar);
                //}
                // merge weekly report list
                string pattern = @"^修正 *(M|m)antis *上的問題(：|:)";
                Regex regex = new Regex(pattern);
                foreach (PeriodReport drm in alist)
                {
                    AccumulatedReport ar = weeklyReport.FirstOrDefault(x => x.ProjectId == drm.ProjectID);
                    if (ar == null)
                    {
                        ar = new AccumulatedReport() { ProjectName = drm.ProjectName, ProjectId = drm.ProjectID, ProjectVersion = drm.Version };
                        weeklyReport.Add(ar);
                    }

                    // check if line contains mantis number
                    var match = regex.Match(drm.Message);
                    if (match.Success)
                    {
                        // add mantis number to project
                        string mantisLine = drm.Message.Substring(match.Length, drm.Message.Length - match.Length);
                        string[] mantisItems = mantisLine.Split('、');
                        foreach (string item in mantisItems)
                            ar.MantisList.Add(item);
                    }
                    else
                    {
                        // add message line to corresponding project
                        ar.ProjectContent.Add(drm.Message);
                    }

                }

                // sort and merge
                if (mergeSameProject)
                    weeklyReport = MergeSameReports(weeklyReport);
                weeklyReport.Sort();

                // pipe to notepad
                foreach (AccumulatedReport r in weeklyReport)
                {
                    strText += r.ToString() + Environment.NewLine;
                }
                NotepadEditor.PipeTextToNotepad(strText);                
                // mail it
                /*List<string> vAddress = new List<string>();
                List<string> vCC = new List<string>();
                string strBody = string.Empty;
                string strHeader = "每周進度 " + startDay.ToShortDateString() + " ~ " + endDay.ToShortDateString();
                vAddress.Add(tbAddress.Text);
                vCC.Add(tbCC.Text);

                strBody += "Dear Julio:\n\n";
                foreach (AccumulatedReport r in weeklyReport)
                {
                    strBody += r.ToString() + Environment.NewLine;
                }

                string strResult = MailToUtility.SendMailToURI(vAddress, vCC, null, strHeader, strBody);
                if (!string.IsNullOrEmpty(strResult))
                    MessageBox.Show(strResult);
                */
            }
            dbm.Dispose();
            return strText;
        }

        private async Task<string> GenerateWeekly()
        {
            // calculate date
            DayOfWeek dayOfWeek = System.Globalization.CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(DateTime.Today);
            int shiftedDay = (int)dayOfWeek;
            DateTime startDay = DateTime.Today.AddDays(-shiftedDay);
            DateTime endDay = startDay.AddDays(7);
            return await GenerateReport(startDay, endDay, false);
        }

        private async void btnWeekly_Click(object sender, RoutedEventArgs e)
        {
            await GenerateWeekly();
        }

        private async void btnMonthly_Click(object sender, RoutedEventArgs e)
        {
            // calculate date
            DateTime startDay = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            int monthDays = DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month);
            DateTime endDay = startDay.AddDays(monthDays - 1);
            await GenerateReport(startDay, endDay, false);
        }

        private void sinceDatePicker_SelectedDateChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            sinceDate = sinceDatePicker.SelectedDate.Value;
            tbSubject.Text = sinceDate.ToString("MM/dd", System.Globalization.CultureInfo.InvariantCulture) + " 進度報告";
        }


        private void endDatePicker_SelectedDateChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (endDatePicker.SelectedDate.Value >= sinceDate)
            {
                endDate = endDatePicker.SelectedDate.Value;
            }
            else
            {                
                MessageBox.Show("End date must be later!");
                endDatePicker.SelectedDate = endDate;  // window loaded value
            }
        }

        private async void btnGo_Click(object sender, RoutedEventArgs e)
        {
            // set time to previus date at 23:59
            DateTime yesterday = sinceDate.AddDays(-1);
            yesterday = new DateTime(
                yesterday.Year,
                yesterday.Month,
                yesterday.Day,
                23,
                59,
                59);
            DateTime untilDate = endDate.AddDays(1);

            reportPanel.Children.Clear();
            //mantisNumbers.Clear();
            reports.Clear();

            if (cbWC8.IsChecked == true)
                await LoadCommits("WorldCard", @"C:\Workspace\WorldCard8\.git", yesterday, untilDate);
            if (cbWCT.IsChecked == true)
                await LoadCommits("WorldCardTeam", @"C:\Workspace\WorldCardTeam\.git", yesterday, untilDate);
            if (cbWCE.IsChecked == true)
                await LoadCommits("WorldCard Enterprise", @"C:\Workspace\WorldCardEnterprice\.git", yesterday, untilDate);
            if (cbSM.IsChecked == true)
                await LoadCommits("Scanner Manager", @"C:\Workspace\ScannerManager\.git", yesterday, untilDate);
            if (cbPDF.IsChecked == true)
                await LoadCommits("PdfViewer", @"C:\Workspace\PDFView\PDF View 2\.git", yesterday, untilDate);
        }

        private void btnAllView_Click(object sender, RoutedEventArgs e)
        {
            AllView allViewWin;
            if (MessageBox.Show(this, "Use remore server " + serverUrl + " ?", string.Empty, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                allViewWin = new AllView(serverUrl);
            else
                allViewWin = new AllView();
            allViewWin.Owner = this;
            allViewWin.ShowDialog();
            e.Handled = true;
        }

        private async void btnTimeSpan_Click(object sender, RoutedEventArgs e)
        {
            TimeSpanWindow tsw = new TimeSpanWindow();
            tsw.Owner = this;
            if (tsw.ShowDialog() == true)
            {
                await GenerateReport(tsw.StartDate, tsw.EndDate, false, tsw.MergeSameProject);
            }
        }

        private async void btnGoogleSheet_Click(object sender, RoutedEventArgs e)
        {
            btnGoogleSheet.IsEnabled = false;
            GoogleSheet googleSheet = new GoogleSheet();
            if (googleSheet.Init())
            {
                SheetPreview sheetPreview = await googleSheet.GetSheetPreviewAsync();
                if (sheetPreview != null)
                {
                    string report = await GenerateWeekly();
                    if (string.IsNullOrEmpty(report))
                        MessageBox.Show("Report is empty!");
                    else
                    {
                        try
                        {
                            GoogleSheetWindow gsw = new GoogleSheetWindow(sheetPreview, report);
                            gsw.Owner = this;
                            if (gsw.ShowDialog() == true)                            
                                await googleSheet.UpdateSheetAsync(gsw.MakeRowData());                            
                            else
                                MessageBox.Show("Do nothing.");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }
                }
            }
            googleSheet.Dispose();
            btnGoogleSheet.IsEnabled = true;
        }

        private void btnMigerate_Click(object sender, RoutedEventArgs e)
        {
            btnMigerate.IsEnabled = false;
            if (MessageBox.Show("Upload ALL data to server?", "Warning", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    UploadDBWindow udw = new UploadDBWindow(serverUrl);
                    udw.Owner = this;
                    udw.ShowDialog();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            btnMigerate.IsEnabled = true;
        }

        private List<AccumulatedReport> MergeSameReports(IReadOnlyList<AccumulatedReport> reports)
        {
            Dictionary<string, AccumulatedReport> dict = new Dictionary<string, AccumulatedReport>();
            foreach (var report in reports)
            {
                if (report.ProjectName == null)
                    report.ProjectName = string.Empty;

                if (dict.ContainsKey(report.ProjectName))
                {
                    dict[report.ProjectName].ProjectContent.AddRange(report.ProjectContent);
                    dict[report.ProjectName].MantisList.UnionWith(report.MantisList);
                    // TODO: use highest version number
                }
                else
                    dict.Add(report.ProjectName, report);
            }

            return dict.Values.ToList();
        }
    }
}
