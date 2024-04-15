// Chua don dep
// Chua comment
using System;
using UnityEngine;

[CreateAssetMenu]
public class TrackCollection : ScriptableObject
{
    [SerializeField]
    private TrackInfo[] trackInfo;

    public TrackInfo GetRandomTrack()
    {
        TrackInfo[] randomList = trackInfo;
        randomList.Shuffle();

        return randomList[0];
    }

    public TrackInfo[] GetTrackInfo()
    {
        return trackInfo;
    }
}

[Serializable]
public struct TrackInfo
{
    public string trackName;
    public Sprite trackSprite;
}