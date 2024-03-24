// chua don dep
// chua comment
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpeedUI : MonoBehaviour
{
    TMP_Text speedText;
    GameObject clockSpeed;

    private void Awake() {
        speedText = GameObject.FindGameObjectWithTag("SpeedUIText").GetComponent<TMP_Text>();
        if (speedText == null) {
            Debug.LogError("Missing Speed UI Text", this);
        }

        clockSpeed = GameObject.FindGameObjectWithTag("SpeedUISpeedometer").transform.GetChild(0).gameObject;
        if (clockSpeed == null){
            Debug.LogError("Missing Speed UI Clock", this);
        }

        //Debug.LogWarning("Speed Text: " + speedText.transform.gameObject.activeSelf + " - " + speedText.gameObject.activeInHierarchy);
        //Debug.LogWarning("Clock UI: " + clockSpeed.activeSelf + " - " + clockSpeed.activeInHierarchy);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float speedValue = CarControl.LocalPlayerInstance.GetComponent<CarControl>().GetSpeed();
        float maxSpeedValue = CarControl.LocalPlayerInstance.GetComponent<CarControl>().maxSpeed;
        speedText.text = "<size=150%>" + speedValue.ToString("F0") + "\n<size=100%>KM/H";

        clockSpeed.transform.rotation = Quaternion.Euler(clockSpeed.transform.rotation.x,
                                                         clockSpeed.transform.rotation.y,
                                                         90f - (speedValue/maxSpeedValue) * 160f);
    }
}
