using Prism.Commands;
using Prism.Mvvm;
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
using System.Windows.Controls;

namespace VimPDFViewer.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Prism Application";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public DelegateCommand Click_Menu_Open { get; }

        public DelegateCommand<Object> ToggleButton_File_Loaded { get; }

        public DelegateCommand<Object> ToggleButton_Edit_Loaded { get; }

        public MainWindowViewModel()
        {
            Click_Menu_Open = new DelegateCommand(openFile);
            ToggleButton_File_Loaded = new DelegateCommand<Object>(fileLoaded);
            ToggleButton_Edit_Loaded = new DelegateCommand<Object>(fileEdit);
        }

        private void fileLoaded(Object sender)
        {
            var btn = (ToggleButton)sender;
            btn.SetBinding(ToggleButton.IsCheckedProperty, new Binding("IsOpen") { Source = btn.ContextMenu });
            btn.ContextMenu.PlacementTarget = btn;
            btn.ContextMenu.Placement = PlacementMode.Bottom;
        }

        private void fileEdit(Object sender)
        {
            var btn = (ToggleButton)(sender);
            btn.SetBinding(ToggleButton.IsCheckedProperty, new Binding("IsOpen") { Source = btn.ContextMenu });
            btn.ContextMenu.PlacementTarget = btn;
            btn.ContextMenu.Placement = PlacementMode.Bottom;

        }

        private string FileName = "";
        private List<BitmapSource> pdfPages = new List<BitmapSource>();
        private PdfDocument pdfDocument;

        private void openFile()
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
        private ImageSource _pdfSource;
        public ImageSource PdfSource
        {
            get { return _pdfSource; }
            set { SetProperty(ref _pdfSource, value); }
        }

        private int _width;
        public int Width
        {
            get { return _width; }
            set { SetProperty(ref _width, value); }
        }

        private int _height;
        public int Height
        {
            get { return _height; }
            set { SetProperty(ref _height, value); }
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
            PdfSource = bmp;
            Width = bmp.PixelWidth;
            Height = bmp.PixelHeight;
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
