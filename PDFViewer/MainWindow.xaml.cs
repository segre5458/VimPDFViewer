using Microsoft.Win32;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.IO;
using Windows.Data.Pdf;
using System;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.Storage.Pickers;
using Windows.Storage;

namespace PDFViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string FileName;
        private List<BitmapSource> pdfPages = new List<BitmapSource>();
        private int DisplayPageNumber = 1;
        private PdfDocument pdfDocument;
        public MainWindow()
        {
            InitializeComponent();

            this.Title = "PdfViewr";
            this.AllowDrop = true;
            this.Drop += MainWindow_Drop;
        }

        private void MainWindow_Drop(Object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) == false) { return; }

            string[] filePath = (string[])e.Data.GetData(DataFormats.FileDrop);
            FileName = filePath[0];
            LoadPdf(FileName); 
        }

        private void ToggleButton_File_Loaded(object sender, RoutedEventArgs e)
        {
            var btn = (ToggleButton)sender;
            btn.SetBinding(ToggleButton.IsCheckedProperty, new Binding("IsOpen") { Source = btn.ContextMenu });
            btn.ContextMenu.PlacementTarget = btn;
            btn.ContextMenu.Placement = PlacementMode.Bottom;
        }

        private void ToggleButton_Edit_Loaded(object sender, RoutedEventArgs e)
        {
            var btn = (ToggleButton)(sender);
            btn.SetBinding(ToggleButton.IsCheckedProperty, new Binding("IsOpen") { Source = btn.ContextMenu });
            btn.ContextMenu.PlacementTarget = btn;
            btn.ContextMenu.Placement = PlacementMode.Bottom;
        }


        private async void MenuItem_Open_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "pdfファイル (*.pdf)|*.pdf|全てのファイル (*.*)|*.*";
            if (dialog.ShowDialog() == true)
            {
                FileName = dialog.FileName;
            }
            LoadPdf(FileName);
        }

        private async void LoadPdf(string fileName)
        {
            var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(fileName);
            if(file != null)
            {
                pdfDocument = await PdfDocument.LoadFromFileAsync(file);
                fileName = System.IO.Path.GetFileName(fileName);
                readPDFtoImage(0);
            }
        }
        private async Task readPDFtoImage(int pageIndex)
        {
            using(PdfPage page = pdfDocument.GetPage((uint)pageIndex))
            {
                var options = new PdfPageRenderOptions();
                using (var stream = new Windows.Storage.Streams.InMemoryRandomAccessStream())
                {
                    await page.RenderToStreamAsync(stream, options);

                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = stream.AsStream();//using System.IOがないとエラーになる
                    image.EndInit();
                    imgPDF.Source = image;
                    imgPDF.Width = image.PixelWidth;
                    imgPDF.Height = image.PixelHeight;
                }
            }
        }


        private void ShowPage(int p)
        {
            if (p < 1) return;
            if (pdfPages.Count == 0) return;
            if (p > pdfPages.Count) return;
            imgPDF.Source = pdfPages[p - 1];
        }
    }
}
