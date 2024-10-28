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


namespace BLIP
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
        public MainWindow(string filepath)
        {
            InitializeComponent();
            try
            {
                if (Path.GetExtension(filepath).ToLower() == ".blp")
                {
                    ReadBLP(filepath);
                    Title = $"BLIP - {filepath}";
                }
                if (Path.GetExtension(filepath).ToLower() == ".dds")
                {
                    ReadDDS(filepath, MainCanvas);
                    Title = $"BLIP - {filepath}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Clipboard.SetText(ex.ToString());
            }
        }

        private void ReadBLP(string path)
        {
            BitmapSource image = GEtBLP(path);
            if (image == null) {return; }
             Height = image.Height;
             Width= image.Width;
            MainCanvas.Children.Clear();
            MainCanvas.Children.Add(new Image() { Source = image, Height = image.Height, Width = image.Width});
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
                    return ConvertBitmapToBitmapSource(bitmap);
                }
            }
            return null;
        }

        public static BitmapSource ConvertBitmapToBitmapSource(Bitmap bitmap)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                // Save the bitmap to the memory stream in a supported format
                bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);
                memoryStream.Position = 0; // Reset the stream position

                // Create a BitmapImage and set the stream as the source
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad; // Load the image into memory
                bitmapImage.EndInit();
                bitmapImage.Freeze(); // Freeze to make it cross-thread accessible

                return bitmapImage;
            }
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
                whichCanvas.Background = new ImageBrush(bitmapSource);
            }
        }


    }
}
