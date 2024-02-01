using TMPro;
using UnityEngine;

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
        GameObject.Find("LauncherManager").GetComponent<LauncherManager>().EnterRoom(this.roomCode);
    }
}
