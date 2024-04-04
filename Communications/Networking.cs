using Communications;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;
using System.Text;

/// <summary>
/// Author:    Shu Chen
/// Partner:   Ping-Hsun Hsieh
/// Date:      25/3/2024
/// Course:    CS 3500, University of Utah, School of Computing
/// Copyright: CS 3500 and Shu Chen - This work may not
///            be copied for use in Academic Coursework.
///
/// I, Shu Chen, certify that I wrote this code from scratch and
/// did not copy it in part or whole from another source.  All
/// references used in the completion of the assignments are cited
/// in my README file.
///
/// File Contents
/// This is a class that can do basic function of TCP networking, It can be used by other
/// classes that need networking function. It can both used by server and client
/// </summary>
namespace NetworkingLibrary
{
    public class Networking : INetworking
    {
        private string id, remoteAddressPort, localAddressPort;                                 //Record the name, local & remote port
        private bool isConnected, isWaitingForClients;                                          //Show the status of current object
        private TcpClient tcpClient;                                                            //The core of the client
        private TcpListener tcpListener;                                                        // the core of the server
        private CancellationTokenSource ctsForWaitingClient = new CancellationTokenSource();    // A token to track if we want to stop waiting for client
        private CancellationTokenSource ctsForWaitingMessage = new CancellationTokenSource();   // A token to track if we want to stop waiting for message
        private string clientOrServer;                                                          //Used when logging, tell us the local is server or client
        private readonly ILogger _logger;                                                       //A custom logger class
        private readonly ReportConnectionEstablished _reportConnected;                          //Delegate method that will called when connected
        private readonly ReportDisconnect _reportDisconnected;                                  //Delegate method that will called when disconnected
        private readonly ReportMessageArrived _reportMessage;                                   //Delegate method that will called when received message

        /// <summary>
        /// The constructor of Networking, When Created, we assume this object
        /// is client side until WaitingClient() is called
        /// </summary>
        /// <param name="logger">the logger to log the info</param>
        /// <param name="onConnect">delegate that will be called each time when connected</param>
        /// <param name="onDisconnect">delegate that will be called when connection is closed</param>
        /// <param name="onMessage">delegate that will be called when a message received</param>
        public Networking(ILogger logger, ReportConnectionEstablished onConnect,
                            ReportDisconnect onDisconnect, ReportMessageArrived onMessage)
        {
            clientOrServer = " -client- ";
            _reportDisconnected = onDisconnect;
            _logger = logger;
            _reportConnected = onConnect;
            _reportMessage = onMessage;
            isConnected = false;
        }

        /// <summary>
        /// The constructor of Networking, Only used by the server when a client trying
        /// to connect it. Server can send message to client through these networking
        /// objects.
        /// </summary>
        /// <param name="logger">the logger to log the info</param>
        /// <param name="onConnect">delegate that will be called each time when connected</param>
        /// <param name="onDisconnect">delegate that will be called when connection is closed</param>
        /// <param name="onMessage">delegate that will be called when a message received</param>
        /// <param name="tcpClient">TCP client object returned from the server's TCP listener.</param>
        public Networking(ILogger logger, ReportConnectionEstablished onConnect,
                    ReportDisconnect onDisconnect, ReportMessageArrived onMessage, TcpClient tcpClient)
                    : this(logger, onConnect, onDisconnect, onMessage)

        {
            clientOrServer = " -server- ";
            this.tcpClient = tcpClient;
            this.isConnected = true;
            this.remoteAddressPort = tcpClient.Client.RemoteEndPoint.ToString();
            this.localAddressPort = tcpClient.Client.LocalEndPoint.ToString();
        }

        /// <summary>
        ///   <para>
        ///     A Unique identifier for the entity on the "other end" of the wire.
        ///   </para>
        ///   <para>
        ///     The default ID is the tcp client's remote end point, but you can change it
        ///     if desired, to something like: "Jim"  (for a servers connection to the Jim client)
        ///   </para>
        /// </summary>
        public string ID { get => id; set => id = value; }

        /// <summary>
        ///   True if there is an active connection.
        /// </summary>
        public bool IsConnected => isConnected;

        /// <summary>
        ///   <remark>
        ///     Only useful for server type programs.
        ///   </remark>
        ///
        ///   <para>
        ///     Used by server type programs which have a port open listening
        ///     for clients to connect.
        ///   </para>
        ///   <para>
        ///     True if the connect loop is active.
        ///   </para>
        /// </summary>
        public bool IsWaitingForClients => isWaitingForClients;

        /// <summary>
        ///   <para>
        ///     When connected, return the address/port of the program we are talking to,
        ///     which is the tcpClient RemoteEndPoint.
        ///   </para>
        ///   <para>
        ///     If not connected then: "Disconnected". Note: if previously was connected, you should
        ///     return "Old Address/Port - Disconnected".
        ///   </para>
        ///   <para>
        ///     If waiting for clients (ISWaitingForClients is true)
        ///     return "Waiting For Connections on Port: {Port}".  Note: probably shouldn't call this method
        ///     if you are a server waiting on clients.... use the LocalAddressPort method.
        ///   </para>
        /// </summary>
        public string RemoteAddressPort
        {
            get
            {
                if (IsWaitingForClients) return $"Waiting for connections on port{localAddressPort}";
                if (IsConnected) return remoteAddressPort;
                if (remoteAddressPort != null) return $"{remoteAddressPort} - Disconnected";
                return "Disconnected";
            }
        }

        /// <summary>
        ///   <para>
        ///     When connected, return the address/port on this machine that we are talking on.
        ///     which is the tcpClient LocalEndPoint.
        ///   </para>
        ///   <para>
        ///     If not connected then: "Disconnected". Note: if previously was connected, you should
        ///     return "Old Address/Port - Disconnected".
        ///   </para>
        ///   <para>
        ///     If waiting for clients (ISWaitingForClients is true)
        ///     return "Waiting For Connections on Port: {Port}"
        ///   </para>
        /// </summary>
        public string LocalAddressPort
        {
            get
            {
                if (IsWaitingForClients) return $"Waiting for connections on port{localAddressPort}";
                if (IsConnected) return localAddressPort;
                if (localAddressPort != null) return $"{localAddressPort} - Disconnected";
                return "Disconnected";
            }
        }

        /// <summary>
        ///   <para>
        ///     Open a connection to the given host/port.  Returns when the connection is established,
        ///     or when an exception is thrown.
        ///   </para>
        ///   <para>
        ///     Note: Servers will not call this method.  It is used by clients connecting to
        ///     a program that is waiting for connections.
        ///   </para>
        ///   <para>
        ///     If the connection happens to already be established, this is a NOP (i.e., nothing happens).
        ///   </para>
        ///   <remark>
        ///     This method will have to create and use the low level C# TcpClient class.
        ///   </remark>
        /// </summary>
        /// <param name="host">e.g., 127.0.0.1, or "localhost", or "thebes.cs.utah.edu"</param>
        /// <param name="port">e.g., 11000</param>
        /// <exception cref="Exception">
        ///     Any exception caused by the underlying TcpClient object should be handled (logged)
        ///     and then propagated (re-thrown).   For example, failure to connect will result in an exception
        ///     (i.e., when the server is down or unreachable).
        ///
        ///     See TcpClient documentation for examples of exceptions.
        ///     https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.tcpclient.-ctor?view=net-7.0#system-net-sockets-tcpclient-ctor
        /// </exception>
        public async Task ConnectAsync(string host, int port)
        {
            tcpClient = new TcpClient();
            try
            {
                await tcpClient.ConnectAsync(host, port);//using await Connect Async to avoid thread block
                if (tcpClient.Connected)
                {
                    isConnected = true;
                    remoteAddressPort = $"{tcpClient.Client.RemoteEndPoint}";
                    localAddressPort = $"{tcpClient.Client.LocalEndPoint}";
                    _reportConnected.Invoke(this);
                    _logger.LogInformation($"{clientOrServer}Connected form {tcpClient.Client.LocalEndPoint} to {tcpClient.Client.RemoteEndPoint}");
                    _logger.LogInformation($"{clientOrServer}Awaiting Data...");
                }
                else throw new Exception($"{clientOrServer}tcp Client didn't connect");
            }
            catch (SocketException e)
            {
                _logger.LogError($"{clientOrServer}Failed to connect because {e.Message}");
                throw new Exception($"Failed to connect because {e.Message}");
            }
            catch (Exception e)
            {
                _logger.LogError($"{clientOrServer} Failed to connect because {e.Message}");
                throw new Exception($"Failed to connect because {e.Message}");
                Environment.Exit(0);
            }
        }

        /// <summary>
        ///   <para>
        ///     Close the TcpClient connection between us and them.
        ///   </para>
        ///   <para>
        ///     Important: the reportDisconnect handler will _not_ be called (because if your code
        ///     is calling this method, you already know that the disconnect is supposed to happen).
        ///   </para>
        ///   <para>
        ///     Note: on the SERVER, this does not stop "waiting for connects" which should be stopped first with: StopWaitingForClients
        ///   </para>
        /// </summary>
        public void Disconnect()
        {
            isConnected = false;
            tcpClient?.Close();
        }

        /// <summary>
        ///   <para>
        ///     Precondition: Networking socket has already been connected.
        ///   </para>
        ///   <para>
        ///     Used when one side of the connection waits for a network messages
        ///     from a the other (e.g., client -> server, or server -> client).
        ///     Usually repeated (see infinite).
        ///   </para>
        ///   <para>
        ///     Upon a complete message (based on terminating character, '\n') being received, the message
        ///     is "transmitted" to the _handleMessage function.  Upon successfully handling one message,
        ///     if multiple messages are "queued up", continue to send them (one after another)until no
        ///     messages are left in the stored buffer.
        ///   </para>
        ///   <para>
        ///     Once all data/messages are processed, continue to wait for more data (and repeat).
        ///   </para>
        ///   <para>
        ///     If the TcpClient stream's ReadAsync is "interrupted" (by the connection being closed),
        ///     the stored handle disconnect delegate will be called and this function will end.
        ///   </para>
        ///   <para>
        ///     Note: This code will "await" network activity and thus the _handleMessage (and
        ///     _handleDisconnect) methods are never guaranteed to be run on the same thread, nor are
        ///     they guaranteed to use the same thread for subsequent executions.
        ///   </para>
        /// </summary>
        ///
        /// <param name="infinite">
        ///    if true, then continually await new messages. If false, stop after first complete message received.
        ///    Thus the "infinite" handling will never return (until the connection is severed).
        /// </param>
        public async Task HandleIncomingDataAsync(bool infinite = true)
        {
            try
            {
                StringBuilder dataBacklog = new StringBuilder();
                byte[] buffer = new byte[4096];
                NetworkStream stream = tcpClient.GetStream();

                if (stream == null)
                {
                    return;
                }

                while (true)
                {
                    int total = 0;
                    try
                    {
                        total = await stream.ReadAsync(buffer, ctsForWaitingMessage.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogInformation($"{clientOrServer} Waiting for message has been canceled.");
                        break;
                    }

                    if (total == 0)// the connection quit unexpectedly
                    {
                        _logger.LogInformation($"{clientOrServer} Connection Closed");
                        _reportDisconnected.Invoke(this);
                        return;
                    }

                    string current_data = Encoding.UTF8.GetString(buffer, 0, total);

                    dataBacklog.Append(current_data);

                    _logger.LogTrace($"{clientOrServer}Received {total} new bytes for a total of {dataBacklog.Length}.");

                    if (CheckForMessage(dataBacklog) && !infinite)
                    {
                        _reportDisconnected.Invoke(this);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"oops{ex}");
            }
        }

        /// <summary>
        ///   <para>
        ///     Send a message across the channel (i.e., the TCP Client Stream).  This method
        ///     uses WriteAsync and the await keyword.
        ///   </para>
        ///   <para>
        ///     Important: If the message contains the termination character (TC) (e.g., '\n') it is
        ///     considered part of a **single** message.  All instances of the TC will be replaced with the
        ///     characters "\\n".
        ///   </para>
        ///   <para>
        ///     If an exception is raised upon writing a message to the client stream (e.g., trying to
        ///     send to a "disconnected" recipient) it must be caught, and then
        ///     the _reportDisconnect method must be invoked letting the user of this object know
        ///     that the connection is gone. No exception is thrown by this function.
        ///   </para>
        ///   <para>
        ///     If the connection has been closed already, the send will simply return without
        ///     doing anything.
        ///   </para>
        ///   <para>
        ///     Note: messages are encoded using UTF8 before being sent across the network.
        ///   </para>
        ///   <para>
        ///     For the implementing class, the signature of this method should use "async Task SendAsync(string text)".
        ///   </para>
        ///   <remark>
        ///     Will use the stored tcp object's stream's writeasync method.
        ///   </remark>
        /// </summary>
        /// <param name="text">
        ///   The entire message to send. Note: this string may contain the Termination Character '\n',
        ///   but they will be replaced by "\\n".  Upon receipt, the "\\n" will be replaced with '\n'.
        ///   Regardless, it is a _single_ message from the Networking libraries point of view.
        /// </param>
        public async Task SendAsync(string text)
        {
            if (!isConnected) return;

            //If the text contain \n, then change the TC to \\n, otherwise keep TC for \n
            if (text.IndexOf("\n") >= 0) { text += @"\\n"; }
            else text += "\n";
            // Encode and Send the message
            byte[] messageBytes = Encoding.UTF8.GetBytes(text);

            // Log the trace
            _logger.LogTrace($"{clientOrServer}Sending a message of size ({text.Length}) to {remoteAddressPort}");

            try
            {
                await tcpClient.GetStream().WriteAsync(messageBytes, 0, messageBytes.Length);
                _logger.LogTrace($"{clientOrServer}Message Sent from:   {tcpClient.Client.LocalEndPoint} to {tcpClient.Client.RemoteEndPoint}");
            }
            catch (Exception ex)
            {
                _logger.LogTrace($"{clientOrServer} Client Disconnected: {tcpClient.Client.RemoteEndPoint} - {ex.Message}");
                _reportDisconnected.Invoke(this);
            }
        }

        /// <summary>
        ///   <para>
        ///     Stop listening for connections.  This is achieved using the Cancellation Token Source that
        ///     was attached to the tcplistner back in the wait for clients method.
        ///   </para>
        ///   <para>
        ///     This code allows for graceful termination of the program, such as if a disconnect button
        ///     is pressed on a GUI.
        ///   </para>
        ///   <para>
        ///     This code should be a very simple call to the Cancel method of the appropriate cancellation token
        ///   </para>
        /// </summary>
        public void StopWaitingForClients()
        {
            ctsForWaitingClient.Cancel();
            tcpListener.Stop();
        }

        /// <summary>
        ///   Stop listening for messages.  This is achieved using the Cancellation Token Source.
        ///   This allows for graceful termination of the program. This method should also be very simple
        ///   utilizing the cancellation token associated with the ReadAsync method used in the
        ///   HandleIncomingData method
        /// </summary>
        public void StopWaitingForMessages()
        {
            ctsForWaitingMessage.Cancel();
        }

        /// <summary>
        ///   <para>
        ///     This method is only used by Server applications.
        ///   </para>
        ///   <para>
        ///     Handle client connections;  wait for network connections using the low level
        ///     TcpListener object.  When a new connection is found:
        ///   </para>
        ///   <para>
        ///     IMPORTANT: create a new thread to handle communications from the new client.
        ///   </para>
        ///   <para>
        ///     This routine runs indefinitely until stopped (could accept many clients).
        ///     Important: The TcpListener should have a cancellationTokenSource attached to it in order
        ///     to allow for it to be shutdown.
        ///   </para>
        ///   <para>
        ///     Important: you will create a new Networking object for each client.  This
        ///     object should use the original call back methods instantiated in the servers Networking object.
        ///     The new networking object will need to store the new tcp client object returned from the tcp listener.
        ///     Finally, the new networking object (on its new thread) should HandleIncomingDataAsync
        ///   </para>
        ///   <para>
        ///     Again: All connected clients will "share" the same onMessage and
        ///     onDisconnect delegates, so those methods had better handle this Race Condition.  (IMPORTANT:
        ///     the locking does _not_ occur in the networking code.)
        ///   </para>
        /// </summary>
        /// <param name="port"> Port to listen on </param>
        /// <param name="infinite"> If true, then each client gets a thread that read an infinite number of messages</param>
        public async Task WaitForClientsAsync(int port, bool infinite)
        {
            clientOrServer = " -server- ";
            tcpListener = new TcpListener(IPAddress.Any, port);
            tcpListener.Start();
            _logger.LogInformation($"{clientOrServer} Server Created, Waiting for client");
            while (!ctsForWaitingClient.Token.IsCancellationRequested)
            {
                isWaitingForClients = true;//true when in loop
                try
                {
                    TcpClient connection = await tcpListener.AcceptTcpClientAsync(ctsForWaitingClient.Token);//Will throw error if token is canceled
                    Networking network = new Networking(_logger, _reportConnected,
                                                         _reportDisconnected, _reportMessage, connection);//warp it in to networking class
                    _reportConnected.Invoke(network); //report to the outside a new network is created, but it won't change the old one, since the old network(client) is in another thread and in an infinite loop
                    _logger.LogInformation($"{clientOrServer}New Connection Accepted From {connection.Client.RemoteEndPoint} to {connection.Client.LocalEndPoint}");
                    Task.Run(() => network.HandleIncomingDataAsync(infinite));
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation($"{clientOrServer}Waiting for clients has been canceled.");
                    break;
                }
                catch (Exception e)
                {
                    _logger.LogError($"{clientOrServer}an exception was occur during waiting for clients, which is {e.Message}");
                }
            }
            isWaitingForClients = false;
        }

        /// <summary>
        ///   Given a string (actually a string builder object)
        ///   check to see if it contains one or more messages as defined by
        ///   protocol (the '\n').
        /// </summary>
        /// <param name="sb"></param>
        /// <returns>true if found one or more complete message</returns>
        private bool CheckForMessage(StringBuilder data)
        {
            string allData = data.ToString();

            bool tCchanged = false;//Record if the TC changed from \n to \\n
            int terminator_position = allData.IndexOf(@"\\n");
            if (terminator_position >= 0) tCchanged = true; //If there is "\\n" means that there are \n in the message
            else terminator_position = allData.IndexOf("\n");//else change the TC to original \n and find again

            bool foundOneMessage = false;
            while (terminator_position >= 0)
            {
                foundOneMessage = true;
                string message = allData.Substring(0, terminator_position);

                _reportMessage.Invoke(this, message);

                if (tCchanged) data.Remove(0, terminator_position + 3);
                else data.Remove(0, terminator_position + 1);

                allData = data.ToString();

                if (tCchanged) terminator_position = allData.IndexOf(@"\\n");
                else terminator_position = allData.IndexOf("\n");
            }

            if (!foundOneMessage)
            {
                _logger.LogTrace($"{clientOrServer}No Message found from {allData}");
            }
            else
            {
                _logger.LogTrace($"{clientOrServer}After Message: {data.Length} bytes unprocessed.");
                _logger.LogInformation($"{clientOrServer}Received Message.");
            }
            return foundOneMessage;
        }
    }
}