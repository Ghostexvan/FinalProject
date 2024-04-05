using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using JSAM;

public class UI_MusicHandler : MonoBehaviour
{
    [SerializeField]
    [Tooltip("You don't really need to specify anything here.")]
    private GameObject roomPanel;

    private float currentMainMusicTimestamp;

    private void Awake()
    {
        roomPanel = GameObject.FindGameObjectWithTag("Room");
        if (roomPanel == null)
        {
            Debug.LogError("Cannot found Room's Panel!", this);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Making this the Main Music
        AudioManager.PlayMusic(MainGameMusic.GranTurismo4SoundtrackArcadeModeLoopable, true);
    }

    // Update is called once per frame
    void Update()
    {
        // This is technically my attempt at making a Cross-fading effect between the 2 tracks.
        // It seems to work pretty well, with me Fading Music In before Fading the other music track out.
        // Fading out first would leave a small period where no music is played, making it non-linear + pretty awkward.

        // Either I have Fade In first then Fade Out, or I let Fade Out be a bit longer than Fade In
        // The Fades are called as Corous, but FadeIn and FadeOut Corous don't require you to wait the whole yield return StartCorou(FadeIn/Out),
        // rather they run normally every frame. Meaning they can be Cross-Faded

        // I didn't expect much since I wrote this at 5 in the morning and I've only tested it at like 1pm in the afternoon...
        /* 
         * SO HOW THIS WORKS IS THAT:
        - Play Track 1 in Main Menu
        - If RoomPanel is active and Track 2 hasn't been played yet:
            + Get the currentTimeStamp of Track 1
            + Fade In and play Track 2 (Fade In time will be 2s)
            + Fade Out track 1 (Fade Out time will also be 2s)
        - If player exits the RoomPanel and Track 1 hasn't resumed/started yet:
            + Fade In and resume playing Track 1 at currentTimeStamp (Fade In time will be 2s)
            + Fade Out track 2 (Fade Out time will be 2s)
        
        - I didn't get Track 2's time stamp since I wanted it to be restarted every time I entered a new Room.
        //
        */

        /// We did have Stop Sounds/Musics on Scene Changed so we don't need to do anything when we change from  Launcher to Gameplay Scene.

        if (roomPanel.activeInHierarchy == true)
        {
            if (!AudioManager.IsMusicPlaying(MainGameMusic.GranTurismo4SoundtrackPowerandSpeedLoopable))
            {
                currentMainMusicTimestamp = AudioManager.MainMusicHelper.AudioSource.time;
                AudioManager.FadeMusicIn(MainGameMusic.GranTurismo4SoundtrackPowerandSpeedLoopable, 1);
                AudioManager.FadeMainMusicOut(2);
            }
        }
        else
        {
            if (!AudioManager.IsMusicPlaying(MainGameMusic.GranTurismo4SoundtrackArcadeModeLoopable))
            {
                AudioManager.FadeMusicIn(MainGameMusic.GranTurismo4SoundtrackArcadeModeLoopable, 1, true).AudioSource.time = currentMainMusicTimestamp;
                AudioManager.FadeMusicOut(MainGameMusic.GranTurismo4SoundtrackPowerandSpeedLoopable, 1);
            }
        }
    }

    // This is basically the same premise, written a little differently
    public void SecondOption(bool isRoomActive)
    {
        float time = 0f;
        if (isRoomActive)
        {
            time = AudioManager.FadeMainMusicOut(2).AudioSource.time;
            if (!AudioManager.IsMusicPlaying(MainGameMusic.GranTurismo4SoundtrackPowerandSpeedLoopable))
            {
                AudioManager.FadeMusicIn(MainGameMusic.GranTurismo4SoundtrackPowerandSpeedLoopable, 2);
            }
        }
        else
        {
            if (!AudioManager.IsMusicPlaying(MainGameMusic.GranTurismo4SoundtrackArcadeModeLoopable))
            {
                AudioManager.FadeMusicIn(MainGameMusic.GranTurismo4SoundtrackArcadeModeLoopable, 2, true).AudioSource.time = time;
                AudioManager.FadeMusicOut(MainGameMusic.GranTurismo4SoundtrackPowerandSpeedLoopable, 2);
            }
        }
    }
}
