using UnityEngine;

// Thu vien can thiet de thiet lap ket noi den server
using Photon.Pun;
// Thu vien can thiet de su dung Photon trong realtime
using Photon.Realtime;

using System.Collections;
using TMPro;

// Class nay duoc su dung de nhap ten nguoi choi. Ten nay se xuat hien tren nguoi choi
// Can component TMP_InputField de su dung duoc class nay
[RequireComponent(typeof(TMP_InputField))]
public class PlayerNameInputField : MonoBehaviour{
    #region Private Constants
    // Region nay chua nhung truong private hang so 
    // Su dung de luu ten nguoi choi vao PlayerPrefs
    const string playerNamePrefKey = "PlayerName";

    #endregion

    #region  MonoBehaviour Callbacks
    // Region chua nhung ham CallBacks trong Unity
    // Ham nay duoc goi trong giai doan khoi tao Object
    private void Start() {
        string defaultName = string.Empty;
        TMP_InputField _inputField = this.GetComponent<TMP_InputField>();

        // Neu chua co gia tri trong InputField
        if (string.IsNullOrEmpty(_inputField.text)){
            // Kiem tra du lieu trong PlayerPref
            if (PlayerPrefs.HasKey(playerNamePrefKey)){
                // Co du lieu -> Ten nguoi choi la ten duoc luu trong PlayerPrefs
                defaultName = PlayerPrefs.GetString(playerNamePrefKey);
                _inputField.text = defaultName;
            }
        }

        // Dat ten nguoi choi tren server Photon
        PhotonNetwork.NickName = defaultName;
    }

    #endregion

    #region Public Methods
    // Region chua nhung ham public
    // Dat ten cho nguoi choi va luu vao PlayerPrefs
    public void SetPlayerName(string value){
        // Kiem tra ten nguoi choi truoc khi luu
        if (string.IsNullOrEmpty(value)){
            // Bao loi neu ten bo trong
            Debug.LogError("Player Name is null or empty");
            return;
        }

        // Dat ten cho nguoi choi tren server Photon
        PhotonNetwork.NickName = value;

        // Luu ten nguoi choi vao PlayerPrefs
        PlayerPrefs.SetString(playerNamePrefKey, value);
    }

    #endregion
}