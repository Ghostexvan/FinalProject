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
using System.Collections.Generic;

public class _GameManager : MonoBehaviourPunCallbacks, IPunObservable
{
    #region Private Serializable Fields
    [SerializeField]
    private int totalLaps;

    [SerializeField]
    private PlayerInfo[] playerInfos;

    #endregion

    #region Private Fields
    private bool isGameStart;
    private GameObject startingPanel;
    private GameObject gamePanel;
    private GameObject[] checkpointList;
    private GameObject[] spawnPositions;

    #endregion

    #region Public Fields
    public static _GameManager Instance;
    public GameObject playerPrefab;

    #endregion

    #region MonoBehaviour Callbacks
    private void Awake()
    {
        if (_GameManager.Instance == null)
        {
            Instance = this;
        }

        playerInfos = new PlayerInfo[PhotonNetwork.PlayerList.Length];

        gamePanel = GameObject.FindGameObjectWithTag("GamePanel");
        if (gamePanel == null)
        {
            Debug.LogError("Missing UI Game Panel", this);
            return;
        }

        startingPanel = GameObject.FindGameObjectWithTag("StartingPanel");
        if (startingPanel == null)
        {
            Debug.LogError("Missing UI Starting Panel", this);
            return;
        }

        spawnPositions = GameObject.FindGameObjectsWithTag("SpawnPosition");
        Array.Sort(spawnPositions, (a, b) =>
        {
            return Int32.Parse(a.name) - Int32.Parse(b.name);
        });

        checkpointList = GameObject.FindGameObjectsWithTag("Checkpoint");
        Array.Sort(checkpointList, (a, b) =>
        {
            return Int32.Parse(a.name) - Int32.Parse(b.name);
        });

        gamePanel.SetActive(false);
        startingPanel.SetActive(true);
    }

    // Start is called before the first frame update
    private void Start()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> player's prefab");
            return;
        }

        if (CarControl.LocalPlayerInstance == null)
        {
            Debug.LogFormat("Instantiating LocalPlayer (Player {0}) from {1}", PhotonNetwork.LocalPlayer.GetPlayerNumber(), SceneManager.GetActiveScene().name);

            // Spawn player
            CarControl.LocalPlayerInstance = SpawnPlayer();
            RegisterPlayer();
        }
        else
        {
            Debug.LogFormat("Ignoring scene load for {0}", SceneManager.GetActiveScene().name);
        }
    }

    // Update is called once per frame
    private void Update()
    {
        UpdatePlayerInfo();
    }

    #endregion

    #region MonoBehaviourPunCallbacks Callbacks
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.LogFormat("OnPlayerEnteredRoom() {0}", newPlayer.NickName);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.LogFormat("OnPlayerLeftRoom() {0}", otherPlayer.NickName);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // throw new NotImplementedException();
        if (PhotonNetwork.IsMasterClient && stream.IsWriting)
        {
            foreach (PlayerInfo playerInfo in playerInfos)
            {
                Debug.LogWarning("Player info send is null: " + (playerInfo == null));
                if (playerInfo != null)
                {
                    stream.SendNext(playerInfo.GetRank());
                    stream.SendNext(playerInfo.GetCurrentLap());
                    stream.SendNext(playerInfo.GetCurrentCheckpoint());
                    stream.SendNext(playerInfo.GetDistanceToNextCheckpoint());
                }
            }
        }
        else if (!PhotonNetwork.IsMasterClient && stream.IsReading)
        {
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                if (playerInfos[i] == null)
                {
                    playerInfos[i] = new PlayerInfo();
                }

                playerInfos[i].SetRank((int)stream.ReceiveNext());
                playerInfos[i].UpdateInfo((int)stream.ReceiveNext(),
                                          (int)stream.ReceiveNext(),
                                          (float)stream.ReceiveNext());
            }
        }
    }

    #endregion

    #region Private Methods
    private Vector3 GetSpawnPosition()
    {
        return spawnPositions[PhotonNetwork.LocalPlayer.GetPlayerNumber()].transform.position;
    }

    private Quaternion GetSpawnRotation()
    {
        return spawnPositions[PhotonNetwork.LocalPlayer.GetPlayerNumber()].transform.rotation;
    }

    private GameObject SpawnPlayer()
    {
        return PhotonNetwork.Instantiate(playerPrefab.name, GetSpawnPosition(), GetSpawnRotation());
    }

    private void RegisterPlayer()
    {
        // photonView.RPC("SetPlayer", RpcTarget.All, PhotonNetwork.LocalPlayer.GetPlayerNumber(), CarControl.LocalPlayerInstance);
        // photonView.RPC("SetPlayer", RpcTarget.All);
        photonView.RPC("SetPlayer", RpcTarget.MasterClient);
    }

    private void UpdatePlayerInfo()
    {
        Dictionary<string, float> playerData = new Dictionary<string, float>
        {
            { "currentLap", CarControl.LocalPlayerInstance.GetComponent<LapController>().GetCurrentLapNum() },
            { "currentCheckpoint", CarControl.LocalPlayerInstance.GetComponent<LapController>().GetCurrentCheckpoint() },
            { "distanceToNextCheckpoint", CarControl.LocalPlayerInstance.GetComponent<LapController>().GetDistanceToNextCheckpoint() }
        };

        object[] sendData = {
            CarControl.LocalPlayerInstance.GetComponent<LapController>().GetCurrentLapNum(),
            CarControl.LocalPlayerInstance.GetComponent<LapController>().GetCurrentCheckpoint(),
            CarControl.LocalPlayerInstance.GetComponent<LapController>().GetDistanceToNextCheckpoint()
        };

        photonView.RPC("SetPlayerInfo", RpcTarget.MasterClient, sendData as object);
    }

    #endregion

    #region Public Methods

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
        return this.isGameStart;
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

    #region IEnumerator Methods
    IEnumerator WaitForOtherPlayers()
    {
        yield return new WaitUntil(() =>
        {
            int numOfPlayerReady = 0;
            foreach (PlayerInfo playerInfo in playerInfos)
            {
                if (playerInfo.GetPlayerStatus())
                {
                    numOfPlayerReady++;
                }
            }
            return numOfPlayerReady == PhotonNetwork.PlayerList.Length;
        });
    }

    #endregion

    #region PunRPC Methods
    [PunRPC]
    public void SetPlayer()
    {
        playerInfos[PhotonNetwork.LocalPlayer.GetPlayerNumber()] = new PlayerInfo();
    }

    [PunRPC]
    public void SetPlayerInfo(object[] playerInfo, PhotonMessageInfo info)
    {
        if (playerInfos[info.Sender.GetPlayerNumber()] == null)
        {
            return;
        }

        playerInfos[info.Sender.GetPlayerNumber()].UpdateInfo(
            (int)playerInfo[0],
            (int)playerInfo[1],
            (float)playerInfo[2]
        );
    }

    #endregion
}
