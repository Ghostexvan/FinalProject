using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using JSAM;

public class GameplayMusicHandler : MonoBehaviour
{
    #region Public Fields
    [Tooltip("Track's BGM. Drag the Music File Object here.")]
    public MusicFileObject trackBGM;
    [Tooltip("Track's Final Lap BGM. Drag the Music File Object here.\n" +
        "If you disabled useFinalLapTrack, just ignore this. Or add a filler file here for the sake of it.")]
    public MusicFileObject fasterTrackBGM;
    [Tooltip("")]
    public bool useFinalLapTrack;
    #endregion

    #region Private Fields (Mostly to get the flags from GameManager)
    // Since GameManager is a Singleton
    private bool isGameStart;
    private bool isAllPlayersFinished;
    private bool isFinalLap;

    private float currentMainMusicTimestamp;

    private LapController lc;
    #endregion

    /// We don't need a track for Results Screen
    private void Awake()
    {
        isGameStart = false;
        isAllPlayersFinished = false;
        isFinalLap = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        lc = CarControl.LocalPlayerInstance.gameObject.GetComponent<LapController>();

        if (lc == null)
        {
            Debug.LogError("Couldn't grab LC!!!");
        }
        else
        {
            Debug.LogWarning("LC IS NOT NULL");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (lc != null)
        {
            Debug.LogWarning("LC IS NOT NULL YAYYYYY");
        }

        isGameStart = GameManager.Instance.GetGameStatus();
        isAllPlayersFinished = GameManager.Instance.IsAllPlayersFinished();
        isFinalLap = lc.isFinalLap;         /// Problem lies here apparently

        // Heck this if part isn't even needed. I'm putting this here for the sake of it.
        // We don't need to sync these in multiplayer since this plays differently depending on the player
        // ("differently" would mostly whether they're on their final laps are not).
        // This component is inside each "BGM Handler"
        if (CarControl.LocalPlayerInstance.gameObject != null)
        {
            ManageMusic(isGameStart, isAllPlayersFinished, useFinalLapTrack, isFinalLap);
            Debug.LogError("This is only a test, I'm printing this out: " + CarControl.LocalPlayerInstance.gameObject);
            Debug.LogError("This is to check whether I can access this GameObject's LapControl or not: " + CarControl.LocalPlayerInstance.gameObject.GetComponent<LapController>() );
            Debug.LogError("And this is to check the lc variable: " + lc );
        }

        /// We did have Stop Sounds/Musics on Scene Changed so we don't need to do anything when we change from Gameplay Scene to Launcher.
    }

    private void ManageMusic(bool isGameStart, bool isAllPlayersFinished, bool useFinalLapTrack, bool isFinalLap)
    {
        /// Fade In Main Track BGM after Countdown and GO SFX. 
        /// isStart = True happens roughly the same time as when GO SFX is played (Right before it is played)
        if (isGameStart == true)
        {
            // If we have yet to reach the final lap
            if (isFinalLap == false)
            {
                // If track BGM hasn't been played yet
                if (!AudioManager.IsMusicPlaying(trackBGM))
                    AudioManager.FadeMusicIn(trackBGM, 2f, true);
            }
            // If we're at the final lap
            else
            {
                // If useFinalLapTrack is true (you can toggle it in the Inspector)
                if (useFinalLapTrack)
                {
                    if (!AudioManager.IsMusicPlaying(fasterTrackBGM))
                    {
                        // Either I have Fade In first then Fade Out, or I let Fade Out be a bit longer than Fade In
                        // The Fades are called as Corous, but FadeIn and FadeOut Corous don't require you to wait the whole yield return StartCorou(FadeIn/Out),
                        // rather they run normally every frame. Meaning they can be Cross-Faded
                        AudioManager.FadeMainMusicOut(3f);      // Fading 
                        AudioManager.FadeMusicIn(fasterTrackBGM, 2f, true);
                    }
                }
                // If useFinalLapTrack is false then do nothing, letting trackBGM to continue playing.
            }
        }

        /// After race has been finished (isStart = false ; GameManager --> StopAllMoving(), isAllPlayersFinished = true),
        /// Fade In results BGM, Fade Out currently played Track BGM (either it's the normal BGM or the Final Lap one)
        // (Sure I can also use else, it works, but isGameStart is also False during the Countdown section and before the GO SFX is played)
        else
        {
            // If race has been finished and Results BGM hasn't been played
            // --> Fade out current Main track (either trackBGM or fasterTrackBGM) and Fade In Results BGM
            if (isAllPlayersFinished)
            {
                if (!AudioManager.IsMusicPlaying(MainGameMusic.Finish1st6thPlaceResults_))
                {
                    AudioManager.FadeMainMusicOut(3f);
                    AudioManager.FadeMusicIn(MainGameMusic.Finish1st6thPlaceResults_, 3f, false);
                }
            }
        }
        //if (isGameStart == false && isAllPlayersFinished == true)
        //{

        //}
    }
}
