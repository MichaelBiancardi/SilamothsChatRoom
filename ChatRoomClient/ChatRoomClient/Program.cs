using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Text;

namespace ChatRoomClient
{
    static class Program
    {
        static String toBeAdded = String.Empty;
        static MainForm form = new MainForm();
        static String name;

        [STAThread]
        static void Main()
        {
            LoginForm login = new LoginForm();
            login.ShowDialog();
            name = LoginForm.Username;

            //Set up multithreading

            Thread initThread = new Thread(new ThreadStart(Init));
            Thread networkThread = new Thread(new ThreadStart(CheckForStuff));

            initThread.Start();
            networkThread.Start();
        }

        static void Init()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(form);
        }

        static void CheckForStuff()
        {
            while (true)
            {
                //Check for incoming messages and such

                TcpClient client = new TcpClient("127.0.0.1", 1304);

                byte[] updateMessage = ASCIIEncoding.ASCII.GetBytes("1|" + name);
                StreamWriter writer = new StreamWriter(client.GetStream());
                writer.BaseStream.Write(updateMessage, 0, updateMessage.Length);
                writer.Flush();

                byte[] buffer = new byte[client.Client.ReceiveBufferSize];
                client.Client.Receive(buffer);
                String response = Encoding.ASCII.GetString(buffer).TrimEnd(new char[] { '\n', '\r', default(char) });
                List<String> splitResponse = response.Split(new char[] { '|' }).ToList<string>();
                String id = splitResponse[0];

                if (id == "1")
                    form.InsertIntoTextBox(splitResponse[1]);

                client.Close();

                if (form.kill)
                {
                    client = new TcpClient("127.0.0.1", 1304);

                    writer = new StreamWriter(client.GetStream());

                    byte[] message = ASCIIEncoding.ASCII.GetBytes("3|" + LoginForm.Username);
                    writer.BaseStream.Write(message, 0, message.Length);
                    writer.Flush();

                    client.Close();

                    Environment.Exit(Environment.ExitCode);
                }
            }
        }

        delegate void Del();
    }
}
