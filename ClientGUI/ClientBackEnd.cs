using AgarioModels;
using Communications;
using Microsoft.Extensions.Logging;
using NetworkingLibrary;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

namespace ClientGUI
{
    public class ClientBackEnd
    {
        private INetworking networking;
        private readonly ILogger _logger;
        public readonly World _world;
        private readonly MainPage _mainPage;
        private readonly Canvas _canvas;

        /// <summary>
        /// Track the mouse position
        /// </summary>
        public Point? relativeToContainerPosition { get; set; }

        public ClientBackEnd(MainPage mainPage)
        {
            _logger = mainPage._logger;
            networking = new Networking(_logger, Connected, Disconnected, ReceivedMessage);
            _mainPage = mainPage;
            _world = new World(_logger);
            this._canvas = new Canvas(_world, mainPage.playSurfacePtr);
            mainPage.playSurfacePtr.Drawable = _canvas;
        }

        /// <summary>
        /// called when user clicked connect button
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
        /// When First client connect to server, we need to send a protocols to server so it knows we start the phase one
        /// </summary>
        /// <param name="channel">networking channel</param>
        private async void Connected(Networking channel)
        {
            _mainPage.gameInfoStackPtr.IsVisible = true;
            _mainPage.playSurfacePtr.IsVisible = true;
            _mainPage.loginStackPtr.IsVisible = false;
            ExecuteOnMainThread((s) => _mainPage.userLoggingLabelPtr.Text = s, "Connected To Server");
            await SendStartGameCommand();
            _world.playerDead = false;
        }

        public async Task SendStartGameCommand()
        {
            await networking.SendAsync(string.Format(Protocols.CMD_Start_Game, _mainPage.nameEntryPtr.Text));
        }

        /// <summary>
        /// A delegate feed to the networking, called when disconnect from the server
        /// </summary>
        /// <param name="channel">networking channel</param>
        private void Disconnected(Networking channel)
        {
            ExecuteOnMainThread((b) => _mainPage.gameInfoStackPtr.IsVisible = b, false);
            ExecuteOnMainThread((b) => _mainPage.playSurfacePtr.IsVisible = b, false);
            ExecuteOnMainThread((b) => _mainPage.loginStackPtr.IsVisible = b, true);
            ExecuteOnMainThread((b) => _mainPage.connectButtonPtr.IsVisible = b, true);
            ExecuteOnMainThread((s) => _mainPage.connectButtonPtr.Text = s, "Connect To Server");
            ExecuteOnMainThread((b) => _mainPage.connectButtonPtr.IsEnabled = b, true);
            ExecuteOnMainThread((s) => _mainPage.userLoggingLabelPtr.Text = s, "Disconnected From Server");
            _world.foods.Clear();
            _world.players.Clear();
        }

        /// <summary>
        /// A delegate feed to the networking, called when received a complete message
        /// </summary>
        /// <param name="channel">networking channel</param>
        /// <param name="message">message</param>
        private void ReceivedMessage(Networking channel, string message)
        {
            if (_world.playerDead) return;
            CheckMessage(message);
            _mainPage.playSurfacePtr.Invalidate();
        }

        public async Task Move(Point? relativeToContainerPosition, CancellationToken cancellationToken)
        {
            lock (this)
            {
                if (relativeToContainerPosition == null) return;
                this.relativeToContainerPosition = relativeToContainerPosition;
            }
            try
            {
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    Vector2 camPos = _canvas.camPos;
                    float zoom = _canvas.currentZoom;

                    string command = String.Format(Protocols.CMD_Move,
                                    (int)((this.relativeToContainerPosition.Value.X - _mainPage.playSurfacePtr.WidthRequest / 2) / zoom + camPos.X),
                                    (int)((this.relativeToContainerPosition.Value.Y - _mainPage.playSurfacePtr.HeightRequest / 2) / zoom + camPos.Y));

                    _logger.LogTrace(command);
                    await networking.SendAsync(command);
                }
            }
            catch (Exception ex) { return; }
        }

        public async Task MoveOnPhone(Vector2 dir)
        {
            Vector2 playerPos = _world.players[_world.playerID].pos;
            playerPos += dir;
            string command = String.Format(Protocols.CMD_Move, (int)playerPos.X, (int)playerPos.Y);
            try
            {
                await networking.SendAsync(command);
                _logger.LogInformation(command);
            }
            catch (Exception ex) { return; }
        }

        public async Task Split()
        {
            Point? relPos = relativeToContainerPosition;
            if (relPos == null) return;
            Vector2 playerPos = _world.players[_world.playerID].pos;

            string command = String.Format(Protocols.CMD_Split,
                                            (int)(relPos.Value.X - _mainPage.playSurfacePtr.WidthRequest / 2 + playerPos.X),
                                            (int)(relPos.Value.Y - _mainPage.playSurfacePtr.HeightRequest / 2 + playerPos.Y));

            _logger.LogTrace(command);
            await networking.SendAsync(command);
        }

        /// <summary>
        /// Check the message that send from server, and update the data.
        /// </summary>
        /// <param name="message"></param>
        private void CheckMessage(string message)
        {
            Match match;

            //Eaten Food
            match = Regex.Match(message, GeneratePattern(Protocols.CMD_Eaten_Food));
            if (match.Success)
            {
                _world.RemoveFood(match.Groups[1].Value);
                return;
            }

            //Eaten Player
            match = Regex.Match(message, GeneratePattern(Protocols.CMD_Dead_Players));
            if (match.Success)
            {
                _world.RemovePlayer(match.Groups[1].Value);
                return;
            }

            //Update Player
            match = Regex.Match(message, GeneratePattern(Protocols.CMD_Update_Players));
            if (match.Success)
            {
                _world.UpdatePlayer(match.Groups[1].Value);
                return;
            }

            //HeartBeat
            match = Regex.Match(message, GeneratePattern(Protocols.CMD_HeartBeat));
            if (match.Success)
            {
                _world.heartBeat = match.Groups[1].Value;
                return;
            }

            //Get Player ID
            match = Regex.Match(message, GeneratePattern(Protocols.CMD_Player_Object));
            if (match.Success)
            {
                _world.playerID = int.Parse(match.Groups[1].Value);
                return;
            }

            //Initialize Food
            match = Regex.Match(message, GeneratePattern(Protocols.CMD_Food));
            if (match.Success)
            {
                _world.InitializeFood(match.Groups[1].Value);
                return;
            }
        }

        private string GeneratePattern(string protocol)
        {
            return protocol + @"(.+)";
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