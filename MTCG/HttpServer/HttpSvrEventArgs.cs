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
        public HttpSvrEventArgs(TcpClient client, string plainMessage)
        {
            _Client = client;
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
            Console.WriteLine("Die header: " + Headers);
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



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public methods                                                                                                   //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Returns a reply to the HTTP request.</summary>
        /// <param name="status">Status code.</param>
        /// <param name="payload">Payload.</param>
        public virtual void Reply(int status, string? payload = null)
        {
            string statusDescription;

            switch (status)
            {
                case 200:
                    statusDescription = "HTTP/1.1 200 OK\n"; break;
                case 400:
                    statusDescription = "HTTP/1.1 400 Bad Request\n"; break;
                case 404:
                    statusDescription = "HTTP/1.1 404 Not Found\n"; break;
                case 500:
                    statusDescription = "HTTP/1.1 500 Internal Server Error\n"; break;
                default:
                    statusDescription = "HTTP/1.1 418 I'm a Teapot\n"; break; 
            }

            if (!string.IsNullOrEmpty(payload)) { statusDescription += payload + "\n\r"; }

            string statusLine = $"HTTP/1.1 {status} {statusDescription}\r\n";
            string headers = "Content-Type: application/json\r\n" +
                             $"Content-Length: {{string.IsNullOrEmpty(payload)? 0 :Encoding.UTF8.GetByteCount(payload)}}\r\n";

            string fullResponse = statusLine + headers;

            byte[] responseBytes = Encoding.UTF8.GetBytes(fullResponse);

            NetworkStream stream = _Client.GetStream();
            try
            {
                stream.Write(responseBytes, 0, responseBytes.Length);
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine("client already disposed!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending response: {ex.Message}");
            }
            _Client.Close();
            _Client.Dispose();
        }


    }
}
