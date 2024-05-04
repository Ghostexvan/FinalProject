// Chua comment
using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;

// Quan ly va ghi nhan thong tin cua cac vong dua
// Su dung PUN va IPunObservable de dong bo (Dang nghien cuu them, chua ro)
public class LapController : MonoBehaviourPunCallbacks, IPunObservable
{
    #region Private Serializable Fields
    // Region chua nhung truong private co the serialize
    // Serialize co the hieu don gian la cac truong co the duoc hien thi trong tab Inspector trong Unity
    // Thong tin cac vong dua cua nguoi choi
    [SerializeField]
    private List<LapInfo> lapInfos = new List<LapInfo>();

    // Checkpoint tiep theo cua nguoi choi
    [SerializeField]
    private int nextCheckpoint = 0;

    // Component GameplaySoundHandler
    [SerializeField]
    private GameplaySoundHandler gameplaySoundHandler;

    #endregion

    #region Private Fields
    // Region chua nhung truong private
    // GameObject hien thi thong tin vong dua
    // Bao gom: Vong dua hien tai, thoi gian hoan thanh cua cac vong
    private GameObject lapInfoText;

    // GameObject hien thi thong bao cua game nhu dem nguoc, bat dau, canh bao...
    private GameObject gameInfoText;

    // Kiem tra co bat dau tinh vong dua chua
    private bool startCount = false;

    // Khoang cach toi checkpoint tiep theo cua nguoi choi
    private float distanceToNextCheckpoint;

    // Thoi gian ma nguoi choi bat dau di sai huong
    // Mac dinh: -1 la khong co
    private double timeStartGoingWrongDirection = -1f;

    // Thoi gian ma nguoi choi bat dau di dung huong sau khi sai huong
    // Mac dinh: -1 la khong co
    private double timeStartGoingRightDirection = -1f;

    // Kiem tra nguoi choi co vua dat duoc checkpoint moi
    private bool isEnterNewCheckpoint;

    // Kiem tra nguoi choi co dang chuan bi reset vi tri
    private bool isResettingPosition;

    // Kiểm tra Lap có phải Lap cuối cùng chưa, được sử dụng trong GameplayMusicHandler.cs
    public bool isFinalLap;

    public bool hardReset = false;

    #endregion

    #region MonoBehaviour Callbacks
    // Region chua nhung ham CallBacks trong Unity
    // Ham nay duoc goi trong giai doan khoi tao Object
    private void Awake()
    {       
        // Lay cac GameObject can thiet, neu khong co -> Bao loi
        this.lapInfoText = GameManager.Instance.GetLapInfoText();
        if (this.lapInfoText == null)
        {
            Debug.LogError("Missing UI Lap Info", this);
            return;
        }

        this.gameInfoText = GameManager.Instance.GetGameInfoText();
        if (this.gameInfoText == null)
        {
            Debug.LogError("Missing UI Game Info", this);
            return;
        }

        // Dat cac gia tri mac dinh
        this.timeStartGoingRightDirection = -1f;
        this.timeStartGoingWrongDirection = -1f;
        this.isEnterNewCheckpoint = false;

        /// Getting the GameplaySoundHandler.
        // Since SFX Handler GameObject is active the whole time, its activeIH is always true.
        // meaning I can use GameObject.Find to look for it.
        gameplaySoundHandler = GameObject.Find("SFX Handler").GetComponent<GameplaySoundHandler>();

        /// Init isFinalLap, though it's not really necessary really
        isFinalLap = false;
    }

    // Ham nay duoc goi dau tien sau giai doan khoi tao Object
    void Start()
    {
        // Doi den khi game bat dau
        StartCoroutine(WaitUntilGameStart());
    }

    // Ham nay duoc goi moi frame mot lan
    void Update()
    {
        // Xac dinh khoang cach den checkpoint tiep theo
        DetermineDistanceToNextCheckpoint();

        // Hien thi thong tin vong dua
        DisplayLapInfo();

        if (!PhotonNetwork.LocalPlayer.IsLocal && hardReset){
            StartCoroutine(ResetPosition(0.1f));
        }
    }

    // Ham nay duoc goi moi khi nguoi choi vao mot collider trigger
    private void OnTriggerEnter(Collider other)
    {
        // Neu trigger vua vao la mot checkpoint
        if (other.tag == "Checkpoint")
        {
            // Lay so checkpoint
            int checkpointNum = Int32.Parse(other.gameObject.name);

            // Ghi nhan va dat checkpoint moi cho nguoi choi
            SetLap(checkpointNum);
        }
    }

    public void ResetPositionControl(){
        hardReset = true;
        StartCoroutine(ResetPosition(0.1f));
    }

    #endregion

    #region MonoBehaviourPunCallbacks Callbacks
    // Region nay chua nhung ham duoc goi de dong bo
    // Dong bo cac du lieu giua client va master
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(lapInfos.Count);
            stream.SendNext(hardReset);

            foreach (LapInfo lapInfo in lapInfos)
            {
                stream.SendNext(lapInfo.GetLapNum());
                stream.SendNext(lapInfo.GetTimeStarted());
                stream.SendNext(lapInfo.GetTimeFinished());
            }

            stream.SendNext(nextCheckpoint);
            stream.SendNext(distanceToNextCheckpoint);
        }
        else
        {
            int lapInfoCount = (int)stream.ReceiveNext();
            hardReset = (bool)stream.ReceiveNext();

            for (int i = 0; i < lapInfoCount; i++)
            {
                if (i >= lapInfos.Count)
                {
                    lapInfos.Add(new LapInfo((int)stream.ReceiveNext(),
                                             (double)stream.ReceiveNext()));
                    lapInfos[i].SetTimeFinished((double)stream.ReceiveNext());
                }
                else
                {
                    stream.ReceiveNext();
                    stream.ReceiveNext();

                    double timeFinished = (double)stream.ReceiveNext();
                    if (timeFinished != lapInfos[i].GetTimeFinished())
                    {
                        lapInfos[i].SetTimeFinished(timeFinished);
                    }
                }
            }

            this.nextCheckpoint = (int)stream.ReceiveNext();
            this.distanceToNextCheckpoint = (float)stream.ReceiveNext();
        }
    }

    #endregion

    #region Public Methods
    // Region nay chua nhung ham public
    // Lay so vong dua cua nguoi choi nay
    public int GetCurrentLapNum()
    {
        return this.lapInfos.Count;
    }

    // Lay checkpoint hien tai ma nguoi choi nay dat duoc
    public int GetCurrentCheckpoint()
    {
        return this.nextCheckpoint == 0 ? GameManager.Instance.GetTotalCheckpointNum() - 1 : this.nextCheckpoint - 1;
    }

    // Lay khoang cach den checkpoint tiep theo cua nguoi choi nay
    public float GetDistanceToNextCheckpoint()
    {
        return this.distanceToNextCheckpoint;
    }

    public bool GetCountStatus()
    {
        return this.startCount;
    }

    public bool IsFinalLap()
    {
        return this.isFinalLap;
    }

    #endregion

    #region Private Methods
    // Region nay chua cac ham private
    // Hien thi thong tin vong dua
    private void DisplayLapInfo()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        // Lay text tu GameObject lien quan de chinh sua
        TMP_Text text = lapInfoText.GetComponent<TMP_Text>();

        // Neu hien tai chua co thong tin gi -> Khong hien thi
        if (lapInfos.Count == 0)
        {
            text.text = "";
            return;
        }

        text.text = "<i>Pos: <pos=25%><size=200%><b>" + GameManager.Instance.GetLocalPlayerRank() + "</b><size=100%>/" + PhotonNetwork.PlayerList.Length + "</i>\n";

        // Hien thi vong dua hien tai cua nguoi choi
        text.text += "<i>Lap: <pos=25%><size=200%><b>" + Math.Min(lapInfos[lapInfos.Count - 1].GetLapNum(), GameManager.Instance.GetTotalLapNum()) + "</b><size=100%>/" + GameManager.Instance.GetTotalLapNum() + "</i><line-height=200%>\n";

        text.text += "<size=150%><i>";

        // Neu vong dua chua hoan thanh
        if (lapInfos[lapInfos.Count - 1].GetTimeFinished() == 0 && lapInfos.Count <= GameManager.Instance.GetTotalLapNum())
        {
            // Hien thi thoi gian tinh tu luc bat dau vong dua den hien tai
            text.text += TimeSpan.FromSeconds(PhotonNetwork.Time - lapInfos[lapInfos.Count - 1].GetTimeStarted()).ToString("mm':'ss':'ff");
        }
        // Neu vong dua da hoan thanh
        else
        {
            // Hien thi thoi gian hoan thanh vong dua
            text.text += TimeSpan.FromSeconds(lapInfos[GameManager.Instance.GetTotalLapNum() - 1].GetTimeFinished() - lapInfos[GameManager.Instance.GetTotalLapNum() - 1].GetTimeStarted()).ToString("mm':'ss':'ff");
        }

        text.text += "</i>";

    }

    // Xac dinh khoang cach cua nguoi choi den checkpoint tiep theo
    private void DetermineDistanceToNextCheckpoint()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        // Neu game chua bat dau
        if (!GameManager.Instance.GetGameStatus())
        {
            // Debug.Log("1 " + this.distanceToNextCheckpoint);

            // Chi tinh khoang cach ma khong lam gi ca
            this.distanceToNextCheckpoint = Vector3.Distance(this.transform.position,
                                                   GameManager.Instance.GetCheckpointPosition(this.nextCheckpoint));
            return;
        }

        // Tinh khoang cach moi tu nguoi choi den checkpoint tiep theo
        // Debug.Log("2 " + this.distanceToNextCheckpoint);
        float distanceCaculated = Vector3.Distance(this.transform.position,
                                                   GameManager.Instance.GetCheckpointPosition(this.nextCheckpoint));

        // Neu nguoi choi dang reset vi tri hoac vua vao mot checkpoint moi
        if (isEnterNewCheckpoint || isResettingPosition)
        {
            // Huy bo moi su kiem tra
            this.timeStartGoingRightDirection = -1f;
            this.timeStartGoingWrongDirection = -1f;

            // Huy bo xong -> Khong con vua vao checkpoint moi nua
            this.isEnterNewCheckpoint = false;

            // Gan khoang cach cu bang khoang cach moi (do vua thay doi vi tri)
            this.distanceToNextCheckpoint = distanceCaculated;
            return;
        }

        // Neu khoang cach moi lon hon khoang cach cu -> Nghi ngo nguoi choi di nguoc chieu
        // 0.01f la sai so cho phep
        if (distanceCaculated > this.distanceToNextCheckpoint + 0.01f)
        {
            // Neu chua dat moc thoi gian kiem tra di nguoc chieu
            if (this.timeStartGoingWrongDirection == -1f)
            {
                Debug.LogWarning("Player going in wrong direction");
                // Dat moc thoi gian di nguoc chieu
                this.timeStartGoingWrongDirection = PhotonNetwork.Time;
            }

            // Neu da dat moc thoi gian kiem tra di dung chieu
            if (this.timeStartGoingRightDirection != -1f)
            {
                // Luc nay, nguoi choi da tiep tuc di nguoc chieu -> Huy bo moc thoi gian di dung chieu
                this.timeStartGoingRightDirection = -1f;
            }

            // Neu thoi gian kiem tra di nguoc chieu da qua thoi gian quy dinh
            if (Time.timeSinceLevelLoad - this.timeStartGoingWrongDirection >= 5.0f)
            {
                // Reset vi tri nguoi choi
                StartCoroutine(ResetPosition(3.0f));

                // Huy bo moc thoi gian di nguoc chieu
                this.timeStartGoingWrongDirection = -1f;

                // Tinh lai khoang cach do vua thay doi vi tri
                distanceCaculated = Vector3.Distance(this.transform.position,
                                                   GameManager.Instance.GetCheckpointPosition(this.nextCheckpoint));
            }
            // Neu dang nghi ngo nguoi choi di nguoc chieu
            // va khoang cach moi khong doi hoac nho hon so voi khoang cach cu 
            // -> Nghi ngo nguoi choi dang di dung chieu lai
        }
        else if (distanceCaculated != this.distanceToNextCheckpoint + 0.01f && this.timeStartGoingWrongDirection != -1f)
        {
            // Neu chua dat moc thoi gian kiem tra di dung chieu
            if (this.timeStartGoingRightDirection == -1f)
            {
                // Dat moc thoi gian di dung chieu
                this.timeStartGoingRightDirection = PhotonNetwork.Time;
            }

            // Neu thoi gian di nguoc chieu da qua thoi gian quy dinh
            // (Tăng thời gian đi ngược chiều từ 5s --> 7s)
            if (PhotonNetwork.Time - this.timeStartGoingWrongDirection >= 7.0f)
            {
                // Reset vi tri nguoi choi
                StartCoroutine(ResetPosition(3.0f));

                // Huy bo moc thoi gian di nguoc chieu
                this.timeStartGoingWrongDirection = -1f;

                // Huy bo moc thoi gian di dung chieu
                this.timeStartGoingRightDirection = -1f;

                // Tinh lai khoang cach do vua thay doi vi tri
                distanceCaculated = Vector3.Distance(this.transform.position,
                                                   GameManager.Instance.GetCheckpointPosition(this.nextCheckpoint));
            }
            // Neu thoi gian di dung chieu da qua thoi gian quy dinh
            else if (PhotonNetwork.Time - this.timeStartGoingRightDirection >= 3.0f)
            {
                // Nguoi choi da quay lai dung chieu duong dua
                Debug.LogWarning("Player return to right direction");

                // Huy bo moc thoi gian di nguoc chieu
                this.timeStartGoingWrongDirection = -1f;

                // Huy bo moc thoi gian di dung chieu
                this.timeStartGoingRightDirection = -1f;
            }
        }

        // Gan khoang cach cu bang khoang cach moi
        this.distanceToNextCheckpoint = distanceCaculated;
    }

    // Dat checkpoint moi cho nguoi choi
    private void SetLap(int checkpointNum)
    {
        if (!photonView.IsMine)
        {
            return;
        }

        // Neu nhu game chua duoc bat dau
        // hoac chua bat dau viec ghi nhan cac vong dua
        // -> Khong thuc hien
        if (!GameManager.Instance.GetGameStatus() && !startCount)
        {
            return;
        }

        // Chi tinh khi checkpoint nhan vao bang checkpoint du kien
        if (checkpointNum == this.nextCheckpoint)
        {
            // Ghi nhan checkpoint tiep theo cho nguoi choi
            this.nextCheckpoint = checkpointNum == GameManager.Instance.GetTotalCheckpointNum() - 1 ?
                                  0 :
                                  this.nextCheckpoint + 1;

            // Danh dau vua vao mot checkpoint moi
            this.isEnterNewCheckpoint = true;

            // Neu checkpoint vua ma nguoi choi vua dat duoc la checkpoint bat dau
            if (checkpointNum == 0)
            {
                // Neu chua bat dau ghi nhan
                if (!startCount)
                {
                    // Danh dau la bat dau ghi nhan
                    // -> Khong thuc hien them gi nua
                    // Vi o vong dua dau tien, chung ta bat dau tinh thoi gian tu luc co hieu lenh xuat phat
                    // (tro choi bat dau)
                    startCount = true;
                    return;
                }

                // Bo sung thoi gian ket thuc cho vong dua hien tai
                lapInfos[lapInfos.Count - 1].SetTimeFinished(PhotonNetwork.Time);

                // Them vong dua moi
                lapInfos.Add(new LapInfo(lapInfos.Count + 1, PhotonNetwork.Time));

                /// Ở đây ta lấy lapInfos.Count sau khi đã cộng, vì ta chỉ PlaySound khi ta qua Lap mới
                /// Play Lap Passed sound
                if (lapInfos.Count < GameManager.Instance.GetTotalLapNum())
                    gameplaySoundHandler.PlayPassedLap();
                /// Neu Lap la Lap cuoi cung --> Play Final Lap sound + isFinalLap = true
                if (lapInfos.Count == GameManager.Instance.GetTotalLapNum())
                {
                    gameplaySoundHandler.PlayFinalLap();
                    isFinalLap = true;
                }



                /// Nếu người chơi kết thúc vòng đua cuối cùng
                if (lapInfos.Count > GameManager.Instance.GetTotalLapNum())
                {
                    this.startCount = false;

                    /// Play Cross Finish Line sound
                    gameplaySoundHandler.PlayCrossedFinishline();
                    ///

                    GameManager.Instance.SetLocalPlayerFinish(lapInfos[GameManager.Instance.GetTotalLapNum() - 1].GetTimeFinished());
                }
            }
        }
    }

    #endregion

    #region IEnumerator Methods 
    // Region nay chua nhung ham IEnumerator    
    // Doi den khi game bat dat
    IEnumerator WaitUntilGameStart()
    {
        if (!photonView.IsMine)
        {
            yield break;
        }

        // Lien tuc kiem tra trang thai hien tai cua game
        yield return new WaitUntil(() => GameManager.Instance.GetGameStatus() && GameManager.Instance.GetStartTime() != -1f);

        // Khi game bat dau, lap tuc them thong tin mot vong dua moi
        lapInfos.Add(new LapInfo(1, GameManager.Instance.GetStartTime()));
    }

    // Reset vi tri cua nguoi choi
    // timeWarning: Thoi gian thong bao truoc khi thay doi vi tri
    IEnumerator ResetPosition(float timeWarning)
    {
        // Neu viec reset dang dien ra -> Khong goi lai nua
        // Tranh truong hop nhieu coroutine duoc goi cung mot luc
        if (isResettingPosition)
        {
            yield break;
        }

        // Neu chua co viec reset nao -> Danh dau la dang reset
        isResettingPosition = true;

        Debug.LogWarning("Reset player position to checkpoint " + (this.nextCheckpoint - 1));

        float timePause = timeWarning;

        // Active GameObject tuong ung
        this.gameInfoText.SetActive(true);

        // Dem nguoc thoi gian va hien thi
        while (timePause > 0)
        {
            this.gameInfoText.GetComponent<TMP_Text>().text = "WRONG DIRECTION!\nResetting position in " + TimeSpan.FromSeconds(timePause).ToString("mm':'ss':'ff");
            timePause -= Time.deltaTime;
            yield return new WaitForSeconds(Time.deltaTime);
        }

        // Sau khi dem nguoc xong -> Deactive GameObject
        this.gameInfoText.SetActive(false);

        // "Tat may" xe nguoi choi -> Nham ngan viec vua di chuyen vua dich chuyen
        this.GetComponent<CarControl>().StopEngine();

        // Dich chuyen nguoi choi den vi tri checkpoint gan nhat ma nguoi choi da di qua
        if (this.nextCheckpoint - 1 >= 0)
        {
            this.transform.position = GameManager.Instance.GetCheckpointPosition(this.nextCheckpoint - 1);
            this.transform.rotation = GameManager.Instance.GetCheckpointRotation(this.nextCheckpoint - 1);
        }
        else
        {
            int subPosition = 1 - this.nextCheckpoint;
            this.transform.position = GameManager.Instance.GetCheckpointPosition(GameManager.Instance.GetTotalCheckpointNum() - subPosition);
            this.transform.rotation = GameManager.Instance.GetCheckpointRotation(GameManager.Instance.GetTotalCheckpointNum() - subPosition);
        }

        // "Mo may" xe nguoi choi -> Bat dau nhan input cua nguoi choi
        this.GetComponent<CarControl>().StartEngine();

        // Thoi gian cooldown, nham han che tinh trang nhieu coroutine dien ra
        yield return new WaitForSeconds(1.0f);

        // Viec reset da hoan thanh
        isResettingPosition = false;
        hardReset = false;
    }

    #endregion
}