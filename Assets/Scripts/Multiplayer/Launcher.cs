using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Diagnostics;

namespace Com.Aaru.Sassin
{
    public class Launcher : MonoBehaviourPunCallbacks
    {
        #region Private Serializable Fields

        /// <summary>
        /// The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created.
        /// </summary>
        [Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created")]
        [SerializeField]
        private byte maxPlayersPerRoom = 4;

        [Tooltip("The Ui Panel to let the user enter name, connect and play")]
        [SerializeField]
        private GameObject controlPanel;
        [Tooltip("The UI Label to inform the user that the connection is in progress")]
        [SerializeField]
        private GameObject progressLabel;

        public bool connectOnStart = false;
        public bool useOfflineMode = false;
        public bool createNewRoom = false;

        private string MAP_PROP_KEY = "map";
        private string MODE_PROP_KEY = "mod";

        public void UseOfflineMode(bool mode)
        {
            useOfflineMode = mode;
        }

        public void CreateNewRoom(bool mode)
        {
            createNewRoom = mode;
        }

        public static string sceneConnectTo = "Multiplayer Arena";

        #endregion

        #region Private Fields

        /// <summary>
        /// This client's version number. Users are separated from each other by gameVersion (which allows you to make breaking changes).
        /// </summary>
        string gameVersion = "1.1";

        /// <summary>
        /// Keep track of the current process. Since connection is asynchronous and is based on several callbacks from Photon,
        /// we need to keep track of this to properly adjust the behavior when we receive call back by Photon.
        /// Typically this is used for the OnConnectedToMaster() callback.
        /// </summary>
        bool isConnecting;

        #endregion

        #region MonoBehaviour CallBacks

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
        /// </summary>
        void Awake()
        {
            // #Critical
            // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during initialization phase.
        /// </summary>
        void Start()
        {
            //Connect();
            progressLabel.SetActive(false);
            controlPanel.SetActive(true);
            PhotonNetwork.Disconnect();

            if (connectOnStart)
            {
                Connect(sceneConnectTo);
            }
        }

        #endregion


        #region Public Methods

        /// <summary>
        /// Start the connection process.
        /// - If already connected, we attempt joining a random room
        /// - if not yet connected, Connect this application instance to Photon Cloud Network
        /// </summary>
        public void Connect(string toConnectTo)
        {
            progressLabel.SetActive(true);
            controlPanel.SetActive(false);
            sceneConnectTo = toConnectTo;
            PhotonNetwork.OfflineMode = useOfflineMode;
            if (PhotonNetwork.OfflineMode)
                return;
            // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
            else if (PhotonNetwork.IsConnected)
            {
                if (createNewRoom)
                {
                    CreateRoomFull();
                    return;
                }
                // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnJoinRandomFailed() and we'll create one.
                PhotonNetwork.JoinRandomRoom();
            }
            else
            {
                // keep track of the will to join a room, because when we come back from the game we will get a callback that we are connected, so we need to know what to do then
                isConnecting = PhotonNetwork.ConnectUsingSettings();
                PhotonNetwork.GameVersion = gameVersion;
            }
        }

        #endregion

        #region MonoBehaviourPunCallbacks Callbacks

        public override void OnConnectedToMaster()
        {
            UnityEngine.Debug.Log("PUN Basics Tutorial/Launcher: OnConnectedToMaster() was called by PUN");
            // #Critical: The first we try to do is to join a potential existing room. If there is, good, else, we'll be called back with OnJoinRandomFailed()
            // we don't want to do anything if we are not attempting to join a room.
            // this case where isConnecting is false is typically when you lost or quit the game, when this level is loaded, OnConnectedToMaster will be called, in that case
            // we don't want to do anything.
            if (isConnecting)
            {
                if (PhotonNetwork.OfflineMode)
                {
                    PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 1 });
                    isConnecting = false;
                }
                else
                {
                    // #Critical: The first we try to do is to join a potential existing room. If there is, good, else, we'll be called back with OnJoinRandomFailed()
                    if (createNewRoom)
                    {
                        CreateRoomFull();
                        return;
                    }
                    ExitGames.Client.Photon.Hashtable RoomCustomProps = new ExitGames.Client.Photon.Hashtable();
                    RoomCustomProps.Add(MAP_PROP_KEY, sceneConnectTo);
                    RoomCustomProps.Add(MODE_PROP_KEY, "tmp");
                    PhotonNetwork.JoinRandomRoom(RoomCustomProps, 0);
                    isConnecting = false;
                }
            }
            else if (PhotonNetwork.OfflineMode)
                PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 1 });
            //PhotonNetwork.JoinRandomRoom();
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            progressLabel.SetActive(false);
            controlPanel.SetActive(true);
            isConnecting = false;
            UnityEngine.Debug.LogWarningFormat("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {0}", cause);
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            UnityEngine.Debug.Log("PUN Basics Tutorial/Launcher:OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");



            // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
            CreateRoomFull();
        }

        public override void OnJoinedRoom()
        {
            UnityEngine.Debug.Log("PUN Basics Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
            // #Critical: We only load if we are the first player, else we rely on `PhotonNetwork.AutomaticallySyncScene` to sync our instance scene.
            if (PhotonNetwork.IsMasterClient)
            {
                UnityEngine.Debug.Log("We load the 'Room for 1' ");

                // #Critical
                // Load the Room Level.
                //PhotonNetwork.LoadLevel("Room for 1");
                PhotonNetwork.LoadLevel(sceneConnectTo);
            }
        }

        public void CreateRoomFull()
        {
            RoomOptions roomOptions =
                new RoomOptions()
                {
                    MaxPlayers = maxPlayersPerRoom,
                    IsVisible = true,
                    IsOpen = true,
                };

            ExitGames.Client.Photon.Hashtable RoomCustomProps = new ExitGames.Client.Photon.Hashtable();
            RoomCustomProps.Add(MAP_PROP_KEY, sceneConnectTo);
            RoomCustomProps.Add(MODE_PROP_KEY, "tmp");
            roomOptions.CustomRoomProperties = RoomCustomProps;

            string[] customLobbyProperties = { MAP_PROP_KEY, MODE_PROP_KEY };

            roomOptions.CustomRoomPropertiesForLobby = customLobbyProperties;


            PhotonNetwork.CreateRoom(null, roomOptions);
        }

        #endregion

    }
}
