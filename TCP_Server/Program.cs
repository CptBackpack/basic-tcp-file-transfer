using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

/*
 *  Homework: GetFile & Client
 *  Read blocks, write in on the string, flush, rinse & repeat
 *  Teacher email flavius.stoichescu@microsoft.com
 * DONT SNED MEMES
*/
namespace cicaCevaTCPIPStuff
{
    enum RequestType
    {
        ListContent = 0,
        GetFile
    }

    class Program
    {
        static string GetFileMethod(String path)
        {
            string fileContent = "";
            try
            {
                fileContent = File.ReadAllText(@path);
            }
            catch (Exception e)
            {
                return ">>> There was an error opening the file. The given path does not exist or the file cannot be accessed!";
            }

            return fileContent;
        }
        static List<string> ListFolderContent(String path)
        {
            try
            {
                var result = new List<string>();
                result.AddRange(Directory.GetFiles(path));
                foreach (var dir in Directory.GetFiles(path))
                {
                    result.AddRange(ListFolderContent(dir));

                }
                return result;
            }
            catch (Exception e)
            {
                return new List<string>();
            }
        }

        static void ClientWorkerThreadMethod(object c)
        {
            try
            {
                TcpClient client = (TcpClient)c;
                Console.WriteLine("> Connection Received!");
                var clientStream = client.GetStream();
                //BinaryReader streamReader = new BinaryReader(clientStream);
                //BinaryWriter streamWriter = new BinaryWriter(clientStream);
                var streamReader = new BinaryReader(clientStream);
                var streamWriter = new BinaryWriter(clientStream);

                var reqType = RequestType.ListContent;
                while (client.Connected)
                {
                    reqType = (RequestType)streamReader.ReadInt32();
                    switch (reqType)
                    {
                        case RequestType.ListContent:
                            Console.WriteLine();
                            Console.WriteLine();
                            Console.WriteLine("> ListContent RequestType received!");
                            var path = streamReader.ReadString();
                            var files = ListFolderContent(path);
                            streamWriter.Write(files.Count);
                            streamWriter.Flush();
                            foreach (var file in files)
                            {
                               
                                streamWriter.Write(file);
                            }
                            streamWriter.Flush();
                            break;
                         case RequestType.GetFile:
                            Console.WriteLine();
                            Console.WriteLine();
                            Console.WriteLine("> GetFile RequestType received!");
                            var filePath = streamReader.ReadString();
                            
                            const int chunkSize = 1024; //Read a file in chunks of 1KB
                           
                            using (var fileContent = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                            {
                                
                                var readBytes = 0;  
                                streamWriter.Write(chunkSize);
                                var fileSize = fileContent.Length;
                                streamWriter.Write(fileSize);
                                streamWriter.Flush();
                                while (true)
                                {
                                    var buffer = new byte[chunkSize];
                                    readBytes = fileContent.Read(buffer, 0, buffer.Length);
                                    
                                    if (readBytes == 0)
                                    {
                                        streamWriter.Write(0);
                                        break;
                                    }
                                    streamWriter.Write(buffer);
                                    
                                }
                            }
                            Console.WriteLine("> Sending file done!");
                              
                            streamWriter.Flush();
                            break;
                        default:
                            break;
                    }
                }
                //streamWriter.Write("go away");
                //streamWriter.Flush();
                //var message = streamReader.ReadString();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in thread");
                Console.WriteLine($"{e}");
            }
        }

        static void Main(string[] args)
        {
            try
            {
                TcpListener server = new TcpListener(IPAddress.Any, 50000);
                server.Start();
                Console.WriteLine(">>> Server Started!");
                while (server.Server.IsBound)
                {
                    var client = server.AcceptTcpClient();
                    Thread clientWorker = new Thread(ClientWorkerThreadMethod);
                    clientWorker.Start(client);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in main.");
                Console.WriteLine($"{e}");
            }
        }
    }
}
