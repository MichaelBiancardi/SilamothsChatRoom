using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.IO;

namespace ChatRoomClient
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            TcpClient client = new TcpClient("127.0.0.1", 1304);

            StreamWriter writer = new StreamWriter(client.GetStream());

            //Here's where we get into weird byte ASCII encoding of the message and stuff
        }

        public void InsertIntoTextBox(String text)
        {
            //Used to get access in Program class

            if (text == String.Empty)
                return;

            outputTextBox.Text += text + Environment.NewLine;
        }
    }
}