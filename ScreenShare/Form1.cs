using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScreenShare
{
    public partial class Form1 : Form
    {
        private int _Fps = 60;
        private long _Quality = 100L;
        private MemoryStream bufferNetwork = new MemoryStream();
        private TcpListener tcpListener;
        private List<TcpClient> clients = new List<TcpClient>();
        private Form2 form = new Form2();

        private object lockObject = new object();

        private byte[] Boundary
        {
            get
            {
                return ToBytes("--boundary--");
            }
        }
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
            var ip = new IPAddress(new byte[] { 127, 0, 0, 1 });
            var endpoint = new IPEndPoint(ip, 8333);

            tcpListener = new TcpListener(endpoint);
            tcpListener.Start();
            form.Show();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            using (var bufferImage = new MemoryStream())
            using (var bitmap = ScreenCapturePInvoke.CaptureFullScreen(true))
            {
                var encoderParam = new EncoderParameters();
                var encoder = ImageCodecInfo.GetImageEncoders().Where(x => x.FormatDescription == "JPEG").First();

                encoderParam.Param = new EncoderParameter[]
                {
                    new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, Quality)
                };

                bitmap.Save(bufferImage, encoder, encoderParam);
                WriteToBuffer(bufferImage);
                WriteToBuffer(Boundary);
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            Fps = Convert.ToInt32(numericUpDown1.Value);
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            Quality = Convert.ToInt64(numericUpDown2.Value);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            tcpListener.Stop();

            foreach (var client in clients)
            {
                client.Close();
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            TcpRoutine();
        }

        private async void TcpRoutine()
        {
            var message = ToBytes("Get MEssage Here");
            var listDisconnect = new List<int>();
            var idx = 0;

            if (tcpListener.Pending())
            {
                var client = await tcpListener.AcceptTcpClientAsync();

                clients.Add(client);
            }

            foreach (var client in clients)
            {
                if (client.Connected)
                    client.Client.Send(message);
                else
                    listDisconnect.Add(idx);

                idx++;
            }

            foreach (var i in listDisconnect)
                clients.RemoveAt(i);
        }

        private byte[] ToBytes(string str)
        {
            return Encoding.ASCII.GetBytes(str);
        }

        private void WriteToBuffer(MemoryStream stream)
        {
            var offset = 0L;
            var buffer = new byte[8192];

            stream.Seek(0, SeekOrigin.Begin);

            try
            {
                lock (bufferNetwork)
                {
                    while (offset < stream.Length)
                    {
                        var len = stream.Length;
                        var remain = len - offset;

                        if (remain > buffer.Length)
                        {
                            stream.Read(buffer, 0, buffer.Length);
                            bufferNetwork.Write(buffer, 0, buffer.Length);
                            offset += buffer.Length;
                        }
                        else
                        {
                            stream.Read(buffer, 0, Convert.ToInt32(remain));
                            bufferNetwork.Write(buffer, 0, Convert.ToInt32(remain));
                            offset += len - offset;
                        }
                    }
                }
            }
            catch
            {
                //
            }
        }

        private void WriteToBuffer(byte[] bytes)
        {
            try
            {
                lock (bufferNetwork)
                {
                    bufferNetwork.Write(bytes, 0, bytes.Length);
                }
            }
            catch
            {
                //
            }
        }

        private byte[] ReadFromBuffer(long count)
        {
            try
            {
                lock (bufferNetwork)
                {
                    var copy = bufferNetwork.ToArray();
                    var buffLen = count > copy.LongLength ? copy.LongLength : count;
                    var newLen = copy.LongLength - buffLen;
                    var ret = new byte[buffLen];

                    bufferNetwork.Seek(0, SeekOrigin.Begin);
                    bufferNetwork.Read(ret, 0, Convert.ToInt32(buffLen));
                    bufferNetwork.SetLength(newLen);

                    if (bufferNetwork.Length > 0)
                    {
                        bufferNetwork.Seek(0, SeekOrigin.Begin);
                        bufferNetwork.Write(copy, Convert.ToInt32(buffLen), Convert.ToInt32(newLen));
                    }

                    return ret;
                }
            }
            catch
            {
                //
            }

            return new byte[] { };
        }
    }
}
