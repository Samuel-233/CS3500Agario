using Microsoft.Extensions.Logging;

namespace ClientGUI
{
    public partial class MainPage : ContentPage
    {
        public readonly ClientBackEnd backEnd;
        public readonly ILogger _logger;
        public readonly Entry nameEntryPtr;
        public readonly Entry iPAddressEntryPtr;
        public readonly Entry portEntryPtr;
        public readonly Button connectButtonPtr;
        public readonly Label userLoggingLabelPtr;
        public readonly VerticalStackLayout gameInfoStackPtr;
        public readonly VerticalStackLayout loginStackPtr;
        public readonly GraphicsView playSurfacePtr;

        private CancellationTokenSource continusMove;

        private DateTime lastTappedTime;

        //TODO Figure out how to do DI for the backEnd
        public MainPage(ILogger<MainPage> logger)
        {
            InitializeComponent();
            _logger = logger;
            nameEntryPtr = NameEntry;
            iPAddressEntryPtr = IPAddressEntry;
            portEntryPtr = PortEntry;
            connectButtonPtr = ConnectButton;
            userLoggingLabelPtr = UserLoggingLabel;
            gameInfoStackPtr = gameInfoStack;
            playSurfacePtr = playSurface;
            loginStackPtr = loginStack;
            backEnd = new ClientBackEnd(this);
            continusMove = new CancellationTokenSource();
        }

        /// <summary>
        /// Called when user clicked connect button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ConnectButton_Clicked(object sender, EventArgs e)
        {
            if (!backEnd.isConnectedToServer())
            {
                NameEntry.IsReadOnly = false;
                IPAddressEntry.IsReadOnly = false;
                gameInfoStack.IsVisible = false;
                playSurface.IsVisible = false;
                ConnectButton.Text = "Connect To Server";
            }
            else
            {
                ConnectButton.Text = "Connecting...";
                ConnectButton.IsEnabled = false;

                if (await backEnd.ConnectToServer())
                {
                    await backEnd.StartToHandleIncomingData();
                }
                else
                {
                    ConnectButton.IsEnabled = true;
                    ConnectButton.Text = "Connect To Server";
                    NameEntry.IsReadOnly = false;
                    IPAddressEntry.IsReadOnly = false;
                }
            }
        }

        /// <summary>
        /// Update the ball's target position when user move the mouse
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PointerMoved(object sender, PointerEventArgs e)
        {
            lock (backEnd)
            {
                backEnd.relativeToContainerPosition = e.GetPosition((View)sender);
            }
        }

        /// <summary>
        /// When pointer Pressed, call this function.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void PointerPressed(object sender, PointerEventArgs e)
        {
            if (backEnd._world.playerDead)
            {
                _logger.LogInformation("Restarted Game");
                backEnd._world.playerDead = false;
                await backEnd.SendStartGameCommand();
                continusMove = new();
                return;
            }
            await backEnd.Split();
        }

        /// <summary>
        /// When Pointer entered the play surface, start to move toward to pointer until pointer leave the play surface
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void PointerEntered(object sender, PointerEventArgs e)
        {
            if (backEnd._world.playerDead) return;
            continusMove = new();
            Point? relativeToContainerPosition = e.GetPosition((View)sender);
            if (relativeToContainerPosition == null) return;
            Task t = new Task(async () =>
            {
                await backEnd.Move(relativeToContainerPosition, continusMove.Token);
            });
            t.Start();
            await t;
        }

        /// <summary>
        /// When pointer Exit the play surface, stop the movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PointerExited(object sender, PointerEventArgs e)
        {
            continusMove.Cancel();
        }

        /// <summary>
        /// When user tap the screen, will call this func
        /// NOTICE: this func will be also called by mouse input
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnTap(object sender, TappedEventArgs e)
        {
            //When player dead, click to restart
            if (backEnd._world.playerDead)
            {
                _logger.LogInformation("Restarted Game");
                backEnd._world.playerDead = false;
                await backEnd.SendStartGameCommand();
                continusMove = new();
                return;
            }

            double tappedTimeInterval = Math.Abs((lastTappedTime - System.DateTime.Now).TotalSeconds);
            lastTappedTime = System.DateTime.Now;
            _logger.LogInformation($"Interval is {tappedTimeInterval}");
            //If user clicked twice, split the ball
            if (tappedTimeInterval < 0.5) await backEnd.Split();
        }

        /// <summary>
        /// When user Drag the surface, call this func.
        /// NOTICE: this func will be also called by mouse input
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void PanUpdated(object sender, PanUpdatedEventArgs e)
        {
            if (backEnd._world.playerDead) return;
            switch (e.StatusType)
            {
                case GestureStatus.Running:
                    float x = (float)e.TotalX;
                    float y = (float)e.TotalY;
                    Point moveDist = GetRelPosOnPhone(new Point(x, y));

                    await backEnd.MoveOnPhone(new System.Numerics.Vector2((float)moveDist.X, (float)moveDist.Y));
                    break;
            }
        }

        /// <summary>
        /// Convert the space from the phone in to playSurface space.
        /// </summary>
        /// <param name="value">The position want to convert</param>
        /// <returns>the corresponding pos in play surface.</returns>
        private Point GetRelPosOnPhone(Point? value)
        {
            if (value == null) return new Point(0,0);
            var mainDisplayInfo = DeviceDisplay.MainDisplayInfo;
            double width = mainDisplayInfo.Width;
            double height = mainDisplayInfo.Height;
            return new Point(value.Value.X / width * playSurface.Width, value.Value.Y / height * playSurface.Height);
        }
    }
}