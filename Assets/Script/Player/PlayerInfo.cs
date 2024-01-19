// Chua comment
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerInfo
{
    # region Private Serializable Fields
    [SerializeField]
    private int rank;

    [SerializeField] 
    private int currentLap;

    [SerializeField]
    private int currentCheckpoint;

    [SerializeField]
    private float distanceToNextCheckpoint;

    [SerializeField]
    private bool isReady;

    #endregion

    #region Construtors
    public PlayerInfo(){
        this.isReady = false;
    }

    #endregion

    #region Public Fields

    public void UpdateInfo(int currentLap, int currentCheckpoint, float distanceToNextCheckpoint){
        this.currentLap = currentLap;
        this.currentCheckpoint = currentCheckpoint;
        this.distanceToNextCheckpoint = distanceToNextCheckpoint;
    }

    public void SetReady(){
        this.isReady = true;
    }

    public void SetRank(int rank){
        this.rank = rank;
    }

    public bool GetPlayerStatus(){
        return isReady;
    }

    public int GetRank(){
        return this.rank;
    }

    public int GetCurrentLap(){
        return this.currentLap;
    }

    public int GetCurrentCheckpoint(){
        return this.currentCheckpoint;
    }

    public float GetDistanceToNextCheckpoint(){
        return this.distanceToNextCheckpoint;
    }
    
    #endregion
}
