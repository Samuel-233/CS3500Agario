using Communications;
using Microsoft.Extensions.Logging;
using NetworkingLibrary;
using System.Text;


namespace ClientGUI
{
    internal class ClientBackEnd
    {
        private INetworking networking;
        private readonly ILogger _logger;
        private readonly MainPage _mainPage;

        public ClientBackEnd(MainPage mainPage)
        {
            _logger = mainPage._logger;
            networking = new Networking(_logger, Connected, Disconnected, ReceivedMessage);
            _mainPage = mainPage;
        }



        /// <summary>
        /// alled when user clicked connect button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns>return true when connected to server, else return false</returns>
        public bool isConnectedToServer()
        {
            if (networking != null && networking.IsConnected)
            {
                networking.Disconnect();
                _logger.LogInformation("Disconnect from server");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Connect to Server
        /// </summary>
        /// <returns>true if connected, else false</returns>
        public async Task<bool> ConnectToServer()
        {
            _mainPage.nameEntryPtr.IsReadOnly = true;
            _mainPage.iPAddressEntryPtr.IsReadOnly = true;
            _mainPage.portEntryPtr.IsReadOnly = true;

            StringBuilder logging = new StringBuilder();

            if (string.IsNullOrWhiteSpace(_mainPage.nameEntryPtr.Text))
            {
                string userName = System.Environment.UserName;
                _mainPage.nameEntryPtr.Text = userName;
                logging.AppendLine("Invalid User Name, Change to computer User Name");
            }
            networking.ID = _mainPage.nameEntryPtr.Text;

            //Check Address and port
            if (string.IsNullOrWhiteSpace(_mainPage.iPAddressEntryPtr.Text))
            {
                _mainPage.iPAddressEntryPtr.Text = "localhost";
                logging.AppendLine("Invalid IP, Change to localhost");
            }
            if (string.IsNullOrWhiteSpace(_mainPage.portEntryPtr.Text))
            {
                _mainPage.iPAddressEntryPtr.Text = "11000";
                logging.AppendLine("Invalid port, Change to 11000");
            }

            try
            {
                await networking.ConnectAsync(_mainPage.iPAddressEntryPtr.Text, int.Parse(_mainPage.iPAddressEntryPtr.Text));
            }
            catch (Exception ex)
            {
                logging.AppendLine(ex.Message);
                ExecuteOnMainThread((s) => _mainPage.userLoggingLabelPtr.Text = s, logging.ToString());
                return false;
            }
            return true;
        }


        /// <summary>
        /// Starting let client to handle incoming data
        /// </summary>
        public async Task StartToHandleIncomingData()
        {
            await networking.HandleIncomingDataAsync();
        }


        /// <summary>
        /// A delegate feed to the networking, called when connected to server
        /// </summary>
        /// <param name="channel">networking channel</param>
        private async void Connected(Networking channel)
        {
        throw new NotImplementedException();
/*            await networking.SendAsync($"Command Name {nameEntry.Text}");
            connectBtn.IsEnabled = true;
            connectBtn.Text = "DisConnect";
            ipAddressEntry.IsReadOnly = true;
            UpdateMessageBox("Connected To the Server", 1);*/
        }

        /// <summary>
        /// A delegate feed to the networking, called when disconnect from the server
        /// </summary>
        /// <param name="channel">networking channel</param>
        private void Disconnected(Networking channel)
        {
            throw new NotImplementedException();
            /*            nameEntry.IsReadOnly = false;
                        ipAddressEntry.IsReadOnly = false;
                        networking.Disconnect();
                        connectBtn.Text = "Connect To Server";
                        UpdateMessageBox("Disconnected from server", 1);*/
        }

        /// <summary>
        /// A delegate feed to the networking, called when received a complete message
        /// </summary>
        /// <param name="channel">networking channel</param>
        /// <param name="message">message</param>
        private async void ReceivedMessage(Networking channel, string message)
        {
            throw new NotImplementedException();
            /*            if (message.Contains("Command Participants,") &&
                        message.Substring(0, 21).Equals("Command Participants,"))
                        {
                            UpdateParticipants(message.Substring(21));
                            return;
                        }
                        if (message.Contains("NAME REJECTED") && message.Length == 13)
                        {
                            nameEntry.Text = oldName;
                            UpdateMessageBox($"Change name request rejected, change back to old name {oldName}", 0);
                            await networking.SendAsync("Command Name " + oldName);
                            return;
                        }
                        UpdateMessageBox(message, 3);*/
        }

        /// <summary>
        /// Make the func execute on Main thread
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <param name="pram1">The pram 1 for the func</param>
        public void ExecuteOnMainThread<T>(Action<T> action, T pram1)
        {
            if (MainThread.IsMainThread)
            {
                //Update Directly if it is on main thread
                action.Invoke(pram1);
            }
            else
            {
                //If not, change it to main thread
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    action.Invoke(pram1);
                });
            }
        }
    }
}
