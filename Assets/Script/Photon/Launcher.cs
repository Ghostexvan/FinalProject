// Chua don dep
// Chua comment
using UnityEngine;

// Thu vien can thiet de thiet lap ket noi den server
using Photon.Pun;
// Thu vien can thiet de su dung Photon trong realtime
using Photon.Realtime;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using Photon.Pun.UtilityScripts;

// Su dung class MonoBehaviourPunCallbacks nham su dung duoc cac ham cua server Photon
public class Launcher : MonoBehaviourPunCallbacks{
    #region Private Serializable Fields
    // Region chua nhung truong private co the serialize
    // Serialize co the hieu don gian la cac truong co the duoc hien thi trong tab Inspector trong Unity
    // So luong nguoi choi toi da trong mot phong
    [Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created")]
    [SerializeField]
    private byte maxPlayersPerRoom = 4;

    // Panel hien thi ten nguoi choi va nut Play
    [Tooltip("The UI Panel to let the user enter name, connect and play")]
    [SerializeField]
    private GameObject controlPanel;

    // Label hien thi trang thai ket noi
    [Tooltip("The UI Label to inform the user that the connection is in progress")]
    [SerializeField]
    private GameObject progressLabel;

    // Panel hien thi thong tin lobby
    [Tooltip("The UI Panel that inform lobby info")]
    [SerializeField]
    private GameObject lobbyPanel;

    // Tong hop cac Text hien thi ten nguoi choi trong Lobby
    [Tooltip("The UI Text to inform all users name in a lobby")]
    [SerializeField]
    private GameObject[] playerText;

    [Tooltip("Start button for master client")]
    [SerializeField]
    private GameObject startButton;

    [Tooltip("Name of level to load when start game")]
    [SerializeField]
    private string levelName;

    #endregion

    #region Private Fields
    // Region chua nhung truong private
    // Phien ban cua client:
    //      Dung de phan biet giua cac phien ban
    //      Co the duoc su dung de phat trien va kiem thu cac ban cap nhat
    //      Nen de 1 neu dang trong qua trinh phat trien
    string gameVersion = "1";

    // Kiem tra trang thai ket noi hien tai cua client
    bool isConnecting;

    #endregion

    #region Public Fields
    // Region chua nhung truong public

    #endregion

    #region MonoBehaviour CallBacks
    // Region chua nhung ham CallBacks trong Unity
    // Ham nay duoc goi trong giai doan khoi tao Object
    private void Awake() {
        // Quan trong
        // Su dung de dong bo giua server va client
        PhotonNetwork.AutomaticallySyncScene = true;    
    }

    // Ham nay duoc goi dau tien sau giai doan khoi tao Object
    private void Start() {
        // Connect();

        // An Label trang thai, hien thi Panel nhap ten va nut Play
        progressLabel.SetActive(false);
        controlPanel.SetActive(true);
        lobbyPanel.SetActive(false);
    }

    private void Update() {
        //Debug.Log(PhotonNetwork.PlayerList.Length);
        // Debug.Log(PhotonNetwork.LocalPlayer.NickName + " is in room: " + PhotonNetwork.InRoom);
        if (PhotonNetwork.InRoom){
            foreach (Player player in PhotonNetwork.PlayerList){
                // Debug.Log(player.GetPlayerNumber() + ": " + player.NickName);
                if (player.GetPlayerNumber() != -1)
                    playerText[player.GetPlayerNumber()].GetComponent<TMP_Text>().text = player.NickName;
            }

            for (int index = PhotonNetwork.PlayerList.Length; index < maxPlayersPerRoom; index++){
                playerText[index].GetComponent<TMP_Text>().text = "Waiting for player to join";
            }

            if (PhotonNetwork.IsMasterClient){
                startButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Start";
                startButton.GetComponent<Button>().enabled = true;
            }
            else {
                startButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Wait for host";
                startButton.GetComponent<Button>().enabled = false;
            }
            
            if (!PhotonNetwork.IsMasterClient && !PhotonNetwork.CurrentRoom.IsOpen){
                StartGame();
            }
        }
    }

    #endregion    

    #region MonoBehaviourPunCallbacks Callbacks
    // Region nay chua nhung ham Callbacks duoc su dung trong server Photon
    // Override ham duoc goi khi ket noi den may chu
    public override void OnConnectedToMaster()
    {
        Debug.Log("PUN Basics Tutorial/Launcher: OnConnectedToMaster() was called by PUN");

        // Neu client dang muon ket noi den phong choi
        if (isConnecting){
            // Thu ket noi den mot phong co san
            // Neu that bai, ham OnJoinRandomFailed() se duoc goi
            PhotonNetwork.JoinRandomRoom();

            // Da ket noi thanh cong -> Khong muon ket noi nua
            isConnecting = false;
        }
    }

    // Override ham duoc goi khi mat ket noi den may chu
    public override void OnDisconnected(DisconnectCause cause)
    {
        // An Label trang thai, hien thi Panel nhap ten va nut Play
        progressLabel.SetActive(false);
        controlPanel.SetActive(true);

        // Da thoat khoi phong -> Khong muon ket noi nua
        isConnecting = false;
        
        Debug.LogWarning($"PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {cause}");
    }

    // Override ham duoc goi khi ket noi den phong that bai
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("PUN Basics Tutorial/Launcher: OnJoinRandomFailed() was called by PUN. No random room avaiable, so we create one.\nCalling: PhotonNetwork.CreateRoom()");
        // Tao mot phong moi
        // Syntax: ten phong, cai dat phong
        // Cac cai dat phong: MaxPlayers - so luong nguoi choi toi da
        PhotonNetwork.CreateRoom(null, new RoomOptions{ MaxPlayers = maxPlayersPerRoom });
    }

    // Override ham duoc goi khi tham gia mot phong
    public override void OnJoinedRoom()
    {
        Debug.Log("PUN Basics Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");

        // Neu so luong nguoi choi trong phong hien la 1
        // if (PhotonNetwork.CurrentRoom.PlayerCount == 1){
        //     Debug.Log("Load the 'Room for 1'");

        //     // Load man choi tuong ung voi 1 nguoi choi (chu phong)
        //     PhotonNetwork.LoadLevel("Cartoon Track Test");
        // }

    }

    #endregion

    #region Public Methods
    // Region chua nhung ham public
    // Tien trinh ket noi den server:
    //      Da ket noi den server -> Tham gia mot phong ngau nhien
    //      Chua ket noi den server -> Co gang ket noi den server
    public void Connect(){
        // An Panel nhap ten va nut Play, hien thi Label trang thai
        controlPanel.SetActive(false);
        progressLabel.SetActive(true);
        lobbyPanel.SetActive(false);

        // Kiem tra ket noi
        if (PhotonNetwork.IsConnected){
            // Tham gia phong ngau nhien neu da ket noi den server
            PhotonNetwork.JoinRandomRoom();
        }
        else {
            // Co gang ket noi den server
            isConnecting = PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = gameVersion;
        }

        StartCoroutine(WaitUntilJoined());
    }

    public void QuitLobby(){
        controlPanel.SetActive(true);
        progressLabel.SetActive(false);
        lobbyPanel.SetActive(false);

        PhotonNetwork.LeaveRoom();
    }

    public void StartGame(){
        controlPanel.SetActive(false);
        progressLabel.SetActive(true);
        lobbyPanel.SetActive(false);

        if (!PhotonNetwork.IsMasterClient){
            return;
        }

        PhotonNetwork.CurrentRoom.IsVisible = false;
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.LoadLevel(levelName);
    }

    #endregion

    #region IEnumerator
    IEnumerator WaitUntilJoined(){
        yield return new WaitUntil(() => PhotonNetwork.InRoom);
        controlPanel.SetActive(false);
        progressLabel.SetActive(false);
        lobbyPanel.SetActive(true);
    }

    #endregion
}