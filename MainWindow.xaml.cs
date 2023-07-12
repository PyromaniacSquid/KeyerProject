using Microsoft.Win32;
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KeyerProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>


    public partial class MainWindow : Window
    {
        // Хранит исходное изображение
        BitmapImage wBitmap;
        // RGBA-данные текущего кеинга и исходного изображения
        PixelColor[,] pixels;
        PixelColor[,] origpixels;
        // RGBA Выбранного пользователем цвета
        PixelColor selectedKeyColor;
        // Коэффициенты для аппроксимации показателя непрозрачности
        float k1;       // Влияние зелёного цвета, определяется на основе цвета фона и k2
        float k2;       // Влияние синего цвета, суть - соотношение Bk/Gk, задается пользователем
        float k3;       // Влияние красного цвета, требуется для более точной настройки кеинга

        // Отслежка чекбокса автоматического выбора k1 (временно не используется)
        public static readonly DependencyProperty AutoGreenProperty =
            DependencyProperty.Register("k1BoxChecked", typeof(bool), 
                typeof(MainWindow), new PropertyMetadata(true));
        public bool k1BoxChecked
        {
            get { return (bool)GetValue(AutoGreenProperty); }
            set { SetValue(AutoGreenProperty, value); }
        }
        // RGBA-представление пикселя (0-255)
        public struct PixelColor
        {
            public byte B;
            public byte G;
            public byte R;
            public byte A;
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Преобразование изображения в матрицу RGBA-значений пикселей
        /// </summary>
        /// <param name="source">Исходное изображение</param>
        /// <param name="pixels">Целевая RGBA-матрица</param>
        /// <param name="stride">Шаг растрового изображения</param>
        /// <param name="offset">Начальная позиция копирования</param>
        private void CopyPixels(BitmapSource source, PixelColor[,] pixels, int stride, int offset)
        {
            var height = source.PixelHeight;
            var width = source.PixelWidth;
            var pixelBytes = new byte[height * width * 4];
            source.CopyPixels(pixelBytes, stride, offset);
            int y0 = offset / width;
            int x0 = offset - width * y0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                    pixels[x + x0, y + y0] = new PixelColor
                    {
                        B = pixelBytes[(y * width + x) * 4 + 0],
                        G = pixelBytes[(y * width + x) * 4 + 1],
                        R = pixelBytes[(y * width + x) * 4 + 2],
                        A = pixelBytes[(y * width + x) * 4 + 3]
                    };
            }
        }
        /// <summary>
        /// OBSOLETE. Возвращает расстояние между двумя RGB-векторами
        /// </summary>
        private double ColorDifference(PixelColor lhs, PixelColor rhs)
        {
            return Math.Sqrt(
                Math.Pow(lhs.R - rhs.R, 2)
                + Math.Pow(lhs.G - rhs.G, 2)
                + Math.Pow(lhs.B - rhs.B, 2)
                )/255;
        }

        // TODO: Использовать параметры в целях декомпозиции
        // Использует глобальный pixels
        /// <summary>
        /// Создает Bitmap на основе текущего значения внешней матрицы pixels
        /// </summary>
        private Bitmap GenerateBitmap(int width, int height)
        {            
            var pixelBytes = new byte[width*height * 4];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    PixelColor pc = pixels[x, y];
                    pixelBytes[(y * width + x) * 4 + 0]=pc.B;
                    pixelBytes[(y * width + x) * 4 + 1]=pc.G;
                    pixelBytes[(y * width + x) * 4 + 2]=pc.R;
                    pixelBytes[(y * width + x) * 4 + 3]=pc.A;
                }
            }
            var handle = GCHandle.Alloc(pixelBytes, GCHandleType.Pinned);
            IntPtr ptr = handle.AddrOfPinnedObject();
            Bitmap res = new Bitmap(
                width, height, wBitmap.PixelWidth*4,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb, handle.AddrOfPinnedObject());
            handle.Free();
            return res;
        }

        // Осуществляет загрузку и вывод на экран изображения
        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDial = new OpenFileDialog();
            fileDial.DefaultExt = "png";
            bool? res = fileDial.ShowDialog();
            if (!res.Value)
            {
                MessageBox.Show("Couldn't open the selected File");
                e.Handled = true;
            }
            else
            {
                wBitmap = new BitmapImage(new Uri(fileDial.FileName, UriKind.Absolute));
                ImagePreview.Source = wBitmap;
                int width = wBitmap.PixelWidth + 1;
                int height = wBitmap.PixelHeight+ 1;
                if (origpixels != null)
                {
                    Array.Clear(origpixels, 0, origpixels.Length);
                }
                origpixels = new PixelColor[width, height];
                pixels = new PixelColor[width, height];
                this.CopyPixels(wBitmap, origpixels, wBitmap.PixelWidth * 4, 0);
            }

        }
        /// <summary>
        /// Осуществляет кеинг изображения
        /// </summary>
        /// <param name="keyColor">Цвет кеинга</param>
        /// <param name="precision">Нижняя граница непрозрачности переднего плана.</param>
        /// <param name="blur_opacity">Дополнтиельная прозрачность смешанных пикселей</param>
        public void RemoveKeyColor(PixelColor keyColor, float precision, float blur_opacity)
        {
            
            int width = wBitmap.PixelWidth + 1;
            int height = wBitmap.PixelHeight + 1;

            // Нормировка
            float rK = keyColor.R / 255f;
            float gK = keyColor.G / 255f;
            float bK = keyColor.B / 255f;
            
            // Автоматический подсчет k1 
            if (AutoChooseK1Box.IsChecked == true) k1 = 1 / (gK - k2 * bK);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (pixels[x, y].A == 0) continue;
                    
                    PixelColor curColor = pixels[x, y];
                    float r = curColor.R / 255f;
                    float g = curColor.G / 255f;
                    float b = curColor.B / 255f;

                    //Аппроксимация непрозрачности цвета переднего плана (Влахос)
                    float alpha = 1 - k1 * (Math.Min(g, gK) - k2 * (k3 * b + (1 - k3) * r));

                    // Цвет переднего плана (R*alpha, G*alpha, B*alpha)
                    float rT = (r - (1 - alpha) * rK);;
                    float gT = (g - (1 - alpha) * gK);
                    float bT = (b - (1 - alpha) * bK);

                    // Кеинг
                    if (alpha < 0.1f)
                    {
                        pixels[x, y].A = 0;
                    }
                    // Стараемся сохранить, но сделать менее видимыми элементы, лежащие на границе объекта с фоном
                    // Тем не менее, для достижения максимального эффекта нужен блюр
                    else if (alpha < precision)
                    {
                        pixels[x, y].R = (byte)(rT * 255);
                        pixels[x, y].G = (byte)(gT * 255);
                        pixels[x, y].B = (byte)(bT * 255);
                        pixels[x, y].A = (byte)(255*alpha*(1f-blur_opacity));
                    }
                    // Объекты переднего плана
                    else
                        pixels[x, y].A = 255;  
                }
            }
             
        }
        private void CopyMatrix(in PixelColor[,] source, PixelColor[,] dest)
        {
            for (int i = 0; i < source.GetLength(0); i++)
            {
                for (int j = 0; j < source.GetLength(1); j++)
                {
                    dest[i, j] = source[i, j];
                }
            }
        }
        /// <summary>
        /// Обработка и вывод изображения
        /// </summary>
        private void Filtering()
        {
            // Сохраняем исходные данные для дальнейшего изменения параметров
            CopyMatrix(origpixels, pixels);
            // Фиксируем параметры 
            k1 = (float)k1Slider.Value;
            k2 = (float)k2Slider.Value; 
            k3 = (float)k3Slider.Value;
            float precision = (float)PrecisionSlider.Value / 100f;
            float blur_opacity = (float)BlurSlider.Value /100f;
            // Запуск кеинга
            RemoveKeyColor(selectedKeyColor, precision, blur_opacity);

            // Генерация результирующего изображения
            int width = wBitmap.PixelWidth;
            int height = wBitmap.PixelHeight;
            Bitmap bm = GenerateBitmap(width, height);
            // Вывод изображения на экран
            var source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bm.GetHbitmap(),
                IntPtr.Zero, System.Windows.Int32Rect.Empty,
                BitmapSizeOptions.FromWidthAndHeight(width, height));
            ImagePreview.Source = source;
            bm.Dispose();
        }
        private void ProcessButton_Click(object sender, RoutedEventArgs e)
        {
            Filtering();
        }

        // Сохранение файла (автоматическое для MVP версии)
        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            using (var fs = new FileStream("processed.png", FileMode.Create))
            {
                BitmapEncoder bmenc = new PngBitmapEncoder();
                bmenc.Frames.Add(BitmapFrame.Create(ImagePreview.Source as BitmapSource));
                bmenc.Save(fs);

            }
            MessageBox.Show("Сохранен в файле: processed.png");
        }

        // Выбор цвета 
        private void ImagePreview_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var p = e.GetPosition(ImagePreview);
            StringBuilder sb = new StringBuilder();
            try
            {
                selectedKeyColor = origpixels[(int)p.X, (int)p.Y];
                var keyColor = System.Windows.Media.Color.FromArgb(selectedKeyColor.A, selectedKeyColor.R, selectedKeyColor.G, selectedKeyColor.B);
                ColorShowcase.Fill = new SolidColorBrush(keyColor);
            }
            // Ошибка появляется при неверном отображении изображении на экране (ошибка загрузки)
            catch (Exception ex)
            {
                MessageBox.Show("Возникла ошибка при определении пикселя.");
            }
        }
        
    }
}
