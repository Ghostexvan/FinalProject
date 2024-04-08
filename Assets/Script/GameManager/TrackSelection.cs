// chua don dep
// chua comment
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;

using JSAM;

public class TrackSelection : MonoBehaviour
{
    [SerializeField]
    private TrackCollection trackCollection;

    public static TrackSelection Instance;

    private void Awake() {
        if (TrackSelection.Instance == null) {
            TrackSelection.Instance = this;
        }

        foreach (string trackName in trackCollection.GetTrackCollection()){
            this.GetComponent<TMP_Dropdown>().AddOptions(new List<string>() {
                trackName,
            });
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.GetPlayer(PhotonNetwork.CurrentRoom.MasterClientId).CustomProperties.ContainsKey("Track")) {
            this.GetComponent<TMP_Dropdown>().value = (int)PhotonNetwork.CurrentRoom.GetPlayer(PhotonNetwork.CurrentRoom.MasterClientId).CustomProperties["Track"];
        }
    }

    public void OnValueChange() {
        Debug.LogWarning("Player " + PhotonNetwork.LocalPlayer.NickName + " has changed track for this room!");
        ExitGames.Client.Photon.Hashtable trackSelection = PhotonNetwork.LocalPlayer.CustomProperties;
        trackSelection["Track"] = this.GetComponent<TMP_Dropdown>().value;
        PhotonNetwork.LocalPlayer.SetCustomProperties(trackSelection);

        /// Test: ...This seems weird, but it's worth a shot though
        /// Again, I can't access it through the Inspector, so I added it here
        AudioManager.StopSoundIfPlaying(MainGameSounds.menu_accept);        // This is optional
        AudioManager.PlaySound(MainGameSounds.menu_accept);
    }

    public string GetTrackName() {
        return this.GetComponent<TMP_Dropdown>().captionText.text;
    }

    public string GetRandomTrackName() {
        return this.trackCollection.GetRandomTrack();
    }

    private void OnEnable() {
        this.GetComponent<TMP_Dropdown>().value = 0;

        if (!PhotonNetwork.IsMasterClient){
            this.GetComponent<TMP_Dropdown>().interactable = false;
        } else {
            this.GetComponent<TMP_Dropdown>().interactable = true;
        }
    }
}
