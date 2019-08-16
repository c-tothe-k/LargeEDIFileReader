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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;

namespace LargeEDIFileReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Open_File(object sender, RoutedEventArgs e)
        {
            var picker = new OpenFileDialog();
            if (picker.ShowDialog() == true)
            {
                FileName.Text = $"File Open: {picker.FileName}";

                var ediStream = new EDIFileStream(picker.FileName);
                bool fileOk = FileUtils.OpenEDIFile(ediStream);
                if (!fileOk)
                {
                    MessageBox.Show("Error: File is not an X12 EDI file. Please try a different file.");
                }
                else
                {
                    TotalPages.Text += Convert.ToString(FileUtils.TotalPages);
                    FileContent.Text = FileUtils.LoadPage(FileUtils.NavigationType.Next);
                    CurrentPage.Text += Convert.ToString(FileUtils.CurrentPageNumber);
                }
            }
        }

        private void Perform_Search(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            FileUtils.Close();
        }

        private void PreviousPage(object sender, RoutedEventArgs e)
        {
            if (FileUtils.CurrentPageNumber > 1)
            {
                FileContent.Text = FileUtils.LoadPage(FileUtils.NavigationType.Previous);
                CurrentPage.Text = $"Current Page: {FileUtils.CurrentPageNumber}";
            }
        }

        private void NextPage(object sender, RoutedEventArgs e)
        {
            if (FileUtils.CurrentPageNumber < FileUtils.TotalPages)
            {
                FileContent.Text = FileUtils.LoadPage(FileUtils.NavigationType.Next);
                CurrentPage.Text = $"Current Page: {FileUtils.CurrentPageNumber}";
            }
        }
    }
}
