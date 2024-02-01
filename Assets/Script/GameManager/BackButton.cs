using Photon.Pun;
using UnityEngine;

public class BackButton : MonoBehaviour
{
    [SerializeField]
    private GameObject destinatedPanel;

    [SerializeField]
    private bool isInRoom;

    [SerializeField]
    private bool isInLobby;

    public void OnClick()
    {
        if (isInRoom)
        {
            PhotonNetwork.LeaveRoom();
        }

        if (isInLobby)
        {
            PhotonNetwork.LeaveLobby();
            PhotonNetwork.Disconnect();
        }

        destinatedPanel.SetActive(true);
        this.transform.parent.transform.gameObject.SetActive(false);
    }
}
