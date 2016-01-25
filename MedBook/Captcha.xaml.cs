
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MedBook
{
    /// <summary>
    /// Interaction logic for Captcha.xaml
    /// </summary>
    public partial class Captcha : Window
    {
        public string TextResult { get; set; }
        private bool _dialog;
        public Captcha(byte[] imgBytes)
        {
            InitializeComponent();
            Img.Source = ByteArrayToImage(imgBytes);
        }

        private static ImageSource ByteArrayToImage(byte[] byteArrayIn)
        {
            if (byteArrayIn == null) return null;

            var stream = new MemoryStream(byteArrayIn);
            var image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = stream;
            image.EndInit();
            return image;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            TextResult = TextBox.Text;
            _dialog = true;
            Close();
        }

        private void Captcha_OnClosing(object sender, CancelEventArgs e)
        {
            DialogResult = _dialog;
        }
    }
}