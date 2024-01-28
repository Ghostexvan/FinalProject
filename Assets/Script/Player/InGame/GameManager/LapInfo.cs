using System;
using UnityEngine;

[Serializable]
// Su dung de luu thong tin cac vong dua
public class LapInfo
{
    #region Private Serializable Fields
    // Region chua nhung truong private co the serialize
    // Serialize co the hieu don gian la cac truong co the duoc hien thi trong tab Inspector trong Unity
    // So vong dua
    [SerializeField]
    private int lapNum;

    // Thoi gian bat dau vong dua
    [SerializeField]
    private double timeStarted;

    // Thoi gian ket thuc vong dua
    [SerializeField]
    private double timeFinished;

    #endregion

    #region Constructor
    // Region nay chua nhung ham khoi tao
    // Khi khoi tao mot vong dua moi, chi can quan tam so vong dua va thoi gian bat dau
    public LapInfo(int lapNum, double timeStarted)
    {
        this.lapNum = lapNum;
        this.timeStarted = timeStarted;
    }

    #endregion

    #region Public Methods
    // Region nay chua nhung ham public
    // Dat thoi gian ket thuc cho vong dua
    public void SetTimeFinished(double timeFinished)
    {
        this.timeFinished = timeFinished;
    }

    // Lay so vong dua hien tai
    public int GetLapNum()
    {
        return this.lapNum;
    }

    // Lay thoi gian bat dau vong dua
    public double GetTimeStarted()
    {
        return this.timeStarted;
    }

    // Lay thoi gian ket thuc vong dua
    public double GetTimeFinished()
    {
        return this.timeFinished;
    }

    #endregion
}
