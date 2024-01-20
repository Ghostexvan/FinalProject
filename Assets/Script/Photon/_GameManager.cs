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

public class _GameManager : MonoBehaviourPunCallbacks, IPunObservable
{
    #region Private Serializable Fields
    [SerializeField]
    private int totalLaps;

    [SerializeField]
    private PlayerInfo[] playerInfos;

    [SerializeField]
    private float startTime;

    #endregion

    #region Private Fields
    private bool isGameStart;
    private bool isCountdownEnd = false;
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
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            playerInfos[i] = new PlayerInfo();
        }

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

        StartCoroutine(WaitForOtherPlayers());
    }

    // Update is called once per frame
    private void Update()
    {
        // startingPanel.transform.GetChild(0).GetComponent<TMP_Text>().text = "Rank " + playerInfos[PhotonNetwork.LocalPlayer.GetPlayerNumber()].GetRank();
        UpdatePlayerInfo();
        RankCalculate();
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
        if (PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                stream.SendNext(i);
                stream.SendNext(playerInfos[i].GetRank());
                stream.SendNext(playerInfos[i].GetCurrentLap());
                stream.SendNext(playerInfos[i].GetCurrentLap());
                stream.SendNext(playerInfos[i].GetDistanceToNextCheckpoint());
            }
        }
        else if (!PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                int index = (int)stream.ReceiveNext();
                playerInfos[index].SetRank((int)stream.ReceiveNext());
                playerInfos[index].UpdateInfo((int)stream.ReceiveNext(),
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
        photonView.RPC("SetPlayer", RpcTarget.All);
    }

    private void UpdatePlayerInfo()
    {
        object[] sendData = {
            CarControl.LocalPlayerInstance.GetComponent<LapController>().GetCurrentLapNum(),
            CarControl.LocalPlayerInstance.GetComponent<LapController>().GetCurrentCheckpoint(),
            CarControl.LocalPlayerInstance.GetComponent<LapController>().GetDistanceToNextCheckpoint()
        };

        if (!CarControl.LocalPlayerInstance.GetComponent<LapController>().GetCountStatus() &&
            (int)sendData[0] == 1)
        {
            sendData[0] = -1;
        }

        photonView.RPC("SetPlayerInfo", RpcTarget.MasterClient, sendData as object);
    }

    private void FillRank()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        int rankNum = 1;
        foreach (PlayerInfo playerInfo in playerInfos)
        {
            playerInfo.SetRank(rankNum);
            rankNum++;
        }
    }

    private void RankCalculate()
    {
        if (!PhotonNetwork.IsMasterClient || !isGameStart)
        {
            return;
        }

        for (int firstLoop = 0; firstLoop < PhotonNetwork.PlayerList.Length - 1; firstLoop++)
        {
            for (int secondLoop = firstLoop + 1; secondLoop < PhotonNetwork.PlayerList.Length; secondLoop++)
            {
                if (!playerInfos[firstLoop].GetPlayerStatus() || !playerInfos[secondLoop].GetPlayerStatus())
                {
                    continue;
                }

                if (playerInfos[firstLoop].GetCurrentLap() != playerInfos[secondLoop].GetCurrentLap())
                {
                    if (playerInfos[firstLoop].GetCurrentLap() > playerInfos[secondLoop].GetCurrentLap() &&
                        playerInfos[firstLoop].GetRank() > playerInfos[secondLoop].GetRank())
                    {
                        Debug.LogWarning("Swap rank of player " + firstLoop + " and " + secondLoop + " due to lap");
                        int rankSwap = playerInfos[firstLoop].GetRank();
                        playerInfos[firstLoop].SetRank(playerInfos[secondLoop].GetRank());
                        playerInfos[secondLoop].SetRank(rankSwap);
                    }
                    else if (playerInfos[firstLoop].GetCurrentLap() < playerInfos[secondLoop].GetCurrentLap() &&
                             playerInfos[firstLoop].GetRank() < playerInfos[secondLoop].GetRank())
                    {
                        Debug.LogWarning("Swap rank of player " + firstLoop + " and " + secondLoop + " due to lap");
                        int rankSwap = playerInfos[firstLoop].GetRank();
                        playerInfos[firstLoop].SetRank(playerInfos[secondLoop].GetRank());
                        playerInfos[secondLoop].SetRank(rankSwap);
                    }

                    continue;
                }
                else if (playerInfos[firstLoop].GetCurrentCheckpoint() != playerInfos[secondLoop].GetCurrentCheckpoint())
                {
                    if (playerInfos[firstLoop].GetCurrentCheckpoint() > playerInfos[secondLoop].GetCurrentCheckpoint() &&
                        playerInfos[firstLoop].GetRank() > playerInfos[secondLoop].GetRank())
                    {
                        Debug.LogWarning("Swap rank of player " + firstLoop + " and " + secondLoop + " due to checkpoint");
                        int rankSwap = playerInfos[firstLoop].GetRank();
                        playerInfos[firstLoop].SetRank(playerInfos[secondLoop].GetRank());
                        playerInfos[secondLoop].SetRank(rankSwap);
                    }
                    else if (playerInfos[firstLoop].GetCurrentCheckpoint() < playerInfos[secondLoop].GetCurrentCheckpoint() &&
                             playerInfos[firstLoop].GetRank() < playerInfos[secondLoop].GetRank())
                    {
                        Debug.LogWarning("Swap rank of player " + firstLoop + " and " + secondLoop + " due to checkpoint");
                        int rankSwap = playerInfos[firstLoop].GetRank();
                        playerInfos[firstLoop].SetRank(playerInfos[secondLoop].GetRank());
                        playerInfos[secondLoop].SetRank(rankSwap);
                    }

                    continue;
                }
                else
                {
                    if (playerInfos[firstLoop].GetDistanceToNextCheckpoint() > playerInfos[secondLoop].GetDistanceToNextCheckpoint() &&
                        playerInfos[firstLoop].GetRank() < playerInfos[secondLoop].GetRank())
                    {
                        Debug.LogWarning("Swap rank of player " + firstLoop + " and " + secondLoop + " due to distance");
                        int rankSwap = playerInfos[firstLoop].GetRank();
                        playerInfos[firstLoop].SetRank(playerInfos[secondLoop].GetRank());
                        playerInfos[secondLoop].SetRank(rankSwap);
                    }
                    else if (playerInfos[firstLoop].GetDistanceToNextCheckpoint() < playerInfos[secondLoop].GetDistanceToNextCheckpoint() &&
                             playerInfos[firstLoop].GetRank() > playerInfos[secondLoop].GetRank())
                    {
                        Debug.LogWarning("Swap rank of player " + firstLoop + " and " + secondLoop + " due to distance");
                        int rankSwap = playerInfos[firstLoop].GetRank();
                        playerInfos[firstLoop].SetRank(playerInfos[secondLoop].GetRank());
                        playerInfos[secondLoop].SetRank(rankSwap);
                    }
                }
            }
        }
    }

    private void TurnOffWaitPanel()
    {
        startingPanel.SetActive(false);
        gamePanel.SetActive(true);
    }

    private void StopAllMoving()
    {
        this.isGameStart = false;
        CarControl.LocalPlayerInstance.GetComponent<CarControl>().StopEngine();
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

    public void SetLocalPlayerFinish()
    {
        photonView.RPC("FinishRace", RpcTarget.All);
    }

    public int GetLocalPlayerRank()
    {
        return playerInfos[PhotonNetwork.LocalPlayer.GetPlayerNumber()].GetRank();
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

        FillRank();
        TurnOffWaitPanel();
        StartCoroutine(CountdownStart(3));
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

        this.isGameStart = true;
        gamePanel.transform.GetChild(1).GetComponent<TMP_Text>().text = "START";
        yield return new WaitForSeconds(3);
        gamePanel.transform.GetChild(1).gameObject.SetActive(false);
        startTime = Time.timeSinceLevelLoad;

        // StartCoroutine(CountdownEnd(3, "Test"));
    }

    IEnumerator CountdownEnd(int seconds, string playerName)
    {
        if (isCountdownEnd)
        {
            yield break;
        }
        isCountdownEnd = true;

        gamePanel.transform.GetChild(1).gameObject.SetActive(true);

        int counter = seconds;
        while (counter > 0)
        {
            if (!playerInfos[PhotonNetwork.LocalPlayer.GetPlayerNumber()].GetPlayerStatus())
            {
                gamePanel.transform.GetChild(1).GetComponent<TMP_Text>().text = "You have finished the race!\n";
                gamePanel.transform.GetChild(1).GetComponent<TMP_Text>().text += "Your rank: " + playerInfos[PhotonNetwork.LocalPlayer.GetPlayerNumber()].GetRank();
                counter--;
                continue;
            }

            gamePanel.transform.GetChild(1).GetComponent<TMP_Text>().text = playerName + " has finish the race!\n";
            gamePanel.transform.GetChild(1).GetComponent<TMP_Text>().text += "Time left to finish: ";
            gamePanel.transform.GetChild(1).GetComponent<TMP_Text>().text += counter.ToString();
            yield return new WaitForSeconds(1);
            counter--;
        }

        StopAllMoving();

        gamePanel.transform.GetChild(1).GetComponent<TMP_Text>().text = "<size=200%><b>GAME OVER!</b>";
        yield return new WaitForSeconds(5.0f);

        gamePanel.SetActive(false);
        startingPanel.SetActive(true);

        int[] rankIndex = new int[4];
        int currentRank = 1;
        while (currentRank <= PhotonNetwork.PlayerList.Length)
        {
            for (int index = 0; index < PhotonNetwork.PlayerList.Length; index++)
            {
                if (playerInfos[index].GetRank() == currentRank)
                {
                    rankIndex[currentRank - 1] = index;
                    currentRank++;
                    break;
                }
            }
        }

        // while (true)
        // {
        startingPanel.transform.GetChild(0).GetComponent<TMP_Text>().text = "<align=center><size=100%><b>GAME OVER!</b></align>\n";
        for (int index = 0; index < PhotonNetwork.PlayerList.Length; index++)
        {
            startingPanel.transform.GetChild(0).GetComponent<TMP_Text>().text += "<size=80%><i>" + (index + 1) + ". " + PhotonNetwork.PlayerList[rankIndex[index]].NickName
                                                                                 + "\n";
            // if (playerInfos[rankIndex[index]].GetFinishTime() == -1) {
            //     startingPanel.transform.GetChild(0).GetComponent<TMP_Text>().text += TimeSpan.FromSeconds.ToString("mm':'ss':'ff");
            // }
        }
        // }
    }

    #endregion

    #region PunRPC Methods
    [PunRPC]
    public void SetPlayer(PhotonMessageInfo info)
    {
        playerInfos[info.Sender.GetPlayerNumber()].SetReady();
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

    [PunRPC]
    public void FinishRace(PhotonMessageInfo info)
    {
        playerInfos[info.Sender.GetPlayerNumber()].SetReady();
        playerInfos[info.Sender.GetPlayerNumber()].SetFinishTime();
        StartCoroutine(CountdownEnd(10, info.Sender.NickName));
    }

    #endregion
}
