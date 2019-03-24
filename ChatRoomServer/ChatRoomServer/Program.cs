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
        static List<String> clientNames = new List<String>();
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

                    List<Message> newMessages = new List<Message>();

                    switch (id)
                    {
                        case "0":
                            //A message has been sent by a client
                            Message newMessage = new Message(splitRequest[1]);
                            newMessage.Sender = splitRequest[2];
                            newMessages.Add(newMessage);
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
                                        String responseString = "1|" + messagesToSend[i].Sender + ": " + messagesToSend[i].Contents;
                                        byte[] response = Encoding.ASCII.GetBytes(responseString);
                                        writer.BaseStream.Write(response, 0, response.Length);
                                        writer.Flush();
                                        Console.WriteLine("A message has been sent out.");

                                        List<String> newReceived = messagesToSend[i].Received;
                                        newReceived.Add(splitRequest[1]);
                                        Message newM = messagesToSend[i];
                                        newM.Received = newReceived;
                                        newMessageList[i] = newM;
                                    }                                    
                                }

                                messagesToSend = newMessageList;

                                for (int i = 0; i < messagesToSend.Count; i++)
                                {
                                    bool canBeRemoved = true;

                                    foreach (String name in clientNames)
                                    {
                                        if (!messagesToSend[i].Received.Contains(name))
                                            canBeRemoved = false;
                                    }

                                    if (canBeRemoved)
                                    {
                                        messagesToSend.RemoveAt(i);
                                        i--;
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
                                clientNames.Add(submittedName);
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

                    foreach (Message message in newMessages)
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