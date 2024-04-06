using AgarioModels;
using Communications;
using Microsoft.Extensions.Logging;
using NetworkingLibrary;
using System.Text;


namespace ClientGUI
{
    public class ClientBackEnd
    {
        private INetworking networking;
        private readonly ILogger _logger;
        private readonly World _world;
        private readonly MainPage _mainPage;

        public ClientBackEnd(MainPage mainPage)
        {
            _logger = mainPage._logger;
            networking = new Networking(_logger, Connected, Disconnected, ReceivedMessage);
            _mainPage = mainPage;
            _world = new World(_logger);
            mainPage.playSurfacePtr.Drawable = new Canvas(_world, mainPage.playSurfacePtr);
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
                _mainPage.portEntryPtr.Text = "11000";
                logging.AppendLine("Invalid port, Change to 11000");
            }

            _mainPage.userLoggingLabelPtr.Text = logging.ToString();

            try
            {
                await networking.ConnectAsync(_mainPage.iPAddressEntryPtr.Text, int.Parse(_mainPage.portEntryPtr.Text));
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
            _mainPage.gameInfoStackPtr.IsVisible = true;
            _mainPage.playSurfacePtr.IsVisible = true;
            _mainPage.loginStackPtr.IsVisible = false;
            ExecuteOnMainThread((s) => _mainPage.userLoggingLabelPtr.Text = s, "Connected To Server");

        }

        /// <summary>
        /// A delegate feed to the networking, called when disconnect from the server
        /// </summary>
        /// <param name="channel">networking channel</param>
        private void Disconnected(Networking channel)
        {
            _mainPage.gameInfoStackPtr.IsVisible = false;
            _mainPage.playSurfacePtr.IsVisible = false;
            _mainPage.loginStackPtr.IsVisible = true;
            _mainPage.connectButtonPtr.IsEnabled = true;
            _mainPage.connectButtonPtr.Text = "Connect To Server";
            ExecuteOnMainThread((s) => _mainPage.userLoggingLabelPtr.Text = s, "Disconnected From Server");
        }

        /// <summary>
        /// A delegate feed to the networking, called when received a complete message
        /// </summary>
        /// <param name="channel">networking channel</param>
        /// <param name="message">message</param>
        private async void ReceivedMessage(Networking channel, string message)
        {
            //_mainPage.playSurfacePtr.Invalidate();
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
