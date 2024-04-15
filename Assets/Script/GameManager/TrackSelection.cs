// chua don dep
// chua comment
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using JSAM;

public class TrackSelection : MonoBehaviour
{
    [SerializeField]
    private TrackCollection trackCollection;

    [SerializeField]
    private Image trackImage;

    [SerializeField]
    private Sprite defaultImage;

    public static TrackSelection Instance;

    private void Awake() {
        if (TrackSelection.Instance == null) {
            TrackSelection.Instance = this;
        }

        foreach (TrackInfo trackInfo in trackCollection.GetTrackInfo()){
            this.GetComponent<TMP_Dropdown>().AddOptions(new List<string>() {
                trackInfo.trackName,
            });
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.CurrentRoom.GetPlayer(PhotonNetwork.CurrentRoom.MasterClientId).CustomProperties.ContainsKey("Track")) {
            int selectedTrack = (int)PhotonNetwork.CurrentRoom.GetPlayer(PhotonNetwork.CurrentRoom.MasterClientId).CustomProperties["Track"];
            this.GetComponent<TMP_Dropdown>().value = selectedTrack;
            if (selectedTrack != 0){
                trackImage.sprite = trackCollection.GetTrackInfo()[selectedTrack - 1].trackSprite;
            } else {
                trackImage.sprite = defaultImage;  
            }
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
        return this.trackCollection.GetRandomTrack().trackName;
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
