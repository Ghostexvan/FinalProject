using System.Collections;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class ReadyButton : MonoBehaviour
{
    private void Update() {
        if (PhotonNetwork.IsMasterClient && !LauncherManager.Instance.CheckAllPlayerReady() && this.transform.GetChild(0).GetComponent<TMP_Text>().text == "Start") {
            this.transform.GetChild(0).GetComponent<TMP_Text>().text = "Cancel Ready";

            StartCoroutine(WaitForAllPlayerReady());
        }
    }

    public void OnClick() {
        if (this.transform.GetChild(0).GetComponent<TMP_Text>().text == "Ready"){
            LauncherManager.Instance.SetPlayerReady();
            this.transform.GetChild(0).GetComponent<TMP_Text>().text = "Cancel Ready";

            if (PhotonNetwork.IsMasterClient) {
                StartCoroutine(WaitForAllPlayerReady());
            }
        } else if (this.transform.GetChild(0).GetComponent<TMP_Text>().text == "Cancel Ready") {
            LauncherManager.Instance.SetPlayerReady();
            this.transform.GetChild(0).GetComponent<TMP_Text>().text = "Ready";
            StopAllCoroutines();
        } else if (this.transform.GetChild(0).GetComponent<TMP_Text>().text == "Start") {
            Debug.Log("Starting game!");
            LauncherManager.Instance.StartGame();
        }
    }

    IEnumerator WaitForAllPlayerReady() {
        yield return new WaitUntil(() => LauncherManager.Instance.CheckAllPlayerReady());

        this.transform.GetChild(0).GetComponent<TMP_Text>().text = "Start";
    }
}
