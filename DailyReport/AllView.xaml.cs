﻿using System;
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

        public AllView()
        {
            InitializeComponent();
            reportList = new ObservableCollection<PeriodReport>();
            reportListView.ItemsSource = reportList;

            Loaded += AllView_Loaded;
        }

        private async void AllView_Loaded(object sender, RoutedEventArgs e)
        {
            DatabaseManager dbm = new DatabaseManager();
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
    }
}