// Chua don dep
// Chua comment
using UnityEngine;

[CreateAssetMenu]
public class TrackCollection : ScriptableObject
{
    [SerializeField]
    private string[] trackCollection;

    public string GetRandomTrack(){
        string[] randomList = trackCollection;
        randomList.Shuffle();

        return randomList[0];
    }

    public string[] GetTrackCollection(){
        return trackCollection;
    }
}
