// chua don dep
// chua comment
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : ILobbyCallbacks, IConnectionCallbacks
{
    private TypedLobby customLobby = new TypedLobby("customLobby", LobbyType.Default);
    private LoadBalancingClient loadBalancingClient;

    private Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();

    public LobbyManager(){
        this.loadBalancingClient = new LoadBalancingClient();
        this.SubscibeToCallbacks();
    }

    ~LobbyManager(){
        this.UnsubscribeToCallbacks();
    }

    private void SubscibeToCallbacks(){
        loadBalancingClient.AddCallbackTarget(this);
    }

    private void UnsubscribeToCallbacks(){
        this.loadBalancingClient.RemoveCallbackTarget(this);
    }

    public void JoinLobby()
    {
        loadBalancingClient.OpJoinLobby(customLobby);
    }

    private void UpdateCachedRoomList(List<RoomInfo> roomList)
    {
        for (int index = 0; index < roomList.Count; index++)
        {
            RoomInfo info = roomList[index];
            if (info.RemovedFromList)
            {
                cachedRoomList.Remove(info.Name);
            }
            else
            {
                cachedRoomList[info.Name] = info;
            }
        }
    }

    #region  ILobbyCallbacks
    void ILobbyCallbacks.OnJoinedLobby()
    {
        Debug.Log("Joined lobby");
        cachedRoomList.Clear();
    }

    void ILobbyCallbacks.OnLeftLobby()
    {
        cachedRoomList.Clear();
    }

    void ILobbyCallbacks.OnRoomListUpdate(List<RoomInfo> roomList)
    {
        UpdateCachedRoomList(roomList);
    }

    void ILobbyCallbacks.OnLobbyStatisticsUpdate(List<Photon.Realtime.TypedLobbyInfo> lobbyStatistics)
    {

    }

    #endregion

    #region  IConnectionCallbacks

    void IConnectionCallbacks.OnDisconnected(DisconnectCause cause)
    {
        cachedRoomList.Clear();
    }

    void IConnectionCallbacks.OnConnected()
    {

    }

    void IConnectionCallbacks.OnConnectedToMaster()
    {

    }

    void IConnectionCallbacks.OnRegionListReceived(RegionHandler regionHandler)
    {

    }

    void IConnectionCallbacks.OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {

    }

    void IConnectionCallbacks.OnCustomAuthenticationFailed(string debugMessage)
    {

    }

    #endregion
}
