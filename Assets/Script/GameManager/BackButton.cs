using Photon.Pun;
using UnityEngine;
using JSAM;

public class BackButton : MonoBehaviour
{
    [SerializeField]
    private GameObject destinatedPanel;

    [SerializeField]
    private bool isInRoom;

    [SerializeField]
    private bool isInLobby;

    //public bool PlaySound = true;

    public void OnClick()
    {
        // if (isInRoom)
        // {
        //     PhotonNetwork.LeaveRoom();
        // }

        // if (isInLobby)
        // {
        //     PhotonNetwork.LeaveLobby();
        //     PhotonNetwork.Disconnect();
        // }

        // destinatedPanel.SetActive(true);
        // this.transform.parent.transform.gameObject.SetActive(false);
        LauncherManager.Instance.Back();    
    }

    

    ////// Sound methods, mostly for testing
    /// Most are migrated to SoundHandler.cs in SFX Handler GameObject
    public void PlaySoundClickBack()
    {
        AudioManager.PlaySound(MainGameSounds.menu_back);

        //if (PlaySound)
        //{
        //    AudioManager.PlaySound(MainGameSounds.menu_back);
        //}
    }

    public void PlaySoundClickAccept()
    {
        AudioManager.PlaySound(MainGameSounds.menu_accept);
    }

    public void PlaySoundHover()
    {
        AudioManager.PlaySound(MainGameSounds.menu_focus);

        //if (PlaySound)
        //{
        //    AudioManager.PlaySound(MainGameSounds.menu_focus);
        //}
    }

    /// <summary>
    /// idk if this is really needed since I did put the settings as StopAllSound when Scene changed
    /// </summary>
    //private void OnDestroy()
    //{
    //    AudioManager.StopSoundIfPlaying(MainGameSounds.menu_back);
    //    AudioManager.StopSoundIfPlaying(MainGameSounds.menu_focus);

    //    //if (PlaySound)
    //    //{
    //    //    AudioManager.StopSoundIfPlaying(MainGameSounds.menu_back);
    //    //    AudioManager.StopSoundIfPlaying(MainGameSounds.menu_focus);
    //    //}
    //}

    //private void OnDisable()
    //{
    //    //if (PlaySound)
    //    //{
    //    //    AudioManager.StopSoundIfPlaying(MainGameSounds.menu_back);
    //    //    AudioManager.StopSoundIfPlaying(MainGameSounds.menu_focus);
    //    //}
    //}
}
