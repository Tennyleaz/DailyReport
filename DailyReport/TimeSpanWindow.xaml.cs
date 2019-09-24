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
    /// TimeSpanWindow.xaml 的互動邏輯
    /// </summary>
    public partial class TimeSpanWindow : Window
    {
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public bool MergeSameProject { get; private set; }

        public TimeSpanWindow()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (dateStart.SelectedDate.HasValue && dateEnd.SelectedDate.HasValue)
            {
                StartDate = dateStart.SelectedDate.Value;
                EndDate = dateEnd.SelectedDate.Value;
                MergeSameProject = mergeProjectBox.IsChecked == true;
                if (EndDate > StartDate)
                {
                    DialogResult = true;
                    Close();                    
                }
            }
        }
    }
}
