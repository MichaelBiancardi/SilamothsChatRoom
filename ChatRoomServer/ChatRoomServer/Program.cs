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
        public String Channel { get; set; }

        public Message(String contents, String channel, String sender)
        {
            Received = new List<String>();
            Sender = sender;
            Contents = contents;
            Channel = channel;
        }
    }

    class Program
    {
        static List<String> clientNames = new List<String>();
        static List<Message> messagesToSend = new List<Message>();
        static List<String> channels = new List<String>();

        static void Main(string[] args)
        {
            try
            {
                StreamReader reader = new StreamReader("channels.txt");

                while (reader.Peek() > -1)
                {
                    channels.Add(reader.ReadLine());
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }

            try
            {
                TcpListener listener = new TcpListener(1304);
                listener.Start();

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
                            Message newMessage = new Message(splitRequest[1], splitRequest[3],splitRequest[2]);
                            newMessage.Sender = splitRequest[2];
                            newMessages.Add(newMessage);

                            String responseString = "0|Good";
                            byte[] response = Encoding.ASCII.GetBytes(responseString);
                            writer.BaseStream.Write(response, 0, response.Length);
                            writer.Flush();
                            break;
                        case "1":
                            //Searching for new messages

                            if (messagesToSend.Count == 0)
                            {
                                responseString = "2";
                                response = Encoding.ASCII.GetBytes(responseString);
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
                                        responseString = "1|" + messagesToSend[i].Sender + ": " + messagesToSend[i].Contents + "|" + messagesToSend[i].Channel;
                                        response = Encoding.ASCII.GetBytes(responseString);
                                        writer.BaseStream.Write(response, 0, response.Length);
                                        writer.Flush();
                                        //Console.WriteLine("A message has been sent out.");

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
                            //Someone is joining and wants to check if a name works

                            String submittedName = splitRequest[1];
                            if (!clientNames.Contains(submittedName))
                            {
                                String channelsString = String.Empty;

                                foreach (String channel in channels)
                                {
                                    channelsString += channel + ",";
                                }

                                channelsString = channelsString.TrimEnd(new char[] { '\n', '\r', '\0', ',' });

                                responseString = "2|Yes|" + channelsString;
                                response = Encoding.ASCII.GetBytes(responseString);
                                writer.BaseStream.Write(response, 0, response.Length);
                                writer.Flush();
                                clientNames.Add(submittedName);

                                Message notifyJoined = new Message(submittedName + " has joined the chat room!", "Temp", "Admin");
                                newMessages.Add(notifyJoined);
                            }
                            else
                            {
                                responseString = "2|No";
                                response = Encoding.ASCII.GetBytes(responseString);
                                writer.BaseStream.Write(response, 0, response.Length);
                                writer.Flush();
                            }
                            break;

                        case "3":
                            //A user is logging off

                            String clientName = splitRequest[1];

                            Message notifyLeft = new Message(clientName + " has left the chat room!", "Temp", "Admin");
                            newMessages.Add(notifyLeft);

                            clientNames.Remove(clientName);
                            break;
                        case "4":
                            //Requesting list of people currently online

                            responseString = "4|";

                            foreach (String name in clientNames)
                            {
                                responseString += name + "`";
                            }

                            responseString = responseString.TrimEnd(new char[] { '`' });

                            response = Encoding.ASCII.GetBytes(responseString);
                            writer.BaseStream.Write(response, 0, response.Length);
                            writer.Flush();
                            break;
                    }

                    foreach (Message message in newMessages)
                    {
                        messagesToSend.Add(message);
                    }

                    client.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }
        }
    }
}