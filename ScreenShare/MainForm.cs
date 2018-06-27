using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScreenShare
{
    public partial class MainForm : Form
    {
        Form1 form1;
        Form2 form2;

        public MainForm()
        {
            InitializeComponent();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            var c = (Control)sender;

            if (e.KeyData == Keys.OemPeriod)
            {
                switch (c.Name)
                {
                    case "T1":
                        T2.Focus();
                        break;
                    case "T2":
                        T3.Focus();
                        break;
                    case "T3":
                        T4.Focus();
                        break;
                }

                e.SuppressKeyPress = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                var endpoint = GetIPEndpoint();

                if (form2 == null || form2.IsDisposed)
                    form2 = new Form2();

                form2.FormClosing += Form2_FormClosing;
                form2.Connect(endpoint);
                form2.Show();

                Hide();
            } catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            form2.FormClosing -= Form2_FormClosing;
            Show();
        }

        private IPEndPoint GetIPEndpoint()
        {
            var port = Convert.ToInt32(numericUpDown1.Value);
            var bit = new byte[4];

            bit[0] = Convert.ToByte(T1.Text);
            bit[1] = Convert.ToByte(T2.Text);
            bit[2] = Convert.ToByte(T3.Text);
            bit[3] = Convert.ToByte(T4.Text);

            var ip = new IPAddress(bit);
            var end = new IPEndPoint(ip, port);

            return end;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (form1 == null || form1.IsDisposed)
                form1 = new Form1();

            form1.FormClosing += Form1_FormClosing;
            form1.Show();
            Hide();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            form1.FormClosing -= Form1_FormClosing;
            Show();
        }
    }
}
