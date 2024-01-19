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
    private float timeStartGoingWrongDirection = -1f;

    // Thoi gian ma nguoi choi bat dau di dung huong sau khi sai huong
    // Mac dinh: -1 la khong co
    private float timeStartGoingRightDirection = -1f;

    // Kiem tra nguoi choi co vua dat duoc checkpoint moi
    private bool isEnterNewCheckpoint;

    // Kiem tra nguoi choi co dang chuan bi reset vi tri
    private bool isResettingPosition;

    #endregion

    #region MonoBehaviour Callbacks
    // Region chua nhung ham CallBacks trong Unity
    // Ham nay duoc goi trong giai doan khoi tao Object
    private void Awake()
    {
        // Lay cac GameObject can thiet, neu khong co -> Bao loi
        this.lapInfoText = _GameManager.Instance.GetLapInfoText();
        if (this.lapInfoText == null)
        {
            Debug.LogError("Missing UI Lap Info", this);
            return;
        }

        this.gameInfoText = _GameManager.Instance.GetGameInfoText();
        if (this.gameInfoText == null)
        {
            Debug.LogError("Missing UI Game Info", this);
            return;
        }

        // Dat cac gia tri mac dinh
        this.timeStartGoingRightDirection = -1f;
        this.timeStartGoingWrongDirection = -1f;
        this.isEnterNewCheckpoint = false;
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

    #endregion

    #region MonoBehaviourPunCallbacks Callbacks
    // Region nay chua nhung ham duoc goi de dong bo
    // Dong bo cac du lieu giua client va master
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(lapInfos.Count);

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

            for (int i = 0; i < lapInfoCount; i++)
            {
                if (i >= lapInfos.Count)
                {
                    lapInfos.Add(new LapInfo((int)stream.ReceiveNext(),
                                             (float)stream.ReceiveNext()));
                    lapInfos[i].SetTimeFinished((float)stream.ReceiveNext());
                }
                else
                {
                    stream.ReceiveNext();
                    stream.ReceiveNext();

                    float timeFinished = (float)stream.ReceiveNext();
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
        return this.nextCheckpoint == 0 ? _GameManager.Instance.GetTotalCheckpointNum() - 1 : this.nextCheckpoint - 1;
    }

    // Lay khoang cach den checkpoint tiep theo cua nguoi choi nay
    public float GetDistanceToNextCheckpoint()
    {
        return this.distanceToNextCheckpoint;
    }

    public bool GetCountStatus(){
        return this.startCount;
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

        text.text = "Pos: " + _GameManager.Instance.GetLocalPlayerRank() + "/" + PhotonNetwork.PlayerList.Length + "<br>";

        // Hien thi vong dua hien tai cua nguoi choi
        text.text += "Lap: " + lapInfos[lapInfos.Count - 1].GetLapNum() + "/" + _GameManager.Instance.GetTotalLapNum() + "<br>";

        // Voi tung thong tin vong dua cua nguoi choi
        foreach (LapInfo lapInfo in lapInfos)
        {
            // Hien thi so vong dua tuong ung
            text.text += lapInfo.GetLapNum() + ": ";

            // Neu vong dua chua hoan thanh
            if (lapInfo.GetTimeFinished() == 0)
            {
                // Hien thi thoi gian tinh tu luc bat dau vong dua den hien tai
                text.text += TimeSpan.FromSeconds(Time.realtimeSinceStartup - lapInfo.GetTimeStarted()).ToString("mm':'ss':'ff");
            }
            // Neu vong dua da hoan thanh
            else
            {
                // Hien thi thoi gian hoan thanh vong dua
                text.text += TimeSpan.FromSeconds(lapInfo.GetTimeFinished() - lapInfo.GetTimeStarted()).ToString("mm':'ss':'ff");
            }

            // Xuong dong (cho cac thong tin vong dua sau)
            text.text += "<br>";
        }
    }

    // Xac dinh khoang cach cua nguoi choi den checkpoint tiep theo
    private void DetermineDistanceToNextCheckpoint()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        // Neu game chua bat dau
        if (!_GameManager.Instance.GetGameStatus())
        {
            // Debug.Log("1 " + this.distanceToNextCheckpoint);

            // Chi tinh khoang cach ma khong lam gi ca
            this.distanceToNextCheckpoint = Vector3.Distance(this.transform.position,
                                                   _GameManager.Instance.GetCheckpointPosition(this.nextCheckpoint));
            return;
        }

        // Tinh khoang cach moi tu nguoi choi den checkpoint tiep theo
        // Debug.Log("2 " + this.distanceToNextCheckpoint);
        float distanceCaculated = Vector3.Distance(this.transform.position,
                                                   _GameManager.Instance.GetCheckpointPosition(this.nextCheckpoint));

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
                this.timeStartGoingWrongDirection = Time.realtimeSinceStartup;
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
                                                   _GameManager.Instance.GetCheckpointPosition(this.nextCheckpoint));
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
                this.timeStartGoingRightDirection = Time.realtimeSinceStartup;
            }

            // Neu thoi gian di nguoc chieu da qua thoi gian quy dinh
            if (Time.realtimeSinceStartup - this.timeStartGoingWrongDirection >= 5.0f)
            {
                // Reset vi tri nguoi choi
                StartCoroutine(ResetPosition(3.0f));

                // Huy bo moc thoi gian di nguoc chieu
                this.timeStartGoingWrongDirection = -1f;

                // Huy bo moc thoi gian di dung chieu
                this.timeStartGoingRightDirection = -1f;

                // Tinh lai khoang cach do vua thay doi vi tri
                distanceCaculated = Vector3.Distance(this.transform.position,
                                                   _GameManager.Instance.GetCheckpointPosition(this.nextCheckpoint));
            }
            // Neu thoi gian di dung chieu da qua thoi gian quy dinh
            else if (Time.realtimeSinceStartup - this.timeStartGoingRightDirection >= 3.0f)
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
        if (!_GameManager.Instance.GetGameStatus() && !startCount)
        {
            return;
        }

        // Chi tinh khi checkpoint nhan vao bang checkpoint du kien
        if (checkpointNum == this.nextCheckpoint)
        {
            // Ghi nhan checkpoint tiep theo cho nguoi choi
            this.nextCheckpoint = checkpointNum == _GameManager.Instance.GetTotalCheckpointNum() - 1 ?
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
                lapInfos[lapInfos.Count - 1].SetTimeFinished(Time.realtimeSinceStartup);

                // if (lapInfos.Count >= _GameManager.Instance.GetTotalLapNum())
                // {
                //     _GameManager.Instance.SetStart();
                // }

                // Them vong dua moi
                lapInfos.Add(new LapInfo(lapInfos.Count + 1, Time.realtimeSinceStartup));
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
        yield return new WaitUntil(() => _GameManager.Instance.GetGameStatus());

        // Khi game bat dau, lap tuc them thong tin mot vong dua moi
        lapInfos.Add(new LapInfo(1, Time.realtimeSinceStartup));
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
            this.gameInfoText.GetComponent<TMP_Text>().text = "WRONG DIRECTION!<br>Resetting position in " + TimeSpan.FromSeconds(timePause).ToString("mm':'ss':'ff");
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
            this.transform.position = _GameManager.Instance.GetCheckpointPosition(this.nextCheckpoint - 1);
            this.transform.rotation = _GameManager.Instance.GetCheckpointRotation(this.nextCheckpoint - 1);
        }
        else
        {
            int subPosition = 1 - this.nextCheckpoint;
            this.transform.position = _GameManager.Instance.GetCheckpointPosition(_GameManager.Instance.GetTotalCheckpointNum() - subPosition);
            this.transform.rotation = _GameManager.Instance.GetCheckpointRotation(_GameManager.Instance.GetTotalCheckpointNum() - subPosition);
        }

        // "Mo may" xe nguoi choi -> Bat dau nhan input cua nguoi choi
        this.GetComponent<CarControl>().StartEngine();

        // Thoi gian cooldown, nham han che tinh trang nhieu coroutine dien ra
        yield return new WaitForSeconds(1.0f);

        // Viec reset da hoan thanh
        isResettingPosition = false;
    }

    #endregion
}