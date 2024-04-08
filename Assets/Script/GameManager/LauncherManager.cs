// Chua don dep
// Chua comment
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using Photon.Pun.UtilityScripts;
using UnityEngine.UI;

using JSAM;
using System;

public class LauncherManager : MonoBehaviourPunCallbacks
{
    #region Private Serializable Fields
    [SerializeField]
    private byte maxPlayersPerRoom = 4;

    [SerializeField]
    private LocalPlayerData localPlayerData;

    [SerializeField]
    private Cars carsPool;

    [SerializeField]
    private GameObject roomInfoObject;

    #endregion

    #region Private Fields
    private bool isConnecting;
    private string gameVersion = "1";
    private TypedLobby customLobby = new TypedLobby("customLobby", LobbyType.Default);
    private Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();
    private Dictionary<string, GameObject> cachedRoomObjectList = new Dictionary<string, GameObject>();
    private GameObject playerModel;
    private GameObject launcherPanel;
    private GameObject lobbyPanel;
    private GameObject roomPanel;
    private GameObject selectPanel;
    private GameObject playerNamePanel;
    private GameObject optionsPanel;
    private GameObject progressPanel;
    private GameObject roomListContent;
    private GameObject playerListObject;
    private GameObject spawnPosition;
    private List<GameObject> historyPanel = new List<GameObject>();

    #endregion

    #region Public Fields
    public static LauncherManager Instance;

    #endregion

    #region MonoBehaviour Callbacks
    private void Awake()
    {
        if (LauncherManager.Instance == null)
        {
            LauncherManager.Instance = this;
        }

        PhotonNetwork.AutomaticallySyncScene = true;

        launcherPanel = GameObject.FindGameObjectWithTag("Launcher");
        if (launcherPanel == null)
        {
            Debug.LogError("Cannot found Launcher's Panel", this);
        }

        lobbyPanel = GameObject.FindGameObjectWithTag("Lobby");
        if (lobbyPanel == null)
        {
            Debug.LogError("Cannot found Lobby's Panel!", this);
        }

        roomPanel = GameObject.FindGameObjectWithTag("Room");
        if (roomPanel == null)
        {
            Debug.LogError("Cannot found Room's Panel!", this);
        }

        selectPanel = GameObject.FindGameObjectWithTag("CarSelect");
        if (selectPanel == null)
        {
            Debug.LogError("Cannot found Select Car's Panel!", this);
        }

        playerNamePanel = GameObject.FindGameObjectWithTag("PlayerName");
        if (playerNamePanel == null)
        {
            Debug.LogError("Cannot found Player Name's Panel!", this);
        }

        optionsPanel = GameObject.FindGameObjectWithTag("Options");
        if (optionsPanel == null)
        {
            Debug.LogError("Cannot found Options's Panel!", this);
        }

        progressPanel = GameObject.FindGameObjectWithTag("Progress");
        if (progressPanel == null)
        {
            Debug.LogError("Cannot found Progress's Panel!", this);
        }

        roomListContent = GameObject.FindGameObjectWithTag("RoomList");
        if (roomListContent == null)
        {
            Debug.LogError("Cannot found Room List Content!", this);
        }

        playerListObject = GameObject.FindGameObjectWithTag("PlayerList");
        if (playerListObject == null)
        {
            Debug.LogError("Cannot found Player List's Panel!", this);
        }

        if (localPlayerData == null)
        {
            Debug.LogError("Missing local player data!", this);
        }

        if (PlayerPrefs.HasKey("Player Car Index") && PlayerPrefs.HasKey("Player Variant Index")){
            localPlayerData.SetPlayerIndex(
                PlayerPrefs.GetInt("Player Car Index"),
                PlayerPrefs.GetInt("Player Variant Index")
            );
        } else {
            localPlayerData.SetPlayerIndex(0, 0);
        }

        localPlayerData.SetPlayerPrefab(carsPool.GetCarPrefab(
            localPlayerData.GetPlayerCarIndex(),
            localPlayerData.GetPlayerVariantIndex()
        ));

        spawnPosition = GameObject.FindGameObjectWithTag("SpawnPosition");
        if (spawnPosition == null)
        {
            Debug.LogError("Cannot found Spawn Position's Object", this);
        }

        SpawnCarModel(
            carsPool.GetCarPrefab(
                localPlayerData.GetPlayerCarIndex(), 
                localPlayerData.GetPlayerVariantIndex()
            )
        );

        // playerModel = Instantiate(localPlayerData.GetPlayerPrefab(), spawnPosition.transform.position + new Vector3(0f, 0.5f, 0f) , spawnPosition.transform.rotation);
    }

    // Start is called before the first frame update
    void Start()
    {
        historyPanel.Add(launcherPanel);

        if (PlayerPrefs.HasKey("PlayerName"))
        {
            launcherPanel.SetActive(true);
            playerNamePanel.SetActive(false);
        }
        else
        {
            launcherPanel.SetActive(false);
            playerNamePanel.SetActive(true);
            historyPanel.Add(playerNamePanel);
        }

        lobbyPanel.SetActive(false);
        roomPanel.SetActive(false);
        selectPanel.SetActive(false);
        optionsPanel.SetActive(false);
        progressPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    #endregion

    #region MonoBehaviourPunCallbacks Callbacks
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to server");
        PhotonNetwork.JoinLobby(customLobby);
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Connected to lobby");
        lobbyPanel.SetActive(true);
        progressPanel.SetActive(false);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("Room list got updated");
        UpdateCachedRoomList(roomList);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined a room, local player index: " + PhotonNetwork.LocalPlayer.GetPlayerNumber());

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            Debug.Log("Getting player " + player.NickName + "'s info");
            StartCoroutine(WaitUntilPlayerConnectedAndReady(player));
        }
    }

    public override void OnLeftRoom()
    {
        SetPlayerReady();
        Debug.LogWarning("Left the room");

        /// Test: When Client/Local Player (you) left the room, stops all Room sounds that was played (if it is still playing)
        AudioManager.StopSoundIfPlaying(MainGameSounds.beep_synthtone01);
        AudioManager.StopSoundIfPlaying(MainGameSounds.alert_clink);
        AudioManager.StopSoundIfPlaying(MainGameSounds.menu_click01);
    }

    public override void OnLeftLobby()
    {   
        Debug.LogWarning("Left the lobby");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarningFormat("Disconnected from the server: {0}", cause);
        isConnecting = false;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        StartCoroutine(WaitUntilPlayerConnectedAndReady(newPlayer));
        // playerListObject.transform.GetChild(newPlayer.GetPlayerNumber()).transform.GetChild(0).GetComponent<TMP_Text>().text = PhotonNetwork.LocalPlayer.NickName;
        // playerListObject.transform.GetChild(newPlayer.GetPlayerNumber()).transform.GetChild(1).GetComponent<Toggle>().isOn = false;

        /// Test: Plays sound when remote player(s) entered the room
        AudioManager.PlaySound(MainGameSounds.beep_synthtone01);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        /// Test: Plays sound if remote player(s) left the room
        AudioManager.PlaySound(MainGameSounds.alert_clink);

        try{
            playerListObject.transform.GetChild(otherPlayer.GetPlayerNumber()).transform.GetChild(0).GetComponent<TMP_Text>().text = "Wait for player...";
            playerListObject.transform.GetChild(otherPlayer.GetPlayerNumber()).transform.GetChild(1).GetComponent<Toggle>().isOn = false;
        } catch (Exception error){

        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Back();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        Debug.LogWarning("Player " + targetPlayer.NickName + " has changed their properties");

        if (changedProps.ContainsKey("Ready"))
        {
            playerListObject.transform.GetChild(targetPlayer.GetPlayerNumber()).transform.GetChild(1).GetComponent<Toggle>().isOn = (bool)changedProps["Ready"];

            ////// Test: Plays sound if a player is ready (Both client/local and remote players)
            if ((bool)changedProps["Ready"] == true)
            {
                AudioManager.PlaySound(MainGameSounds.menu_click01);
            }
        }
    }

    #endregion

    #region Private Methods
    private void Connect()
    {
        if (PhotonNetwork.IsConnected)
        {
            return;
        }

        progressPanel.SetActive(true);
        launcherPanel.SetActive(false);

        isConnecting = PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.GameVersion = gameVersion;
    }

    private void UpdateCachedRoomList(List<RoomInfo> roomList)
    {
        for (int index = 0; index < roomList.Count; index++)
        {
            RoomInfo info = roomList[index];
            if (info.RemovedFromList)
            {
                Debug.LogWarning("Remove room name: " + info.Name);
                cachedRoomList.Remove(info.Name);

                if (cachedRoomObjectList.ContainsKey(info.Name))
                {
                    Destroy(cachedRoomObjectList[info.Name]);
                }

                cachedRoomObjectList.Remove(info.Name);
            }
            else
            {
                Debug.Log("Add room name: " + info.Name + ", current members: " + info.PlayerCount + "/" + info.MaxPlayers + ", is open: " + info.IsOpen + ", is visible: " + info.IsVisible);
                cachedRoomList[info.Name] = info;
                if (!cachedRoomObjectList.ContainsKey(info.Name) || cachedRoomObjectList[info.Name] == null)
                {
                    Debug.Log(roomInfoObject);
                    cachedRoomObjectList[info.Name] = Instantiate(roomInfoObject, roomListContent.transform);
                }
                cachedRoomObjectList[info.Name].transform.GetChild(0).GetComponent<TMP_Text>().text = info.CustomProperties["ROOM_NAME"].ToString();
                cachedRoomObjectList[info.Name].transform.GetChild(1).GetComponent<TMP_Text>().text = info.IsOpen && info.PlayerCount < info.MaxPlayers ? "Open" : "Closed";
                cachedRoomObjectList[info.Name].transform.GetChild(2).GetComponent<TMP_Text>().text = "Number of player in room: " + info.PlayerCount + "/" + info.MaxPlayers;
                cachedRoomObjectList[info.Name].GetComponent<RoomButton>().SetRoomCode(info.Name);
                if (!info.IsOpen || info.PlayerCount == info.MaxPlayers){
                    cachedRoomObjectList[info.Name].GetComponent<Button>().interactable = false;
                } else {
                    cachedRoomObjectList[info.Name].GetComponent<Button>().interactable = true;
                }
            }
        }
    }

    private void LoadingToLevel()
    {
        roomPanel.SetActive(false);
        progressPanel.SetActive(true);
    }

    #endregion

    #region Public Methods
    /// <summary>
    /// Test: Method to PlaySound when a button is pressed
    /// Most are migrated to SoundHandler.cs in SFX Handler GameObject
    /// </summary>
    //public void PlaySound()
    //{
    //    // I didn't add StopSoundIfPlaying here since I don't think it's really needed?
    //    // I added it though
    //    AudioManager.StopSoundIfPlaying(MainGameSounds.menu_accept);
    //    AudioManager.PlaySound(MainGameSounds.menu_accept);
    //}

    public void EnterLobby()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.JoinLobby(customLobby);
        }
        else
        {
            Connect();
        }

        historyPanel.Add(lobbyPanel);
    }

    public void EnterCarSelect()
    {
        launcherPanel.SetActive(false);
        selectPanel.SetActive(true);

        historyPanel.Add(selectPanel);
    }

    public void EnterSetPlayerName()
    {
        Debug.LogWarning("Player Name Panel still in development!");
        launcherPanel.SetActive(false);
        playerNamePanel.SetActive(true);

        historyPanel.Add(playerNamePanel);
    }

    public void EnterOptions()
    {
        Debug.LogWarning("Options Panel still in development!");

        launcherPanel.SetActive(false);
        optionsPanel.SetActive(true);

        historyPanel.Add(optionsPanel);
    }

    public void CreateRoom()
    {
        Debug.Log("Creating a room");
        RoomOptions roomOptions = new RoomOptions()
        {
            MaxPlayers = maxPlayersPerRoom,
        };

        ExitGames.Client.Photon.Hashtable RoomCustomProps = new ExitGames.Client.Photon.Hashtable
        {
            { "ROOM_NAME", PhotonNetwork.LocalPlayer.NickName + "'s Room" }
        };

        roomOptions.CustomRoomProperties = RoomCustomProps;

        roomOptions.CustomRoomPropertiesForLobby = new string[] {
            "ROOM_NAME",
        };

        PhotonNetwork.CreateRoom(null, roomOptions, customLobby);
        progressPanel.SetActive(true);
        lobbyPanel.SetActive(false);

        historyPanel.Add(roomPanel);

        StartCoroutine(WaitUntilInsideRoom());
    }

    public void EnterRoom(string roomCode)
    {
        Debug.Log("Entering room name " + roomCode);
        PhotonNetwork.JoinRoom(roomCode);
        progressPanel.SetActive(true);
        lobbyPanel.SetActive(false);

        historyPanel.Add(roomPanel);

        StartCoroutine(WaitUntilInsideRoom());
    }

    public void SetPlayerReady()
    {
        ExitGames.Client.Photon.Hashtable readyProperty = PhotonNetwork.LocalPlayer.CustomProperties;

        if (!readyProperty.ContainsKey("Ready"))
        {
            readyProperty["Ready"] = true;
            StartCoroutine(WaitUntilGameStart());
        }
        else
        {
            readyProperty["Ready"] = !(bool)readyProperty["Ready"];
            StopAllCoroutines();
        }

        PhotonNetwork.LocalPlayer.SetCustomProperties(readyProperty);
    }

    public bool IsLocalPlayerReady(){
        return PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Ready") && (bool)PhotonNetwork.LocalPlayer.CustomProperties["Ready"] == true;
    }

    public bool CheckAllPlayerReady()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (!player.CustomProperties.ContainsKey("Ready") || !(bool)player.CustomProperties["Ready"])
            {
                return false;
            }
        }

        return true;
    }

    public void CloseRoom(){
        PhotonNetwork.CurrentRoom.IsOpen = false;
    }

    public void OpenRoom(){
        PhotonNetwork.CurrentRoom.IsOpen = true;
    }

    public void StartGame()
    {
        PhotonNetwork.CurrentRoom.IsVisible = false;
        PhotonNetwork.CurrentRoom.IsOpen = false;

        ExitGames.Client.Photon.Hashtable gameReadyProperty = PhotonNetwork.LocalPlayer.CustomProperties;
        gameReadyProperty["GameStart"] = true;
        PhotonNetwork.LocalPlayer.SetCustomProperties(gameReadyProperty);
        LoadingToLevel();

        if (TrackSelection.Instance.GetTrackName() == "Random Track")
        {
            PhotonNetwork.LoadLevel(TrackSelection.Instance.GetRandomTrackName());
        }
        else
        {
            PhotonNetwork.LoadLevel(TrackSelection.Instance.GetTrackName());
        }
    }

    public LocalPlayerData GetLocalPlayerData(){
        return this.localPlayerData;
    }

    public Cars GetCarsPool(){
        return this.carsPool;
    }

    public void SpawnCarModel(GameObject car){
        if (this.playerModel != null) {
            Destroy(this.playerModel);
        }

        this.playerModel = Instantiate(car, spawnPosition.transform.position + new Vector3(0f, 0.5f, 0f) , spawnPosition.transform.rotation);
    }

    public void Back(){
        if (historyPanel.Count == 1)
            return;

        if (historyPanel[historyPanel.Count - 1] == roomPanel) {
            PhotonNetwork.LeaveRoom();
        } else if (historyPanel[historyPanel.Count - 1] == lobbyPanel){
            PhotonNetwork.LeaveLobby();
            PhotonNetwork.Disconnect();
        }

        historyPanel[historyPanel.Count - 1].SetActive(false);
        historyPanel.RemoveAt(historyPanel.Count - 1);
        historyPanel[historyPanel.Count - 1].SetActive(true);
    }

    #endregion

    #region PunRPC Methods

    #endregion

    #region IEnumerator Callbacks
    IEnumerator WaitUntilInsideRoom()
    {
        yield return new WaitUntil(() => PhotonNetwork.InRoom);
        roomPanel.SetActive(true);
        progressPanel.SetActive(false);

        roomPanel.transform.GetChild(0).GetComponent<TMP_Text>().text = PhotonNetwork.CurrentRoom.CustomProperties["ROOM_NAME"].ToString();
    }

    IEnumerator WaitUntilPlayerConnectedAndReady(Player player)
    {
        yield return new WaitUntil(() => player.GetPlayerNumber() != -1);

        Debug.Log("Player number " + player.GetPlayerNumber() + ": " + player.NickName);
        playerListObject.transform.GetChild(player.GetPlayerNumber()).transform.GetChild(0).GetComponent<TMP_Text>().text = player.NickName;
        playerListObject.transform.GetChild(player.GetPlayerNumber()).transform.GetChild(1).GetComponent<Toggle>().isOn = player.CustomProperties.ContainsKey("Ready") ?
                                                                                                                          (bool)player.CustomProperties["Ready"] :
                                                                                                                          false;
    }

    IEnumerator WaitUntilGameStart()
    {
        yield return new WaitUntil(() => PhotonNetwork.CurrentRoom.GetPlayer(PhotonNetwork.CurrentRoom.MasterClientId).CustomProperties.ContainsKey("GameStart") &&
                                         (bool)PhotonNetwork.CurrentRoom.GetPlayer(PhotonNetwork.CurrentRoom.MasterClientId).CustomProperties["GameStart"]);
        LoadingToLevel();
    }

    #endregion
}
