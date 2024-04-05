using TMPro;
using UnityEngine;

using JSAM;

public class RoomButton : MonoBehaviour
{
    private string roomCode;

    public void SetRoomCode(string roomCode){
        this.roomCode = roomCode;
    }

    public string GetRoomCode(){
        return this.roomCode;
    }

    public void OnClick(){
        // Plays Button click sound
        // I wrote the PlaySound here directly since RoomInfo is a prefab, and prefabs can't actually get
        // GameObject references from game Scenes directly.
        // AudioManager (kinda) works since it's a Singleton...? Also because of the JSAM import at the start I think.
        AudioManager.StopSoundIfPlaying(MainGameSounds.menu_accept);
        AudioManager.PlaySound(MainGameSounds.menu_accept);

        GameObject.Find("LauncherManager").GetComponent<LauncherManager>().EnterRoom(this.roomCode);
    }
}
