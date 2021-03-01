using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.IO;

namespace TCPClient
{
    class Program
    {

        static void Main(string[] args)
        {
            //======================================================================
            //Server variables declaration
            //----------------------------------------------------------------------
            var server_IP = "127.0.0.1";
            var server_Port = 50000;
            NetworkStream netStream;
            BinaryReader bReader;
            BinaryWriter bWriter;
            //======================================================================

            try
            {
                //======================================================================
                //Connect and tell us that you are connected, pls!
                //----------------------------------------------------------------------
                TcpClient client = new TcpClient();
                client.Connect(server_IP, server_Port);
                Console.WriteLine(">>> Client connected to the server [" + server_IP + ":" + server_Port + "]");
                //======================================================================


                //======================================================================
                //Set up streams and make sure that you can send and receive data from the server
                //----------------------------------------------------------------------
                netStream = client.GetStream();
                bReader = new BinaryReader(netStream);
                bWriter = new BinaryWriter(netStream);

                if (netStream.CanWrite)
                {
                    Console.WriteLine("> You can send data to the server!");
                }
                if (netStream.CanRead)
                {
                    Console.WriteLine("> You can read data from the server!");
                }
                //======================================================================

                Console.WriteLine();
                Console.WriteLine();

                //======================================================================
                //Send 0 so the server knows we want to see files in a directory
                //----------------------------------------------------------------------
                Console.WriteLine("--------- ListContent call ----------");
                var directory = "F:\\visual studio\\TCP_Server_Client";
                bWriter.Write(0);
                bWriter.Write(directory);
                var text = bReader.ReadInt32();
                if(text == 0)
                {
                    Console.WriteLine(">>> No file found in the directory. Maybe it does not exist?");
                }
                while (text > 0)
                {
                    var file = bReader.ReadString();
                    Console.WriteLine(file);
                    text--;
                }
                //======================================================================

                //Just some empty space visiting us to say hello! :)
                Console.WriteLine();
                Console.WriteLine();


                //======================================================================
                //Send 1 to getfile
                //----------------------------------------------------------------------
                Console.WriteLine("--------- GetFile call ----------");
                
                var filePath = "F:\\yes.jpg";
                var newFilePath = "F:\\yes2.jpg";
                bWriter.Write(1);
                bWriter.Write(filePath);
                var chunkSize = bReader.ReadInt32();
                Console.WriteLine("Chunk size received: " + chunkSize);
                byte[] buffer = new byte[chunkSize];

               // var receivedBytes = 0;

                long fileSize = bReader.ReadInt64();
                Console.WriteLine("File size received: " + fileSize);
                using (Stream targetFile = new FileStream(newFilePath, FileMode.OpenOrCreate))
                {
                    while (fileSize > 0)
                    {
                        int receivedBytes = (int)Math.Min(fileSize, (long)chunkSize);
                        //Console.WriteLine(receivedBytes);
                        buffer = bReader.ReadBytes(receivedBytes);
                        targetFile.Write(buffer, 0, receivedBytes);
                        fileSize -= receivedBytes;
                        //Console.WriteLine(fileSize);
                    }
                }
                Console.WriteLine("File transfer complete!");
                //======================================================================

            }
            catch (Exception e)
            {
                Console.WriteLine("Exception error in main:");
                Console.WriteLine($"{e}");
            }
            
            Console.ReadKey();
        }
    }
}
