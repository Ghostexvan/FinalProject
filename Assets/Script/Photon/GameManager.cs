// Chua don dep
// Chua comment
using System;
using System.Collections;

using UnityEngine;
// Thu vien can thiet de quan ly cac Scene
using UnityEngine.SceneManagement;

// Thu vien can thiet de thiet lap ket noi den server
using Photon.Pun;
// Thu vien can thiet de su dung Photon trong realtime
using Photon.Realtime;
using TMPro;
using Photon.Pun.UtilityScripts;

public class GameManager : MonoBehaviourPunCallbacks, IPunObservable
{
    #region Private Fields
    private bool isStart = false;

    #endregion

    #region  Private Serialize Fields
    [SerializeField]
    private GameObject startingPanel;

    [SerializeField]
    private GameObject gamePanel;

    [SerializeField]
    private int totalLaps;

    [SerializeField]
    private GameObject[] checkpointList;

    [SerializeField]
    private PlayerInfo[] playerInfos;

    #endregion

    #region Public Fields
    // Region chua cac truong public
    // Instance cua GameManager
    // Su dung static de co the truy cap tu bat ky dau
    public static GameManager Instance;

    // Prefab tuong ung voi nguoi choi
    [Tooltip("The prefab to use for representing the player")]
    public GameObject playerPrefab;

    // chuyen ve private serialize
    public GameObject[] spawnPositions;

    // chuyen ve private
    public int readyPlayers = 0;

    #endregion

    #region MonoBehaviour CallBacks
    // Region chua nhung ham CallBacks trong Unity

    private void Awake()
    {
        // Gan gia tri Instance bang script hien tai
        if (GameManager.Instance == null)
            Instance = this;

        gamePanel = GameObject.FindGameObjectWithTag("GamePanel");
        if (this.gamePanel == null)
        {
            Debug.LogError("Missing UI Game Panel", this);
            return;
        }
        gamePanel.SetActive(false);

        startingPanel = GameObject.FindGameObjectWithTag("StartingPanel");
        if (this.startingPanel == null)
        {
            Debug.LogError("Missing UI Starting Panel", this);
            return;
        }
        startingPanel.SetActive(true);

        spawnPositions = GameObject.FindGameObjectsWithTag("SpawnPosition");
        Array.Sort(spawnPositions, (a, b) =>
        {
            return a.name.CompareTo(b.name);
        });

        checkpointList = GameObject.FindGameObjectsWithTag("Checkpoint");
        Array.Sort(checkpointList, (a, b) =>
        {
            return Int32.Parse(a.name) - Int32.Parse(b.name);
        });

        playerInfos = new PlayerInfo[PhotonNetwork.PlayerList.Length];
    }

    // Ham nay duoc goi dau tien sau giai doan khoi tao Object
    private void Start()
    {
        // Kiem tra playerPrefab
        if (playerPrefab == null)
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> player's prefab reference");
        }
        else
        {
            // Chi tao instance tuong ung voi client khi chua co
            if (CarControl.LocalPlayerInstance == null)
            {
                Debug.LogFormat("Instantiating LocalPlayer from {0}", SceneManager.GetActiveScene().name);

                // Tao mot instance tuong ung voi nguoi choi
                CarControl.LocalPlayerInstance = SpawnPlayer();
                photonView.RPC("SetPlayer", RpcTarget.All);
                photonView.RPC("PlayerReady", RpcTarget.All);
                
                StartCoroutine(WaitUntilStart());
            }
            else
            {
                Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
            }
        }
    }

    #endregion

    #region Photon Callbacks
    // Region chua cac ham lien quan den server Photon
    // Override ham duoc goi khi thoat khoi phong choi
    public override void OnLeftRoom()
    {
        // Thoat khoi phong hien tai -> Ve Scene menu chinh
        SceneManager.LoadScene(0);
    }

    // Override ham duoc goi khi co nguoi choi tham gia phong
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        // Nhung nguoi khac ngoai tru nguoi choi dang ket noi co the nhan duoc thong bao
        Debug.LogFormat("OnPlayerEnteredRoom() {0}", newPlayer.NickName);

        // Neu client hien tai la chu phong
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerEnteredRoom() IsMasterClient {0}", PhotonNetwork.IsMasterClient);
        }
    }

    // Override ham duoc goi khi co nguoi choi roi phong
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // Thong bao ai la nguoi roi phong
        Debug.LogFormat("OnPlayerLeftRoom() {0}", otherPlayer.NickName);

        // Neu client hien tai la chu phong
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerLeftRoom() IsMasterClient {0}", PhotonNetwork.IsMasterClient);

            // Load man choi tuong ung voi so luong nguoi choi
            // LoadArena();
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // if (stream.IsWriting)
        // {
        //     stream.SendNext(this.isStart);
        // }
        // else
        // {
        //     this.isStart = (bool)stream.ReceiveNext();
        // }
    }

    #endregion

    #region Private Methods
    // Region chua nhung ham private
    // Ham xu ly su kien load man choi
    void LoadArena()
    {
        // Kiem tra client hien tai co phai chu phong khong
        if (!PhotonNetwork.IsMasterClient)
        {
            // Khong phai -> Bao loi
            Debug.LogError("PhotonNetwork: Trying to load a level but we are not the master Client");
            return;
        }

        Debug.LogFormat("PhotonNetwork: Loading level : {0}", PhotonNetwork.CurrentRoom.PlayerCount);
        // Load man choi tuong ung voi so luong nguoi choi
        PhotonNetwork.LoadLevel("Room for " + PhotonNetwork.CurrentRoom.PlayerCount);
    }

    void ReloadArena()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            // Khong phai -> Bao loi
            Debug.LogError("PhotonNetwork: Trying to reload a level but we are not the master Client");
            return;
        }

        Debug.Log("PhotonNetwork: Reload level");
        PhotonNetwork.LoadLevel(SceneManager.GetActiveScene().name);
    }

    GameObject SpawnPlayer()
    {
        return PhotonNetwork.Instantiate(this.playerPrefab.name,
                                          spawnPositions[PhotonNetwork.LocalPlayer.GetPlayerNumber()].transform.position,
                                          spawnPositions[PhotonNetwork.LocalPlayer.GetPlayerNumber()].transform.rotation, 0);
    }

    #endregion

    #region Public Methods
    // Region chua nhung ham public
    // Ham xu ly thoat khoi phong choi
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public int GetTotalLapNum()
    {
        return this.totalLaps;
    }

    public GameObject GetLapInfoText()
    {
        return this.gamePanel.transform.GetChild(0).gameObject;
    }

    public GameObject GetGameInfoText()
    {
        return this.gamePanel.transform.GetChild(1).gameObject;
    }

    public bool GetGameStatus()
    {
        return this.isStart;
    }

    public Vector3 GetCheckpointPosition(int index)
    {
        return this.checkpointList[index].transform.position;
    }

    public Quaternion GetCheckpointRotation(int index)
    {
        return this.checkpointList[index].transform.rotation;
    }

    public int GetTotalCheckpointNum()
    {
        return this.checkpointList.Length;
    }

    #endregion

    #region PunRPC Methods
    [PunRPC]
    void TurnOffWaitPanel()
    {
        startingPanel.SetActive(false);
        gamePanel.SetActive(true);
    }

    [PunRPC]
    public void PlayerReady()
    {
        this.readyPlayers += 1;
    }

    [PunRPC]
    public void SetStart()
    {
        // this.isStart = true;
        StartCoroutine(CountdownStart(3));
    }

    [PunRPC]
    public void SetPlayer()
    {
        playerInfos[PhotonNetwork.LocalPlayer.GetPlayerNumber()] = new PlayerInfo();
        // playerInfos[PhotonNetwork.LocalPlayer.GetPlayerNumber()].UpdateInfo();
    }

    #endregion

    #region  IEnumerator Methods
    IEnumerator WaitForOtherPlayers()
    {
        yield return new WaitUntil(() => this.readyPlayers == PhotonNetwork.PlayerList.Length);
        yield return new WaitForSeconds(1f);
        photonView.RPC("TurnOffWaitPanel", RpcTarget.All);
        photonView.RPC("SetStart", RpcTarget.All);
        // startingPanel.SetActive(false);
        // gamePanel.SetActive(true);
    }

    IEnumerator WaitUntilStart()
    {
        yield return new WaitUntil(() => isStart);
        // startingPanel.SetActive(false);
        // gamePanel.SetActive(true);
    }

    IEnumerator CountdownStart(int seconds)
    {
        int counter = seconds;
        while (counter > 0)
        {
            gamePanel.transform.GetChild(1).GetComponent<TMP_Text>().text = counter.ToString();
            yield return new WaitForSeconds(1);
            counter--;
        }

        this.isStart = true;
        gamePanel.transform.GetChild(1).GetComponent<TMP_Text>().text = "START";
        yield return new WaitForSeconds(3);
        gamePanel.transform.GetChild(1).gameObject.SetActive(false);
    }

    #endregion
}