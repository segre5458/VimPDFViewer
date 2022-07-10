using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Windows.Data.Pdf;
using Windows.Storage;

namespace PDFViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);
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
            var file = await StorageFile.GetFileFromPathAsync(fileName);
            if (file != null)
            {
                pdfDocument = await PdfDocument.LoadFromFileAsync(file);
                fileName = System.IO.Path.GetFileName(fileName);
                await readPDFtoImage(pdfDocument);
            }
        }
        private async Task readPDFtoImage(PdfDocument pdfDocument)
        {
            Debug.WriteLine(pdfDocument.PageCount);
            BitmapImage[] image = new BitmapImage[pdfDocument.PageCount];
            for (uint pageNum = 0; pageNum < pdfDocument.PageCount; pageNum++)
            {
                using (PdfPage page = pdfDocument.GetPage(pageNum))
                {
                    var options = new PdfPageRenderOptions();
                    using (var stream = new Windows.Storage.Streams.InMemoryRandomAccessStream())
                    {
                        await page.RenderToStreamAsync(stream, options);

                        BitmapImage img = new BitmapImage();
                        img.BeginInit();
                        img.CacheOption = BitmapCacheOption.OnLoad;
                        img.StreamSource = stream.AsStream();
                        img.EndInit();
                        image[pageNum] = img;
                    }
                }
            }
            BitmapImage bmp = imageCombine(image);
            imgPDF.Source = bmp;
            imgPDF.Width = bmp.PixelWidth;
            imgPDF.Height = bmp.PixelHeight;
        }

        private BitmapImage imageCombine(BitmapImage[] bmpimg)
        {
            Bitmap[] bmp = new Bitmap[bmpimg.Length];
            for (int i = 0; i < bmpimg.Length; i++)
            {
                bmp[i] = BitmapImage2Bitmap(bmpimg[i]);
            }
            int dstWidth = 0, dstHeight = 0;
            for (int i = 0; i < bmp.Length; i++)
            {
                if (dstWidth < bmp[i].Width)
                {
                    dstWidth = bmp[i].Width;
                }
                dstHeight += bmp[i].Height;
            }

            var dst = new Bitmap(dstWidth, dstHeight);
            var dstRect = new Rectangle();
            using (var gs = Graphics.FromImage(dst))
            {
                for (int i = 0; i < bmp.Length; i++)
                {
                    dstRect.Width = bmp[i].Width;
                    dstRect.Height = bmp[i].Height;
                    gs.DrawImage(bmp[i], dstRect, 0, 0, bmp[i].Width, bmp[i].Height, GraphicsUnit.Pixel);
                    dstRect.Y = dstRect.Bottom;
                }
            }
            return Bitmap2BitmapImage(dst);
        }
        private Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            // BitmapImage bitmapImage = new BitmapImage(new Uri("../Images/test.png", UriKind.Relative));

            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        private BitmapImage Bitmap2BitmapImage(Bitmap bmp)
        {
            BitmapImage bitmapImage = new BitmapImage();
            using (MemoryStream memory = new MemoryStream())
            {
                bmp.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
            }
            return bitmapImage;
        }
    }
}
