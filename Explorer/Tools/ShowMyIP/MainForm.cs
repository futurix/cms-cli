using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace ShowMyIP
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            string ipText = null;
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
             
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    ipText = ip.ToString();
            }

            if (!String.IsNullOrWhiteSpace(ipText))
                ipLabel.Text = ipText;
        }

        private void ipLabel_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(ipLabel.Text))
                Clipboard.SetText(ipLabel.Text);
        }
    }
}
