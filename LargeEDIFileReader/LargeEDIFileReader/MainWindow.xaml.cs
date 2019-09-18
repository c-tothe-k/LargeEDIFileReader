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
                SearchResults.Text = String.Empty;
                FileName.Text = $"File Open: {picker.FileName}";

                var fileStream = File.OpenRead(picker.FileName);
                var ediStream = new EDIFileStream(fileStream);

                bool fileOk = FileUtils.OpenEDIFile(ediStream);
                if (!fileOk)
                {
                    MessageBox.Show("Error: File is not an X12 EDI file. Please try a different file.");
                }
                else
                {
                    TotalPages.Text = $"Total Pages: {Convert.ToString(FileUtils.TotalPages)}";
                    FileContent.Text = FileUtils.LoadPage(FileUtils.NavigationType.Next);
                    CurrentPage.Text = $"Current Page: {Convert.ToString(FileUtils.CurrentPageNumber)}";
                    UpdateLineNumbers();
                }
            }
        }

        private void Perform_Search(object sender, RoutedEventArgs e) =>               
             SearchResults.Text = FileUtils.PerformSearch(EDIFileStream.SearchType.Text, SearchTerm.Text);        

        private void Perform_Search_Count(object sender, RoutedEventArgs e) =>
             SearchResults.Text = FileUtils.PerformSearch(EDIFileStream.SearchType.CountOnly, SearchTerm.Text);

        private void Perform_GotoLine(object sender, RoutedEventArgs e)
        {
            int lineToGoTo = 0;
            if ( Int32.TryParse( GotoLine.Text, out lineToGoTo) )
            {
                JumpToSegment(lineToGoTo);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) =>
            FileUtils.Close();
        

        private void PreviousPage(object sender, RoutedEventArgs e)
        {
            if (FileUtils.CurrentPageNumber > 1)
            {
                FileContent.Text = FileUtils.LoadPage(FileUtils.NavigationType.Previous);
                CurrentPage.Text = $"Current Page: {FileUtils.CurrentPageNumber}";
                UpdateLineNumbers();
            }
        }

        private void NextPage(object sender, RoutedEventArgs e)
        {
            if (FileUtils.CurrentPageNumber < FileUtils.TotalPages)
            {
                FileContent.Text = FileUtils.LoadPage(FileUtils.NavigationType.Next);
                CurrentPage.Text = $"Current Page: {FileUtils.CurrentPageNumber}";
                UpdateLineNumbers();
            }
        }

        private void UpdateLineNumbers()
        {
            int pageSegmentSize = 10_000; //TODO get this out of the edistream instance instead of hardcode
            var lineNumbers = Enumerable.Range(1, pageSegmentSize)
                      .Select(i => Convert.ToString(i + pageSegmentSize * (FileUtils.CurrentPageNumber - 1))).ToArray();
            Gutter.Text = String.Join(Environment.NewLine, lineNumbers);

        }

        private void JumpToSegment(int segmentLineNumber)
        {
            int pageStart = 0;
            FileContent.Text = FileUtils.LoadPageFromSegmentPosition(segmentLineNumber, out pageStart);
            int exactLine = segmentLineNumber - pageStart - 10; //"magic number" is to get the result in the center-ish of the window
            FileContent.ScrollToLine(exactLine);
            FileContent.SelectionStart = FileContent.GetCharacterIndexFromLineIndex(exactLine);


            CurrentPage.Text = $"Current Page: {FileUtils.CurrentPageNumber}";
        }

        private void NavigateToElement(object sender, MouseButtonEventArgs e)
        {
            int cursorPos = SearchResults.CaretIndex;
            int curLine = SearchResults.GetLineIndexFromCharacterIndex(cursorPos);
            string clickedLine = SearchResults.GetLineText(curLine);
            int segmentNum = Convert.ToInt32(clickedLine.Split(':')[0]);
            JumpToSegment(segmentNum);


        }

        private void FileContent_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            //keep the gutter on track with the file
            Gutter.ScrollToVerticalOffset(e.VerticalOffset);
          
            //TODO: add one for the gutter as well
        }
    }
}
