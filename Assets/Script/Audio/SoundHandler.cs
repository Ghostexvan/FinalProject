using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using JSAM;

/// <summary>
/// This is only for the UI, I'll be writing a seperate script for Gameplay Scene later.
/// </summary>
public class SoundHandler : MonoBehaviour
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

    /// (Mostly used for Button callbacks in Inspector)
    #region Public Sound Methods 
    // Click and Back are put in Buttons' OnClick callback in Inspector, hence why they have no references
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

    public void PlaySoundHover()
    {
        AudioManager.StopSoundIfPlaying(MainGameSounds.menu_focus);
        AudioManager.PlaySound(MainGameSounds.menu_focus);
    }

    public void PlaySoundCarCatalog()
    {
        AudioManager.StopSoundIfPlaying(MainGameSounds.Carcatalog);
        AudioManager.PlaySound(MainGameSounds.Carcatalog);
    }

    public void PlaySoundMove()
    {
        AudioManager.StopSoundIfPlaying(MainGameSounds.Move);
        AudioManager.PlaySound(MainGameSounds.Move);
    }
    #endregion

    // Just to make sure, we'd already fixed it in the Settings
    //private void OnDestroy()
    //{
    //    AudioManager.StopAllSounds();
    //    AudioManager.StopAllMusic();
    //}
}
