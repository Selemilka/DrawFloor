using System;
using System.Collections.Generic;
using System.IO;
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

namespace DrawFloor
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            double fieldsForPrinter = ToPixels(20);
            double canvasWidth = 2490, canvasHeight = 3510;
            double listWidth = 210, listHeight = 297;

          //  int cnt = (int)Math.Round(ToPixels(listWidth) / ToPixels(40));

            double bigSquareLength = ToPixels(22.5), smallSquareLength = ToPixels(5);
            
            for (int i = 0; i < 1000; ++i)
            {
                for (int j = -100; j < 100; ++j)
                {
                    double x1 = bigSquareLength * j + smallSquareLength * i + fieldsForPrinter, x2 = x1;
                    double y1 = bigSquareLength * i - smallSquareLength * j + fieldsForPrinter, y2 = y1 + bigSquareLength + smallSquareLength;
                    x1 = Math.Max(x1, fieldsForPrinter);
                    y1 = Math.Max(y1, fieldsForPrinter);
                    x2 = Math.Min(x2, canvasWidth - fieldsForPrinter);
                    y2 = Math.Min(y2, canvasHeight - fieldsForPrinter);
                    if (x1 <= x2 && y1 <= y2)
                        canvasMain.Children.Add(GetLine(x1, y1, x2, y2));

                    x1 = bigSquareLength * j + smallSquareLength * i + fieldsForPrinter; x2 = x1 + bigSquareLength;
                    y1 = bigSquareLength * i - smallSquareLength * j + fieldsForPrinter; y2 = y1;
                    x1 -= smallSquareLength;
                    x1 = Math.Max(x1, fieldsForPrinter);
                    y1 = Math.Max(y1, fieldsForPrinter);
                    x2 = Math.Min(x2, canvasWidth - fieldsForPrinter);
                    y2 = Math.Min(y2, canvasHeight - fieldsForPrinter);
                    if (x1 <= x2 && y1 <= y2)
                        canvasMain.Children.Add(GetLine(x1, y1, x2, y2));
                }
            }
            
            Line line = GetLine(fieldsForPrinter, fieldsForPrinter, fieldsForPrinter, canvasHeight - fieldsForPrinter);
            canvasMain.Children.Add(line);
            line = GetLine(fieldsForPrinter, fieldsForPrinter, canvasWidth - fieldsForPrinter, fieldsForPrinter);
            canvasMain.Children.Add(line);
            line = GetLine(canvasWidth - fieldsForPrinter, fieldsForPrinter, canvasWidth - fieldsForPrinter, canvasHeight - fieldsForPrinter);
            canvasMain.Children.Add(line);
            line = GetLine(fieldsForPrinter, canvasHeight - fieldsForPrinter, canvasWidth - fieldsForPrinter, canvasHeight - fieldsForPrinter);
            canvasMain.Children.Add(line);
        }

        public static Line GetLine(double x1, double y1, double x2, double y2) => new Line()
        {
            X1 = x1,
            X2 = x2,
            Y1 = y1,
            Y2 = y2,
            Stroke = Brushes.Black,
            StrokeThickness = 1,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        };

        public static double ToPixels(double x) => x / 210 * 2490;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            /*
            BitmapSource bitmapSource = CaptureScreen(canvasMain, 2490, 3510);

            PngBitmapEncoder enc = new PngBitmapEncoder();

            enc.Frames.Add(BitmapFrame.Create(bitmapSource));

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "image",
                DefaultExt = ".png",
                Filter = "PNG Images (.png)|*.png"
            };
            var result = dlg.ShowDialog();

            if (result == true)
            {
                string filename = dlg.FileName;
                using (var stm = System.IO.File.Create(filename))
                {
                    enc.Save(stm);
                }
            }
            */
            ExportToPng(@"img.png", canvasMain);
        }

        private static BitmapSource CaptureScreen(Visual target, double dpiX, double dpiY)
        {
            if (target == null)
            {
                return null;
            }
            Rect bounds = VisualTreeHelper.GetDescendantBounds(target);
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)(bounds.Width * dpiX / 96.0),
                                                            (int)(bounds.Height * dpiY / 96.0),
                                                            dpiX,
                                                            dpiY,
                                                            PixelFormats.Pbgra32);
            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext ctx = dv.RenderOpen())
            {
                VisualBrush vb = new VisualBrush(target);
                ctx.DrawRectangle(vb, null, new Rect(new Point(), bounds.Size));
            }
            rtb.Render(dv);
            return rtb;
        }

        public void ExportToPng(string path, Canvas surface)
        {
            if (path == null) return;

            // Save current canvas transform
            Transform transform = surface.LayoutTransform;
            // reset current transform (in case it is scaled or rotated)
            surface.LayoutTransform = null;

            // Get the size of canvas
            Size size = new Size(surface.Width, surface.Height);
            // Measure and arrange the surface
            // VERY IMPORTANT
            surface.Measure(size);
            surface.Arrange(new Rect(size));

            // Create a render bitmap and push the surface to it
            RenderTargetBitmap renderBitmap =
              new RenderTargetBitmap(
                (int)size.Width,
                (int)size.Height,
                96d,
                96d,
                PixelFormats.Pbgra32);
            renderBitmap.Render(surface);

            // Create a file stream for saving image
            using (FileStream outStream = new FileStream(path, FileMode.Create))
            {
                // Use png encoder for our data
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                // push the rendered bitmap to it
                encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                // save the data to the stream
                encoder.Save(outStream);
            }

            // Restore previously saved layout
            surface.LayoutTransform = transform;
        }
    }
}
