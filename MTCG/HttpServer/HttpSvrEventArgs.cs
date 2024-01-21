using System;
using System.Net.Sockets;
using System.Text;



namespace MTCG.HttpServer
{
    /// <summary>This class provides HTTP server event arguments.</summary>
    public class HttpSvrEventArgs : EventArgs
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // protected members                                                                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>TCP client.</summary>
        protected TcpClient _Client;



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // constructors                                                                                                     //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Creates a new instance of this class.</summary>
        /// <param name="client">TCP client object.</param>
        /// <param name="plainMessage">HTTP plain message.</param>
        /// 
        public HttpSvrEventArgs(TcpClient client)
        {
            Console.WriteLine("in http überladeneme kontruktor");
            _Client = client;
            PlainMessage = string.Empty;
            Payload = string.Empty;
            Headers = new HttpHeader[0];
            Client = client;

        }
        public HttpSvrEventArgs(TcpClient client, string plainMessage)
        {
            _Client = client;
            Client = client;
            PlainMessage = plainMessage;
            Payload = string.Empty;

            string[] lines = plainMessage.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
            bool inheaders = true;
            List<HttpHeader> headers = new();

            for (int i = 0; i < lines.Length; i++)
            {
                if (i == 0)
                {
                    string[] inc = lines[0].Split(' ');
                    Method = inc[0];
                    Path = inc[1];
                }
                else if (inheaders)
                {
                    if (string.IsNullOrWhiteSpace(lines[i]))
                    {
                        inheaders = false;
                    }
                    else { headers.Add(new HttpHeader(lines[i])); }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(Payload)) { Payload += "\r\n"; }
                    Payload += lines[i];
                }
            }

            Headers = headers.ToArray();

        }



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties                                                                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Gets the plain HTTP message.</summary>
        public string PlainMessage
        {
            get; protected set;
        }


        /// <summary>Gets the HTTP method.</summary>
        public virtual string Method
        {
            get; protected set;
        } = string.Empty;


        /// <summary>Gets the request path.</summary>
        public virtual string Path
        {
            get; protected set;
        } = string.Empty;


        /// <summary>Gets the HTTP headers.</summary>
        public virtual HttpHeader[] Headers
        {
            get; protected set;
        }


        /// <summary>Gets the HTTP payload.</summary>
        public virtual string Payload
        {
            get; protected set;
        }

        /// <summary>Gets the TCP client.</summary>
        public virtual TcpClient Client
        {
            get; protected set;
        }



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public methods                                                                                                   //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Returns a reply to the HTTP request.</summary>
        /// <param name="status">Status code.</param>
        /// <param name="payload">Payload.</param>
        public virtual void Reply(int status, string? payload = null)
        {
            string statusDescription;
            Console.WriteLine("in repyl with" + status);
            Console.WriteLine("with payload:" + payload);

            switch (status)
            {
                case 200:
                    statusDescription = "HTTP/1.1 200 OK\r\n"; break;
                case 201:
                    statusDescription = "HTTP/1.1 201 Created\r\n"; break;
                case 204:
                    statusDescription = "HTTP/1.1 204 No Content\r\n"; break;
                case 400:
                    statusDescription = "HTTP/1.1 400 Bad Request\r\n"; break;
                case 401:
                    statusDescription = "HTTP/1.1 401 Unauthorized\r\n"; break;
                case 403:
                    statusDescription = "HTTP/1.1 403 Forbidden\r\n"; break;
                case 404:
                    statusDescription = "HTTP/1.1 404 Not Found\r\n"; break;
                case 405:
                    statusDescription = "HTTP/1.1 405 Method Not Allowed\r\n"; break;
                case 409:
                    statusDescription = "HTTP/1.1 409 Conflict\r\n"; break;
                case 500:
                    statusDescription = "HTTP/1.1 500 Internal Server Error\r\n"; break;
                default:
                    statusDescription = "HTTP/1.1 418 I'm a Teapot\r\n"; break;
            }

            string headers = "Content-Type: application/json\r\n";
            if (!string.IsNullOrEmpty(payload))
            {
                int contentLength = Encoding.UTF8.GetByteCount(payload);
                headers += $"Content-Length: {contentLength}\r\n";
            }

            string fullResponse = statusDescription + headers + "\r\n" + payload + "\r\n\n";

            Console.WriteLine("Full response: " + fullResponse + "\n\n\n------");

            try
            {
                byte[] responseBytes = Encoding.UTF8.GetBytes(fullResponse);
                NetworkStream stream = _Client.GetStream();
                stream.Write(responseBytes, 0, responseBytes.Length);
                stream.Flush();
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine("Client already disposed!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending response: {ex.Message}");
            }
            finally
            {
                _Client.Close();
                _Client.Dispose();
            }


        }


    }
}
