using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;
using System.Globalization;

public class UDPSocketTest : MonoBehaviour
{
    // I'm not really a "regions" type of guy but I do want the code format to be uniform
    // (I also reused some of the code snippets from my old projects)
    #region Private UDP Variables
    // UDP Variables
    private Thread receiveThread;
    private UdpClient udpClient;
    private string dataReceived = "";
    #endregion

    #region Private Serialize Fields
    [SerializeField]
    private string label;

    [SerializeField]
    private float steerAngle;

    [SerializeField]
    private float normalizedRota;

    [SerializeField]
    private double receiveTimeOut = 1f;

    #endregion

    #region Label Display Test
    public TMP_Text labelText;
    public TMP_Text angleText;
    public TMP_Text rotationText;
    #endregion

    // 


    #region Public Readonly Field (UDP Socket Port)
    public readonly int port = 27001;
    #endregion

    private void Awake()
    {
        labelText = GameObject.Find("Label (TMP)").GetComponent<TMP_Text>();
        angleText = GameObject.Find("Angle (TMP)").GetComponent<TMP_Text>();
        rotationText = GameObject.Find("Rotation (TMP)").GetComponent<TMP_Text>();
    }

    // Start is called before the first frame update
    void Start()
    {
        InitUDPSocket();

    }

    // Update is called once per frame
    void Update()
    {
        ChangeLabel();
    }

    #region UDP - Data Receiving Methods
    private void InitUDPSocket()
    {
        Debug.LogWarning("[UDP STARTED] UDP Initialized");

        receiveThread = new Thread(new ThreadStart(ReceiveDataOld));
        receiveThread.IsBackground = true;
        /*
         Background threads are identical to foreground threads, except that background threads do not prevent a process from terminating.
         Once all foreground threads belonging to a process have terminated, the common language runtime ends the process. 
         Any remaining background threads are stopped and do not complete.
         */
        receiveThread.Start();
    }

    private void ReceiveDataOld()
    {
        udpClient = new UdpClient(port);

        while (udpClient != null)
        {

            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, port);
                byte[] buffer = udpClient.Receive(ref anyIP);

                print("[UDP INFO] UDP buffer length received: " + buffer.Length);

                if (buffer == null)
                {
                    Debug.LogWarning("[UDP WARNING] UDP has not received any data");
                    // dataReceived = "";
                    return;
                }

                dataReceived = Encoding.UTF8.GetString(buffer);

                print("[UDP INFO] UDP Data received: " + dataReceived);
            }
            catch (Exception error)
            {
                if (udpClient != null)
                {
                    Debug.LogError("[UDP ERROR] UDP Socket Exception error: " + error);
                }
                else
                {
                    Debug.LogWarning("[UDP WARNING] Thread is about to be terminated...");
                }
            }
        

        }
    }

    private void ReceiveData()
    {
        udpClient = new UdpClient(port);

        while(udpClient != null)
        {
            //
            IAsyncResult asyncResult = udpClient.BeginReceive(null, null);
            asyncResult.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(receiveTimeOut));

            if (asyncResult.IsCompleted)
            {
                try
                {
                    IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, port);
                    byte[] buffer = udpClient.Receive(ref anyIP);

                    print("[UDP INFO] UDP buffer length received: " + buffer.Length);

                    if (buffer == null)
                    {
                        Debug.LogWarning("[UDP WARNING] UDP has not received any data");
                        // dataReceived = "";
                        return;
                    }

                    dataReceived = Encoding.UTF8.GetString(buffer);

                    print("[UDP INFO] UDP Data received: " + dataReceived);
                }
                catch (Exception error)
                {
                    if (udpClient != null)
                    {
                        Debug.LogError("[UDP ERROR] UDP Socket Exception error: " + error);
                    }
                    else
                    {
                        Debug.LogWarning("[UDP WARNING] Thread is about to be terminated...");
                    }
                }
            }
            else
            {
                Debug.LogWarning("[UDP WARNING] Not received any data, timeout and begin new one");
                dataReceived = "";
            }
        }

    }

    private void ChangeLabel()
    {
        if (dataReceived != "")
        {
            string[] splitData = dataReceived.Split('$');

            labelText.text = splitData[0];
            angleText.text = splitData[1];
            rotationText.text = (Math.Round(float.Parse(splitData[1], CultureInfo.InvariantCulture) / 90f, 2)).ToString();
        }
        else
        {
            labelText.text = "SAMPLE TEXT";
            angleText.text = "SAMPLE ANGLE";
            rotationText.text = "SAMPLE ROTATION";
        }
    }

    public void PreprocessUDPData(string dataReceived)
    {
        if (dataReceived != "")
        {
            string[] splitData = dataReceived.Split('$');

            label = splitData[0];
            steerAngle = float.Parse(splitData[1], CultureInfo.InvariantCulture);
            normalizedRota = (float)Math.Round(float.Parse(splitData[1], CultureInfo.InvariantCulture), 2);
        }
        else
        {
            label = "";
            steerAngle = 0.0f;
            normalizedRota = 0.0f;
        }
    }

    private void OnDestroy()
    {
        //isCameraActive = false;
        if (udpClient != null)
        {
            udpClient.Close();

            udpClient = null;
        }
        // Client must be TURNED OFF before CLOSING THREAD
        if (receiveThread.IsAlive || receiveThread != null)
        {
            if (receiveThread.Join(100))
            {
                print("UDP Thread has closed successfully - OnDestroy");
            }
            else
            {
                print("UDP Thread did not close in 100ms, abort - OnDestroy");
                receiveThread.Abort();
            }
            //receiveThread.Abort();
            ////receiveThread.Join();

            receiveThread = null;

        }

    }
    #endregion


}

[System.Serializable]
public class ConvertedUDPData
{
    public Vector2 directionInput;
    public float steeringAngle;

    // Since most of the cars have different max rotation angles, they rely on values from -1 --> 1 to rotate.
    public float normalizedWheelRotation;   

    public ConvertedUDPData()
    {
        directionInput = Vector2.zero;
        steeringAngle = 0.0f;
        normalizedWheelRotation = 0.0f;
    }

    public ConvertedUDPData(Vector2 v2, float angle)
    {
        directionInput = new Vector2(v2.x, v2.y);
        steeringAngle = angle;
        normalizedWheelRotation = (float)Math.Round((steeringAngle / 90), 2);
    }


    public void UDP_DataConvert(string label, float angle)
    {
        // Default vars
        Vector2 defaultV2 = Vector2.zero;
        float defaultAngle = 0.0f;

        // label switch case
        switch(label.ToUpper())
        {
            case "IDLE":
                directionInput = Vector2.zero;
                steeringAngle = 0.0f;
                normalizedWheelRotation = 0.0f;
                break;

            default:
                directionInput = defaultV2;
                steeringAngle = defaultAngle;
                normalizedWheelRotation = 0.0f;
                break;
        }
    }
}
