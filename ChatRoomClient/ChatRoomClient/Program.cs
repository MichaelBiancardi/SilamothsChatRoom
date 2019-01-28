using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace ChatRoomClient
{
    static class Program
    {
        static String toBeAdded = String.Empty;
        static MainForm form = new MainForm();

        [STAThread]
        static void Main()
        {
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
            //Check for incoming messages and such

            TcpListener listener = new TcpListener(1304);

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();

                StreamReader reader = new StreamReader(client.GetStream());
                StreamWriter writer = new StreamWriter(client.GetStream());

                try
                {
                    String[] tokens = reader.ReadLine().Split('|');

                    String id = tokens[0];

                    switch (id)
                    {
                        case "0":
                            //Someone has sent a chat message

                            toBeAdded = tokens[1] + ": " + tokens[2];   //Store name of user plus their message

                            form.InsertIntoTextBox(toBeAdded);  //For some reason I did this a different way in Web Server...I'll have to test if that was necessary
                            break;
                    }
                }
                catch { }
            }
        }

        delegate void Del();
    }
}
