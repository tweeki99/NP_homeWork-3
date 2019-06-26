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

namespace NpHw_3.Client
{
    /// <summary>
    /// Логика взаимодействия для NameWindow.xaml
    /// </summary>
    public partial class NameWindow : Window
    {
        public NameWindow()
        {
            InitializeComponent();
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            if (nameBox.Text.Length == 0)
            {
                MessageBox.Show("Введите имя");
                return;
            }
            this.DialogResult = true;
        }

        public string UserName
        {
            get { return nameBox.Text; }
        }
    }
}
