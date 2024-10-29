using System;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using War3Net.Drawing.Blp;
using Image = System.Windows.Controls.Image;
using System.Windows.Controls;
using ImageMagick;
using System.Windows.Interop;
using static System.Net.Mime.MediaTypeNames;


namespace BLIP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string path = "";
        string extension = "";
        private double TitleHeight = 50;
        public MainWindow()
        {
            InitializeComponent();
            
             
        }
        public MainWindow(string filepath)
        {
            InitializeComponent();
          //  GetTitleBarHeight();
            try
            {
                if (Path.GetExtension(filepath).ToLower() == ".blp")
                {
                    path = filepath; extension = "blp";
                    Title = $"BLIP - {path}";
                    ReadBLP(path);

                }
                if (Path.GetExtension(filepath).ToLower() == ".dds")
                {
                    path = filepath; extension = "dds";
                    ReadDDS(path, MainCanvas);
                    Title = $"BLIP - {path}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Clipboard.SetText(ex.ToString());
            }
        }
        private void GetTitleBarHeight()
        {
            // Initialize the window size temporarily to measure non-client area
            this.Width = 100;
            this.Height = 100;

            // Force layout update to ensure dimensions are accurate
            this.UpdateLayout();

            // Calculate title bar height as the difference between window height and client area height
            TitleHeight = this.Height - this.ActualHeight;
        }
        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            
        }
        private void ReadBLP(string path)
        {
            BitmapSource image = GEtBLP(path);
            if (image == null) { return; }

            MainCanvas.Width =   image.Width; ; 
            MainCanvas.Height =  image.Height   ; ;
            MainCanvas.Children.Clear();
            MainCanvas.Children.Add(new Image() { Source =  image });
           // ResizeBitmapSourceToFitCanvas(image, MainCanvas, image.Height, image.Width);
             Height =  MainCanvas.Height + TitleHeight;
            Width =  MainCanvas.Width + 30;
        }
        public void ResizeBitmapSourceToFitCanvas(BitmapSource bitmapSource, Canvas canvas, double height, double width)
        {
            if (bitmapSource == null || canvas == null)
                return;

            // Set canvas size
            canvas.Height = height;
            canvas.Width = width;

            // Calculate aspect ratio
            double aspectRatio = bitmapSource.Width / bitmapSource.Height;

            // Determine new dimensions while maintaining the aspect ratio
            double newWidth, newHeight;

            // Fit the image within the canvas dimensions
            if (width / height > aspectRatio)
            {
                // Fit by height
                newHeight = height;
                newWidth = height * aspectRatio; // Scale based on height
            }
            else
            {
                // Fit by width
                newWidth = width;
                newHeight = width / aspectRatio; // Scale based on width
            }

            // Create a new Image element
            Image imageControl = new Image
            {
                Source = bitmapSource,
                Width = newWidth,
                Height = newHeight
            };

            // Center the image in the canvas
            Canvas.SetLeft(imageControl, (width - newWidth) / 2);
            Canvas.SetTop(imageControl, (height - newHeight) / 2);

            // Clear existing children and add the resized image
            canvas.Children.Clear();
            canvas.Children.Add(imageControl);
            canvas.Background = System.Windows.Media.Brushes.Black;
        }
        public BitmapSource GEtBLP(string path)
        {
            if (File.Exists(path))
            {
                using (FileStream fileStream = File.OpenRead(path))
                {
                    // Read the BLP file
                    var blpFile = new BlpFile(fileStream);
                    var bitmapSource = blpFile.GetBitmapSource();

                    // Convert BitmapSource to Bitmap
                    var bitmap = new Bitmap(bitmapSource.PixelWidth, bitmapSource.PixelHeight, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                    var data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                    bitmapSource.CopyPixels(Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
                    bitmap.UnlockBits(data);

                    // Return the converted BitmapSource
                    return bitmapSource;
                }
            }
            return null;
        }

      
       
    
    private void ReadDDS(string filePath, Canvas whichCanvas)
        {
            // Load the DDS image using Magick.NET
            using (var magickImage = new MagickImage(filePath))
            {
                // Ensure the color space is sRGB
                magickImage.ColorSpace = ColorSpace.sRGB;

                // Convert the image to a byte array with RGBA mapping
                var pixels = magickImage.GetPixels().ToByteArray(PixelMapping.RGBA);

                // Convert each pixel from RGBA to BGRA format for WPF compatibility
                for (int i = 0; i < pixels.Length; i += 4)
                {
                    byte red = pixels[i];
                    byte green = pixels[i + 1];
                    byte blue = pixels[i + 2];
                    byte alpha = pixels[i + 3];

                    // Rearrange to BGRA
                    pixels[i] = blue;
                    pixels[i + 1] = green;
                    pixels[i + 2] = red;
                    pixels[i + 3] = alpha;
                }

                // Create the WriteableBitmap with correctly cast dimensions and pixel format
                var bitmapSource = new WriteableBitmap(
                    (int)magickImage.Width,  // Cast to int
                    (int)magickImage.Height, // Cast to int
                    96,  // Standard DPI
                    96,  // Standard DPI
                    PixelFormats.Bgra32,  // BGRA format for WPF
                    null);

                // Copy the processed pixel array into the WriteableBitmap
                bitmapSource.WritePixels(
                    new System.Windows.Int32Rect(0, 0, (int)magickImage.Width, (int)magickImage.Height),
                    pixels,
                    (int)magickImage.Width * 4, // Cast width to int and calculate stride
                    0);

                // Set the image as the background of the Canvas
                MainCanvas.Width = bitmapSource.Width; ;
                MainCanvas.Height = bitmapSource.Height; ;
                Width = bitmapSource.Width + 30;
                Height = bitmapSource.Height + TitleHeight;
                whichCanvas.Background = new ImageBrush(bitmapSource);
            }
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape) {Environment.Exit(0);}
        }
    }
}
