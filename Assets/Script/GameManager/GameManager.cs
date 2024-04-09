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

using JSAM;

/// <summary>
/// Remember to include GameplaySoundHandler.cs and GameplayMusicHandler.cs in the game Scene.
/// You can check the Audio GameObjects to find them.
/// </summary>
public class GameManager : MonoBehaviourPunCallbacks, IPunObservable
{
    #region Private Serializable Fields
    [SerializeField]
    private int totalLaps;

    [SerializeField]
    private PlayerInfo[] playerInfos;

    [SerializeField]
    private double startTime = -1f;

    [SerializeField]
    private double endTime = -1f;

    [SerializeField]
    private GameplaySoundHandler gameplaySoundHandler;
    #endregion

    #region Private Fields
    private bool isGameStart;

    // This one is used to check whether the race has finished overall, is used as flag for audios mostly
    private bool isAllPlayersFinished;

    private bool isCountdownEnd = false;
    private GameObject startingPanel;
    private GameObject gamePanel;
    private GameObject endPanel;
    private GameObject[] checkpointList;
    private GameObject[] spawnPositions;

    #endregion

    #region Public Fields
    public static GameManager Instance;
    public LocalPlayerData playerData;

    //[HideInInspector]
    //public bool isGameStart;
    //[HideInInspector]
    //public bool isAllPlayersFinished;
    #endregion

    #region MonoBehaviour Callbacks
    private void Awake()
    {
        if (GameManager.Instance == null)
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

        endPanel = GameObject.FindGameObjectWithTag("EndPanel");
        if (endPanel == null)
        {
            Debug.LogError("Missing UI Ending Panel", this);
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

        endPanel.SetActive(false);
        gamePanel.SetActive(false);
        startingPanel.SetActive(true);

        this.startTime = -1f;
        this.endTime = -1f;

        /// Getting the GameplaySoundHandler.
        // Since SFX Handler GameObject is active the whole time, its activeIH is always true.
        // meaning I can use GameObject.Find to look for it.
        gameplaySoundHandler = GameObject.Find("SFX Handler").GetComponent<GameplaySoundHandler>();

        // Initialize isAllPlayersFinished flag (I mean I can just use endPanel GameObject but it wouldn't feel right)
        isAllPlayersFinished = false;
    }

    // Start is called before the first frame update
    private void Start()
    {
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
        // Loads the Launcher scene upon exiting Room
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
        // Leaving the calculations to the Master Client
        if (PhotonNetwork.IsMasterClient)
        {
            // Calculating each players' infos (Rank, Laps, Distance and Finish Time)
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                stream.SendNext(i);
                stream.SendNext(playerInfos[i].GetRank());
                stream.SendNext(playerInfos[i].GetCurrentLap());
                stream.SendNext(playerInfos[i].GetCurrentLap());
                stream.SendNext(playerInfos[i].GetDistanceToNextCheckpoint());
                stream.SendNext(playerInfos[i].GetFinishTime());
            }
        }
        else if (!PhotonNetwork.IsMasterClient)
        {
            // If player is not Master Client --> Receives the calculated infos
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                int index = (int)stream.ReceiveNext();
                playerInfos[index].SetRank((int)stream.ReceiveNext());
                playerInfos[index].UpdateInfo((int)stream.ReceiveNext(),
                                              (int)stream.ReceiveNext(),
                                              (float)stream.ReceiveNext());
                playerInfos[index].SetFinishTime((double)stream.ReceiveNext());
            }
        }
    }

    public double GetStartTime()
    {
        return this.startTime;
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
        return PhotonNetwork.Instantiate("Player/" + playerData.GetPlayerPrefabName(), GetSpawnPosition(), GetSpawnRotation());
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

    private int GetPlayerFinish()
    {
        int result = 0;
        for (int index = 0; index < PhotonNetwork.PlayerList.Length; index++)
        {
            if (playerInfos[index].GetFinishTime() != -1f)
            {
                result++;
            }
        }
        return result;
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

    public bool IsAllPlayersFinished()
    {
        return this.isAllPlayersFinished;
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

    public void SetLocalPlayerFinish(double finishTime)
    {
        photonView.RPC("FinishRace", RpcTarget.All, finishTime);
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

            /// Play Countdown Sound
            gameplaySoundHandler.PlayCountdownSFX();
            ///

            yield return new WaitForSeconds(1);
            counter--;
        }

        this.isGameStart = true;
        /// Play Start Sound
        /// Adding the if to prevent this from being repeatedly played
        if (!AudioManager.IsSoundPlaying(GameplaySounds.SE_RC_GO))
            gameplaySoundHandler.PlayGoSFX();
        /// 


        gamePanel.transform.GetChild(1).GetComponent<TMP_Text>().text = "START";
        yield return new WaitForSeconds(3);
        gamePanel.transform.GetChild(1).gameObject.SetActive(false);

        if (PhotonNetwork.IsMasterClient)
        {
            startTime = PhotonNetwork.Time;
            photonView.RPC("SetStartTime", RpcTarget.Others, startTime);
        }

        // StartCoroutine(CountdownEnd(3, "Test"));
    }

    IEnumerator CountdownEnd(int seconds, string playerName)
    {
        ///// If one player finishes the race, CountdownEnd will be called from GameManager singleton.
        /// This will only be called ONCE, since it kinda determines when the game ends as a whole
        if (isCountdownEnd)
        {
            yield break;
        }
        isCountdownEnd = true;

        /// If Local Player had already finished the race (isReady had already turned from True to False again)
        /// --> endPanel active
        if (!playerInfos[PhotonNetwork.LocalPlayer.GetPlayerNumber()].GetPlayerStatus())
        {
            gamePanel.SetActive(false);
            endPanel.SetActive(true);
        }
        /// Else, Countdown Panel would be active instead
        else
        {
            // This is where the Countdown UI panel is: GamePanel->GameInfo
            gamePanel.transform.GetChild(1).gameObject.SetActive(true);

            /// Plays Ending Countdown
            gameplaySoundHandler.PlayStartEndingCountdown();
            ///
        }

        int counter = seconds;
        while (counter > 0)
        {
            if (this.GetPlayerFinish() == PhotonNetwork.PlayerList.Length)
            {
                break;
            }

            /// During the Countdown Period, if Local Player actually managed to Finish the race
            /// --> EndPanel to true
            if (playerInfos[PhotonNetwork.LocalPlayer.GetPlayerNumber()].GetFinishTime() != -1f)
            {
                if (gamePanel.activeSelf)
                {
                    gamePanel.SetActive(false);
                    endPanel.SetActive(true);
                    CarControl.LocalPlayerInstance.GetComponent<CarControl>().StopEngine();
                }

                endPanel.transform.GetChild(0).GetComponent<TMP_Text>().text = "You have finished the race!\n";
                endPanel.transform.GetChild(0).GetComponent<TMP_Text>().text += "Your rank: " + playerInfos[PhotonNetwork.LocalPlayer.GetPlayerNumber()].GetRank();
                yield return new WaitForSeconds(1);
                counter--;
                continue;
            }

            gamePanel.transform.GetChild(1).GetComponent<TMP_Text>().text = playerName + " has finish the race!\n";
            gamePanel.transform.GetChild(1).GetComponent<TMP_Text>().text += "Time left to finish: ";
            gamePanel.transform.GetChild(1).GetComponent<TMP_Text>().text += counter.ToString();
            yield return new WaitForSeconds(1);
            counter--;
        }


        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("SetEndTime", RpcTarget.All, PhotonNetwork.Time);
        }

        /// Final Countdown Loop ended, all cars will stop moving
        /// And isGameStart will be false
        StopAllMoving();

        /// If Ending Countdown SFX is playing, end it
        /// Line 507 is where it's played
        AudioManager.StopSoundIfPlaying(MainGameSounds.SE_RC_START_ENDING_COUNTDOWN);
        ///

        ////// Since all players have either crossed the finish or have not made it to the finish line in time, we set the flag
        /// This will mainly be used for changing the BGM (see IsAllPlayersFinished() in Public methods)
        isAllPlayersFinished = true;


        gamePanel.SetActive(false);
        endPanel.SetActive(true);
        endPanel.transform.GetChild(0).GetComponent<TMP_Text>().text = "<size=200%><b>GAME OVER!</b>";
        yield return new WaitUntil(() => this.endTime != -1f && PhotonNetwork.Time - this.endTime >= 5.0f);

        ///// This here displays the results screen with the rank

        /// Get each players' rank index
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

        /// Display rank index
        endPanel.transform.GetChild(0).GetComponent<TMP_Text>().text = "<align=center><size=100%><b>GAME OVER!</b></align>\n";
        for (int index = 0; index < PhotonNetwork.PlayerList.Length; index++)
        {
            endPanel.transform.GetChild(0).GetComponent<TMP_Text>().text += "<size=80%><i>" + (index + 1) + ". " + PhotonNetwork.PlayerList[rankIndex[index]].NickName
                                                                                 + "<pos=75%>";
            if (playerInfos[rankIndex[index]].GetFinishTime() == -1)
            {
                endPanel.transform.GetChild(0).GetComponent<TMP_Text>().text += TimeSpan.FromSeconds(this.endTime - this.startTime).ToString("mm':'ss':'ff") + "\n";
            }
            else
            {
                endPanel.transform.GetChild(0).GetComponent<TMP_Text>().text += TimeSpan.FromSeconds(playerInfos[rankIndex[index]].GetFinishTime() - this.startTime).ToString("mm':'ss':'ff") + "\n";
            }
        }

        /// Wait a set amount of time for players to view the results screen, then load the Launcher scene
        yield return new WaitForSeconds(10f);       /// Increased to 10s, giving players more time to check the results screen
        SceneManager.LoadScene("Launcher");
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
    public void FinishRace(double timeFinished, PhotonMessageInfo info)
    {
        playerInfos[info.Sender.GetPlayerNumber()].SetReady();
        playerInfos[info.Sender.GetPlayerNumber()].SetFinishTime(timeFinished);
        StartCoroutine(CountdownEnd(10, info.Sender.NickName));
    }

    [PunRPC]
    public void SetStartTime(double startTime)
    {
        this.startTime = startTime;
    }

    [PunRPC]
    public void SetEndTime(double endTime)
    {
        this.endTime = endTime;
    }

    #endregion
}
