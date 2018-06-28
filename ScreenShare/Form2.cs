using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScreenShare
{
    public partial class Form2 : Form
    {
        private int _Fps = 60;
        private TcpClient client = new TcpClient();
        private MemoryStream bufferNetwork = new MemoryStream();

        private byte[] Boundary
        {
            get
            {
                return ToBytes("--boundary--");
            }
        }
        public int Interval
        {
            get => Convert.ToInt32(1000 / Fps);
        }
        public int Fps
        {
            get => _Fps;
            set
            {
                _Fps = value;
                tmrScreen.Interval = Interval;
            }
        }

        public Form2()
        {
            InitializeComponent();
            tmrScreen.Interval = Interval;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (client.Connected && client.Client.Available > 0)
            {
                var buffer = new byte[client.Client.Available];

                client.Client.Receive(buffer);

                WriteToBuffer(buffer);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        public void Connect(IPEndPoint end)
        {
            try
            {
                client.ReceiveBufferSize = int.MaxValue;
                client.Connect(end);
                tmrReceive.Enabled = true;
                tmrScreen.Enabled = true;
            } catch
            {
                throw;
            }
        }

        public void Disconnect()
        {
            tmrReceive.Enabled = false;
            tmrScreen.Enabled = false;

            if (client.Connected)
            {
                client.Close();
                client.Dispose();
            }
        }
        
        private void timer2_Tick(object sender, EventArgs e)
        {
            var arr = bufferNetwork.ToArray();
            var bytePos = IndexOf(arr, Boundary);

            if (bytePos > -1)
            {
                var takenByte = bytePos + Boundary.Length;
                var arr_byte = ReadFromBuffer(takenByte);

                TryShow(arr_byte);
            }
        }

        private void TryShow(byte[] arr)
        {
            using (var stream = new MemoryStream())
            {
                try
                {
                    stream.Write(arr, 0, arr.Length - Boundary.Length - 1);

                    using (var bitmap = Image.FromStream(stream))
                    using (var gr = pictureBox1.CreateGraphics())
                    {
                        gr.CompositingMode = CompositingMode.SourceCopy;
                        gr.CompositingQuality = CompositingQuality.HighSpeed;
                        gr.SmoothingMode = SmoothingMode.HighSpeed;
                        gr.DrawImage(bitmap, 0, 0, pictureBox1.Width, pictureBox1.Height);
                    }
                } catch (Exception ex)
                {
                    //
                }
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

        private byte[] ToBytes(string str)
        {
            return Encoding.ASCII.GetBytes(str);
        }

        public int IndexOf(byte[] arrayToSearchThrough, byte[] patternToFind)
        {
            if (patternToFind.Length > arrayToSearchThrough.Length)
                return -1;
            for (int i = 0; i < arrayToSearchThrough.Length - patternToFind.Length; i++)
            {
                bool found = true;
                for (int j = 0; j < patternToFind.Length; j++)
                {
                    if (arrayToSearchThrough[i + j] != patternToFind[j])
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                {
                    return i;
                }
            }
            return -1;
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            Disconnect();
            bufferNetwork.Dispose();
        }
    }
}
