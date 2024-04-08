using System;
using UnityEngine;

[CreateAssetMenu]
public class GameSettings : ScriptableObject
{
    [Serializable]
    public struct ResolutionInfo{
        public int width, height;
        public bool isFullscreen;
    }

    [Serializable]
    public struct VolumeInfo{
        public float master;
        public float music;
        public float sound;
    }

    public ResolutionInfo resolution;
    public VolumeInfo volume;
    public int frameRate;
    public bool isSet;
}
