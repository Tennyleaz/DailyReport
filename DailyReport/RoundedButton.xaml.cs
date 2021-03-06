﻿using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DailyReport
{
    /// <summary>
    /// RoundedButton.xaml 的互動邏輯
    /// </summary>
    public partial class RoundedButton : UserControl
    {
        public RoundedButton()
        {
            InitializeComponent();
        }

        public event RoutedEventHandler Click;

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Click?.Invoke(this, e);
        }
    }
}
