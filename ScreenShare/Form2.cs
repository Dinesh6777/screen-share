using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
        TcpClient client = new TcpClient();

        public Form2()
        {
            InitializeComponent();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (client.Connected && client.Client.Available > 0)
            {
                var buffer = new byte[client.Client.Available];

                client.Client.Receive(buffer);

                var message = ByteToString(buffer);
                Console.WriteLine(message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Connect();
        }

        private async void Connect()
        {
            var ip = new IPAddress(new byte[] { 127, 0, 0, 1 });
            var end = new IPEndPoint(ip, 8333);
            client.Connect(end);
        }

        private string ByteToString(byte[] arr)
        {
            return Encoding.ASCII.GetString(arr);
        }
    }
}
