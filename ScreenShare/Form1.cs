using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScreenShare
{
    public partial class Form1 : Form
    {
        private int _Fps = 60;
        private long _Quality = 100L;
        private MemoryStream bufferImage = new MemoryStream();
        public int Interval {
            get => Convert.ToInt32(1000 / Fps);
        }
        public int Fps
        {
            get => _Fps;
            set
            {
                _Fps = value;
                timer1.Interval = Interval;
            }
        }
        public long Quality
        {
            get => _Quality;
            set
            {
                if (value <= 100L && value > 0)
                {
                    _Quality = value;
                }
            }
        }

        public Form1()
        {
            InitializeComponent();
            timer1.Interval = Interval;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            using (var bitmap = ScreenCapturePInvoke.CaptureFullScreen(true))
            {
                var encoderParam = new EncoderParameters();
                var encoder = ImageCodecInfo.GetImageEncoders().Where(x => x.FormatDescription == "JPEG").First();

                encoderParam.Param = new EncoderParameter[]
                {
                    new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, Quality)
                };

                if (bufferImage != null)
                {
                    bufferImage.Dispose();
                    bufferImage = new MemoryStream();
                }

                bitmap.Save(bufferImage, encoder, encoderParam);
                Console.WriteLine("Image Size : " + bufferImage.Length);
            }
        }
    }
}
