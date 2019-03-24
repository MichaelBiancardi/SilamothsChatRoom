using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;

namespace ChatRoomServer
{
    class Program
    {
        static List<String> messagesToSend = new List<String>();

        static void Main(string[] args)
        {
            TcpListener listener = new TcpListener(1304);
            listener.Start();

            try
            {
                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    StreamWriter writer = new StreamWriter(client.GetStream());

                    byte[] buffer = new byte[client.Client.ReceiveBufferSize];
                    client.Client.Receive(buffer);
                    String request = Encoding.ASCII.GetString(buffer).TrimEnd(new char[] { '\n', '\r', '\0' });
                    List<String> splitRequest = request.Split('|').ToList<String>();

                    String id = splitRequest[0];

                    List<String> newMessages = new List<String>();

                    switch (id)
                    {
                        case "0":
                            //A message has been sent by a client
                            newMessages.Add(splitRequest[1]);
                            Console.WriteLine("A message has been received: " + splitRequest[1]);
                            break;
                        case "1":
                            //Searching for new messages

                            if (messagesToSend.Count == 0)
                            {
                                String responseString = "2";
                                byte[] response = Encoding.ASCII.GetBytes(responseString);
                                writer.BaseStream.Write(response, 0, response.Length);
                                writer.Flush();
                            }
                            else
                            {
                                foreach (String message in messagesToSend)
                                {
                                    String responseString = "1|" + message;
                                    byte[] response = Encoding.ASCII.GetBytes(responseString);
                                    writer.BaseStream.Write(response, 0, response.Length);
                                    writer.Flush();
                                    Console.WriteLine("A message has been sent out.");
                                }
                            }
                            break;
                    }

                    foreach (String message in newMessages)
                    {
                        messagesToSend.Add(message);
                    }

                    client.Close();
                }
            }
            catch (Exception ex) { }
        }
    }
}