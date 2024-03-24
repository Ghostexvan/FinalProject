using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// This can be put anywhere since I actually referred to specific GameObjects/Components.
/// Look for "Input Mode (TMP)" if you want to find where this script is put
/// </summary>
public class InputModeUI_ : MonoBehaviour
{
    private UDPSocketTest_Controller udpsock;     // This is to get isKeyboardInput and elapsedKBTime
    private TMP_Text inputModeText;
    private bool isUDPActive;
    private GameObject timeCounter;

    private float elapsedKBTime;        // elapsedKBTime trong UDPSocketTest1

    private void Awake()
    {
        udpsock = GameObject.Find("GameManager").GetComponent<UDPSocketTest_Controller>();
        inputModeText = GameObject.Find("Input Mode (TMP)").GetComponent<TMP_Text>();       // Input Mode UI Text
        timeCounter = GameObject.Find("Keyboard Input Time (TMP)");    // Keyboard Input Waiting Time UI
    }

    // Start is called before the first frame update
    void Start()
    {
        isUDPActive = udpsock.isUDPActive;
        if (isUDPActive)
        {
            inputModeText.text = "UDP INPUT MODE";
            inputModeText.color = new Color32(0, 255, 0, 255);
        }
        else
        {
            inputModeText.text = "KEYBOARD INPUT MODE";
            inputModeText.color = new Color32(255, 0, 0, 255);
        }
    }

    // Update is called once per frame
    void Update()
    {
        elapsedKBTime = udpsock.elapsedKBTime;      // elapsedKBTime == TimeToWait trong UDPSocketTest1

        isUDPActive = udpsock.isUDPActive;
        if (isUDPActive)
        {
            inputModeText.text = "UDP INPUT MODE";
            inputModeText.color = new Color32(0, 255, 0, 255);

            if (udpsock.isKeyboardInput)
            {
                inputModeText.text = "(Temp) KEYBOARD INPUT MODE";
                inputModeText.color = new Color32(255, 0, 0, 255);
            }
        }
        else
        {
            inputModeText.text = "KEYBOARD INPUT MODE";
            inputModeText.color = new Color32(255, 0, 0, 255);
        }

        //// Putting the countdown here to see things easier
        // I can put it in isUDPActive and then in udpsock.isKeyboardInput, BUT putting it here makes it easier for me to read
        if (elapsedKBTime > 0 && isUDPActive && udpsock.isKeyboardInput)
        {
            timeCounter.SetActive(true);
            timeCounter.GetComponent<TMP_Text>().text = elapsedKBTime.ToString("0.00");
        }
        if (elapsedKBTime <= 0)
        {
            timeCounter.SetActive(false);
        }

    }
}
