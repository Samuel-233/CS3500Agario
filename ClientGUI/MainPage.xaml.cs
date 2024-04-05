using Microsoft.Extensions.Logging;
using System.Xml.Serialization;

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

        //TODO Figure out how to do DI for the backEnd
        public MainPage(ILogger<MainPage> logger)
        {
            InitializeComponent();
            _logger = logger;
            backEnd = new ClientBackEnd(this);
            nameEntryPtr = NameEntry;
            iPAddressEntryPtr = IPAddressEntry;
            portEntryPtr = PortEntry;
            connectButtonPtr = ConnectButton;
            userLoggingLabelPtr = UserLoggingLabel;
            gameInfoStackPtr = gameInfoStack;
            playSurfacePtr = playSurface;
            loginStackPtr = loginStack;
            /*            this.PlaySurface.Drawable = new MyCanvas(boxes,
                            MoveOnUpdateCheckBox, InvalidateAlwaysCheckBox,
                            DrawOnMe);*/
            
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

        private async void PointerChanged(object  sender, EventArgs e){

        }

        private async void OnTap(object sender, EventArgs e)
        {

        }

        private async void PanUpdated(object sender, EventArgs e)
        {

        }
    }

}
