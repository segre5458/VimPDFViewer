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
using VimPDFViewer.Models;

namespace VimPDFViewer.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
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
                fileName = Path.GetFileName(fileName);
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
            BitmapImage bmp = BitmapManager.imageCombine(image);
            PdfSource = bmp;
            Width = bmp.PixelWidth;
            Height = bmp.PixelHeight;
        }
    }
}
