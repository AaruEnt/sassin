using System;
using System.Collections;

using UnityEngine;
using UnityEngine.SceneManagement;

using Photon.Pun;
using Photon.Realtime;
using Valve.VR;
using UnityEngine.Events;

using NaughtyAttributes;

namespace Com.Aaru.Sassin
{
    public class GameManager : MonoBehaviourPunCallbacks
    {
        #region Public Fields

        [Tooltip("The prefab to use for representing the player")]
        public GameObject playerPrefab;
        public GameObject daggerPrefab;
        [Tooltip("The Player's UI GameObject Prefab")]
        [SerializeField]
        public GameObject PlayerUiPrefab;

        public static GameManager Instance;
        public UnityEvent OnPlayerJoinRoom;

        [Button]
        public void ManualLeaveRoom() { LeaveRoom(); }

        #endregion

        private string MODE_PROP_KEY = "mod";
        #region Photon Callbacks

        /// <summary>
        /// Called when the local player left the room. We need to load the launcher scene.
        /// </summary>
        public override void OnLeftRoom()
        {
            SceneManager.LoadScene(0);
        }

        #endregion

        #region Public Methods

        public void LeaveRoom()
        {
            SteamVR_Fade.View(Color.black, 0.1f);
            PhotonNetwork.LeaveRoom();
        }

        #endregion

        #region Private Methods

        void Start()
        {
            Instance = this;
            if (PhotonNetwork.CurrentRoom != null && PlayerManager.LocalPlayerInstance == null)
            {
                if (playerPrefab == null || daggerPrefab == null)
                {
                    UnityEngine.Debug.LogError("<Color=Red><a>Missing</a></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'", this);
                }
                else
                {
                    if (PlayerManager.LocalPlayerInstance == null)
                    {
                        UnityEngine.Debug.LogFormat("We are Instantiating LocalPlayer from {0}", UnityEngine.Application.loadedLevelName);
                        // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
                        PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(0f, 2f, 0f), Quaternion.identity, 0);
                        PhotonNetwork.Instantiate(this.daggerPrefab.name, new Vector3(0f, 2f, 0f), Quaternion.identity, 0);
                        GameObject tmp = GameObject.Find("UICanvas");
                        GameObject _uiGo = Instantiate(PlayerUiPrefab);
                        _uiGo.SendMessage("SetTarget", tmp.transform.parent.GetComponent<PlayerManager>(), SendMessageOptions.RequireReceiver);
                    }
                }
            }
            SteamVR_Fade.Start(Color.black, 0f);
            SteamVR_Fade.Start(Color.clear, 1f);
            #if UNITY_5_4_OR_NEWER
            // Unity 5.4 has a new scene management. register a method to call CalledOnLevelWasLoaded.
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
            #endif
        }

        void LoadArena()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                UnityEngine.Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
                return;
            }
            UnityEngine.Debug.LogFormat("PhotonNetwork : Loading Level : {0}", PhotonNetwork.CurrentRoom.PlayerCount);
            OnPlayerJoinRoom.Invoke();
            //PhotonNetwork.LoadLevel("Room for " + PhotonNetwork.CurrentRoom.PlayerCount);
            //PhotonNetwork.LoadLevel(Launcher.sceneConnectTo);
        }

        #if UNITY_5_4_OR_NEWER
        void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadingMode)
        {
            this.CalledOnLevelWasLoaded(scene.buildIndex);
        }
        #endif

        #endregion

        #region Photon Callbacks

        public override void OnPlayerEnteredRoom(Player other)
        {
            UnityEngine.Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName); // not seen if you're the player connecting

            if (PhotonNetwork.IsMasterClient)
            {
                UnityEngine.Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom

                LoadArena();
            }
        }

        public override void OnPlayerLeftRoom(Player other)
        {
            UnityEngine.Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName); // seen when other disconnects

            if (PhotonNetwork.IsMasterClient)
            {
                UnityEngine.Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom

                LoadArena();
            }
        }

        public override void OnMasterClientSwitched(Player player)
        {

            var gmm = GetComponent<GamemodeManager>();
            switch (PhotonNetwork.CurrentRoom.CustomProperties[MODE_PROP_KEY])
            {
                case "":
                    if (gmm)
                        gmm.SetGamemode();
                    break;
                case "None":
                    if (gmm)
                        gmm.SetGamemode();
                    break;
                case "Arena":
                    if (gmm)
                        gmm.SetGamemode();
                    break;
                case "Scout":
                    PhotonNetwork.LeaveRoom(); break;
                case "Gather/Invasion":
                    PhotonNetwork.LeaveRoom();
                    break;
                default:
                    PhotonNetwork.LeaveRoom(); break;
            }
        }


        #endregion

        #region MonoBehavior Callbacks

#if !UNITY_5_4_OR_NEWER
        /// <summary>See CalledOnLevelWasLoaded. Outdated in Unity 5.4.</summary>
        void OnLevelWasLoaded(int level)
        {
            this.CalledOnLevelWasLoaded(level);
        }
#endif

        void CalledOnLevelWasLoaded(int level)
        {
            // check if we are outside the Arena and if it's the case, spawn around the center of the arena in a safe zone
            if (!Physics.Raycast(transform.position, -Vector3.up, 5f))
            {
                transform.position = new Vector3(0f, 5f, 0f);
            }
        }

        #if UNITY_5_4_OR_NEWER
        public override void OnDisable()
        {
            // Always call the base to remove callbacks
            base.OnDisable ();
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        #endif

        #endregion
    }
}
