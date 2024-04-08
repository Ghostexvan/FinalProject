using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using JSAM;

/// <summary>
/// This is for the Gameplay's UIs. Mostly for the countdowns and SFX played when a player crossed the finish line...?
/// </summary>
public class GameplaySoundHandler : MonoBehaviour
{
    #region MonoBehaviour Callbacks (unused)
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    #endregion

    /// <summary>
    /// ALL OF THESE ARE INTENDED TO BE PLAYED ON THE PLAYER'S SIDE ONLY, so I didn't sync it with other players'.
    /// Well that said, some are put in methods that are used online though. (PlayStartEndingCountdown)
    /// </summary>
    #region Public Sound Methods
    public void PlayCountdownSFX()
    {
        AudioManager.StopSoundIfPlaying(MainGameSounds.SE_RC_321);
        AudioManager.PlaySound(MainGameSounds.SE_RC_321);
    }

    public void PlayGoSFX()
    {
        AudioManager.StopSoundIfPlaying(MainGameSounds.SE_RC_321);
        AudioManager.StopSoundIfPlaying(MainGameSounds.SE_RC_GO);
        AudioManager.PlaySound(MainGameSounds.SE_RC_GO);
    }

    public void PlayCrossedFinishline()
    {
        //AudioManager.StopSoundIfPlaying(GameplaySounds.SE_RC_FINISH);
        AudioManager.PlaySound(MainGameSounds.SE_RC_FINISH);
    }

    public void PlayStartEndingCountdown()
    {
        AudioManager.StopSoundIfPlaying(MainGameSounds.SE_RC_START_ENDING_COUNTDOWN);
        AudioManager.PlaySound(MainGameSounds.SE_RC_START_ENDING_COUNTDOWN);
    }

    public void PlaySoundClick()
    {
        AudioManager.StopSoundIfPlaying(MainGameSounds.menu_accept);
        AudioManager.PlaySound(MainGameSounds.menu_accept);
    }

    public void PlaySoundBack()
    {
        AudioManager.StopSoundIfPlaying(MainGameSounds.menu_back);
        AudioManager.PlaySound(MainGameSounds.menu_back);
    }

    public void PlayPassedLap()
    {
        AudioManager.PlaySound(MainGameSounds.SE_RC_LAP);
    }

    public void PlayFinalLap()
    {
        AudioManager.PlaySound(MainGameSounds.SE_RC_LAP_FINAL);
    }

    #endregion

}
