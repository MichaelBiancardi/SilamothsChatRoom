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
        public bool kill = false;

        public MainForm()
        {
            InitializeComponent();
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            if (inputTextBox.Text == String.Empty)
                return;

            TcpClient client = new TcpClient("127.0.0.1", 1304);

            StreamWriter writer = new StreamWriter(client.GetStream());

            byte[] message = ASCIIEncoding.ASCII.GetBytes("0|" + inputTextBox.Text + "|" + LoginForm.Username + "|" + channelTabs.SelectedTab.Text);
            writer.BaseStream.Write(message, 0, message.Length);
            writer.Flush();

            byte[] buffer = new byte[client.Client.ReceiveBufferSize];
            client.Client.Receive(buffer);
            String response = Encoding.ASCII.GetString(buffer).TrimEnd(new char[] { '\n', '\r', default(char) });
            List<String> splitResponse = response.Split(new char[] { '|' }).ToList<string>();
            String id = splitResponse[0];

            if (id == "0")
            {
                if (splitResponse[1] != "Good")
                    MessageBox.Show("Message did not send.  Please try again and ensure you have Wi-Fi connection.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }            

            client.Close();

            inputTextBox.Text = String.Empty;
        }

        public void InsertIntoTextBox(String text, String channel)
        {
            //Used to get access in Program class

            if (text == String.Empty)
                return;

            try
            {
                RichTextBox channelBox = (RichTextBox)channelTabs.TabPages[channel].Controls[0];    //Hopefully works

                channelBox.Invoke(new Action(() => channelBox.Text += text + Environment.NewLine));

                //Gets error because form hasn't quite loaded yet because multithreading...
            }
            catch (Exception ex) { }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            kill = true;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            channelTabs.TabPages.Clear();

            foreach (String channel in LoginForm.Channels)
            {
                RichTextBox temp = new RichTextBox();
                temp.ClientSize = channelTabs.ClientSize;
                temp.ScrollBars = RichTextBoxScrollBars.Vertical;
                temp.KeyPress += KeyPress;

                channelTabs.TabPages.Add(channel, channel);

                channelTabs.TabPages[channel].Controls.Add(temp);
            }

            DataGridView people = new DataGridView();
            people.Columns.Add("name", "Name");
            people.Columns.Add("message", "Message");
            people.AllowUserToAddRows = false;
            people.ReadOnly = true;
            people.Size = new Size(3000, 582);
            people.Columns[0].Width = 582;
            channelTabs.TabPages.Add("People", "Who's Online?");
            channelTabs.TabPages["People"].Controls.Add(people);

            TabControl messages = new TabControl();
            messages.Size = new Size(3000, 582);
            channelTabs.TabPages.Add("Messages", "Messages");
            channelTabs.TabPages["Messages"].Controls.Add(messages);

            inputTextBox.Select();
        }

        private void KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void ChannelTabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (channelTabs.SelectedTab == channelTabs.TabPages["People"] && channelTabs.SelectedTab != null)
            {
                DataGridView peopleGrid = (DataGridView)channelTabs.TabPages["People"].Controls[0];
                peopleGrid.Rows.Clear();

                TcpClient client = new TcpClient("127.0.0.1", 1304);

                StreamWriter writer = new StreamWriter(client.GetStream());

                byte[] message = ASCIIEncoding.ASCII.GetBytes("4");
                writer.BaseStream.Write(message, 0, message.Length);
                writer.Flush();

                byte[] buffer = new byte[client.Client.ReceiveBufferSize];
                client.Client.Receive(buffer);
                String response = Encoding.ASCII.GetString(buffer).TrimEnd(new char[] { '\n', '\r', default(char) });
                List<String> splitResponse = response.Split(new char[] { '|' }).ToList<string>();
                String id = splitResponse[0];

                if (id == "4")
                {
                    List<String> people = splitResponse[1].Split('`').ToList<String>(); //Prevent people from typing '`' into name

                    for (int i = 0; i < people.Count; i++)
                    {
                        peopleGrid.Rows.Add();
                        String[] cells = new String[2];
                        cells[0] = people[i];
                        cells[1] = "Send A Message";
                        peopleGrid.Rows[i].SetValues(cells);
                    }
                }
            }
        }
    }
}