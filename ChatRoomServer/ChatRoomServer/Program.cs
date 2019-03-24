using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;

namespace ChatRoomServer
{
    struct Message
    {
        public String Contents { get; set; }
        public String Sender { get; set; }
        public List<String> Received { get; set; }

        public Message(String contents)
        {
            Received = new List<string>();
            Sender = String.Empty;
            Contents = contents;
        }
    }

    class Program
    {
        //static List<String> messagesToSend = new List<String>();
        static List<String> clientNames = new List<string>();
        static List<Message> messagesToSend = new List<Message>();

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
                                List<Message> newMessageList = messagesToSend;

                                for (int i = 0; i < messagesToSend.Count; i++)
                                {
                                    if (!messagesToSend[i].Received.Contains(splitRequest[1]))
                                    {
                                        String responseString = "1|" + messagesToSend[i].Contents;
                                        byte[] response = Encoding.ASCII.GetBytes(responseString);
                                        writer.BaseStream.Write(response, 0, response.Length);
                                        writer.Flush();
                                        Console.WriteLine("A message has been sent out.");

                                        List<String> newReceived = messagesToSend[i].Received;
                                        newReceived.Add(splitRequest[1]);
                                        Message newMessage = messagesToSend[i];
                                        newMessage.Received = newReceived;
                                        newMessageList[i] = newMessage;
                                    }
                                }
                            }
                            break;
                        case "2":
                            String submittedName = splitRequest[1];
                            if (!clientNames.Contains(submittedName))
                            {
                                String responseString = "2|Yes";
                                byte[] response = Encoding.ASCII.GetBytes(responseString);
                                writer.BaseStream.Write(response, 0, response.Length);
                                writer.Flush();
                            }
                            else
                            {
                                String responseString = "2|No";
                                byte[] response = Encoding.ASCII.GetBytes(responseString);
                                writer.BaseStream.Write(response, 0, response.Length);
                                writer.Flush();
                            }
                            break;
                    }

                    foreach (String message in newMessages)
                    {
                        Message newMessage = new Message(message);

                        messagesToSend.Add(newMessage);
                    }

                    client.Close();
                }
            }
            catch (Exception ex) { }
        }
    }
}