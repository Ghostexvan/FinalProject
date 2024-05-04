using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Diagnostics;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

/// <summary>
/// This class will most likely be most likely be put in a GameObject that contains the UDP Script/Component.
/// </summary>
public class PythonScriptCall : MonoBehaviour
{
    [Tooltip("Specify the path to the included Python executable app. This will be a const since this app may or may not be put in. Do have the path lead right to the .exe file.")]
    // public string file;
    public string pythonAppPath;

    private static Process appProcess;
    public bool isActive = false;      // Initial value is false

    void Awake()
    {
        if (pythonAppPath == "")
        {
            if (SceneManager.GetActiveScene().name.Equals("Launcher", StringComparison.OrdinalIgnoreCase))
            {
                pythonAppPath = Application.dataPath + "/../" + "dist/" + "Hand Gesture Controller/Hand Gesture Controller.exe";
            }
            else
            {
                pythonAppPath = Application.dataPath + "/../" + "dist/" + "10F TFLite Small - MediaPipe Holistic/MediaPipe Holistic Demo.exe";
                if (File.Exists(pythonAppPath) == false)
                {
                    pythonAppPath = Application.dataPath + "/../" + "dist/" + "10F TFLite - MediaPipe Holistic/MediaPipe Holistic Demo.exe";
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (isActive == true)
        {
            if (SceneManager.GetActiveScene().name.Equals("Launcher", StringComparison.OrdinalIgnoreCase))
            {
                isActive = this.gameObject.GetComponent<UI_UDP_Receiver>().isUDPActive;
                if (isActive)
                {
                    UnityEngine.Debug.LogWarning("[APP INFO] " + pythonAppPath);
                    StartCoroutine(RunOnStart());
                }
                else
                {
                    UnityEngine.Debug.LogWarning("[APP WARNING] UDP is disabled! App will not be started");
                }
            }
            // This is to open the app in the gameplay scenes
            else
            {
                isActive = this.gameObject.GetComponent<UDPSocketTest_Controller>().isUDPActive;
                if (isActive)
                {
                    UnityEngine.Debug.LogWarning("[APP INFO] " + pythonAppPath);
                    StartCoroutine(RunOnStart());
                }
                else
                {
                    UnityEngine.Debug.LogWarning("[APP WARNING] UDP is disabled! App will not be started");
                }
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // I didn't get why I put it as a Corou though
    IEnumerator RunOnStart()
    {
        // print(Application.dataPath);

        ProcessStartInfo appInfo = new ProcessStartInfo();
        appInfo.FileName = pythonAppPath;

        /// Added cond in case something happens
        if (File.Exists(pythonAppPath))
        {
            try
            {
                appProcess = Process.Start(appInfo);
            }
            catch (Exception exc)
            {
                UnityEngine.Debug.LogError("App does not exist! Error:" + exc);
                yield break;
            }
        }

        yield return null;
    }

    // Turns off current webcam app when changing scenes
    private void OnDestroy()
    {
        if (appProcess != null)
        {
            if (!appProcess.HasExited)
            {
                appProcess.Kill();
            }

        }
    }

    // Turns off current webcam app when exiting game
    private void OnApplicationQuit()
    {
        if (appProcess != null)
        {
            if (!appProcess.HasExited)
            {
                appProcess.Kill();
            }
        }
    }
}
