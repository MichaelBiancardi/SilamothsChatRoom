using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net.Sockets;

namespace ChatRoomClient
{
    public partial class LoginForm : Form
    {
        static String name;
        static List<String> channels;

        public LoginForm()
        {
            InitializeComponent();
        }

        private void goButton_Click(object sender, EventArgs e)
        {
            if (nameTextBox.Text == String.Empty)
            {
                MessageBox.Show("Please enter a name.", "Error", MessageBoxButtons.OK);
                return;
            }

            TcpClient client = new TcpClient("127.0.0.1", 1304);

            StreamWriter writer = new StreamWriter(client.GetStream());

            byte[] message = ASCIIEncoding.ASCII.GetBytes("2|" + nameTextBox.Text);
            writer.BaseStream.Write(message, 0, message.Length);
            writer.Flush();

            byte[] buffer = new byte[client.Client.ReceiveBufferSize];
            client.Client.Receive(buffer);
            String response = Encoding.ASCII.GetString(buffer).TrimEnd(new char[] { '\n', '\r', default(char) });
            List<String> splitResponse = response.Split(new char[] { '|' }).ToList<string>();
            String id = splitResponse[0];

            if (id == "2")
            {
                if (splitResponse[1] == "Yes")
                {
                    MessageBox.Show("Have fun in Silamoth's Chat Room!", "Success", MessageBoxButtons.OK);
                    name = nameTextBox.Text;

                    channels = splitResponse[2].Split(',').ToList<String>();    //Get list of channels

                    Close();
                }
                else
                {
                    MessageBox.Show("That name is already taken.  Please enter a new name.", "Error", MessageBoxButtons.OK);
                    return;
                }
            }
        }

        public static String Username
        {
            get { return name; }
        }

        public static List<String> Channels
        {
            get { return channels; }
        }

        private void LoginForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (name == null)
                Environment.Exit(Environment.ExitCode);
        }

        private void NameTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '`')
                e.Handled = true;
        }
    }
}
