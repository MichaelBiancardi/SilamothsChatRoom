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

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                StreamWriter writer = new StreamWriter(client.GetStream());

                byte[] buffer = new byte[client.Client.ReceiveBufferSize];
                client.Client.Receive(buffer);
                String request = Encoding.ASCII.GetString(buffer).TrimEnd(new char[] { '\n', '\r', '\0' });
                List<String> splitRequest = request.Split('|').ToList<String>();

                String id = splitRequest[0];

                List<String> newMessages = new List<String>();  //Ensure no message gets sent twice???
                //Definitely need to make sure everyone gets each message exactly once

                switch (id)
                {
                    case "0":
                        //A message has been sent
                        newMessages.Add(splitRequest[1]);
                        break;
                    case "1":
                        //Searching for new messages
                        foreach (String message in messagesToSend)
                        {
                            String responseString = "1 " + message;
                            byte[] response = Encoding.ASCII.GetBytes(responseString);
                            writer.BaseStream.Write(response, 0, response.Length);
                            writer.Flush();
                        }
                        break;
                }

                messagesToSend = newMessages;
            }
        }
    }
}