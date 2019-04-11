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
using Google.Apis.Sheets.v4.Data;

namespace DailyReport
{
    /// <summary>
    /// GoogleSheetWindow.xaml 的互動邏輯
    /// </summary>
    public partial class GoogleSheetWindow : Window
    {
        private readonly SheetPreview _preview;
        private string _report;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="preview"></param>
        /// <param name="report"></param>
        /// <exception cref="ArgumentNullException"/>
        public GoogleSheetWindow(SheetPreview preview, string report)
        {
            InitializeComponent();
            Loaded += GoogleSheetWindow_Loaded;

            _preview = preview;
            _report = report;
            if (string.IsNullOrEmpty(report))
                throw new ArgumentNullException(nameof(report));
            if (preview == null)
                throw new ArgumentNullException(nameof(preview));
        }

        private void GoogleSheetWindow_Loaded(object sender, RoutedEventArgs e)
        {
            tbPreviousWeekDate.Text = _preview.PreviousWeekDate;
            tbPreviousWeekContent.Text = _preview.PreviousWeekContent;
            tbPreviousWeekPlan.Text = _preview.PreviousWeekPlan;
            tbThisWeekDate.Text = _preview.NextWeekDate;

            if (!string.IsNullOrEmpty(_preview.NextWeekContent))
            {
                tbThisWeekContent.Text = _preview.NextWeekContent;
                MessageBox.Show(this, "Report has content already.");
            }
            else
            {
                tbThisWeekContent.Text = _report;
            }

            if (string.IsNullOrEmpty(_preview.NextWeekDate))
                tbThisWeekDate.Text = DateTime.Today.ToString("yyyy/M/dd", System.Globalization.CultureInfo.InvariantCulture);
        }

        private void RoundedButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbThisWeekContent.Text))
                DialogResult = true;
        }

        public List<RowData> MakeRowData()
        {
            CellData reportCell = new CellData()
            {
                UserEnteredValue = new ExtendedValue() { StringValue = tbThisWeekContent.Text }
            };
            CellData planCell = new CellData()
            {
                UserEnteredValue = new ExtendedValue() { StringValue = tbThisWeekPlan.Text }
            };

            RowData rowData1 = new RowData()
            {
                Values = new List<CellData>() { reportCell }
            };
            RowData rowData2 = new RowData()
            {
                Values = new List<CellData>() { planCell }
            };

            List<RowData> vData = new List<RowData>();
            vData.Add(rowData1);
            vData.Add(rowData2);
            return vData;
        }
    }
}
