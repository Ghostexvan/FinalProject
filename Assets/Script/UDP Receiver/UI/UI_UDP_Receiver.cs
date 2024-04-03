// Chua comment
// Chua don dep

using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

public class UI_UDP_Receiver : MonoBehaviour
{
    #region Private Fields
    private Thread receiveThread;
    private UdpClient client;
    private string dataReceived = "";
    private string currentAction = null;
    private bool isCooldownSlide = false;
    private Vector2 pointerCoordinate;
    private string actionCommand;

    #endregion

    #region Private Serialize Fields
    [SerializeField]
    private string defaultActionCommand = "Wait";

    [SerializeField]
    private Vector2 defaultPointerCoordinate = new Vector2(0, 0);

    [SerializeField]
    private double receiveTimeOut = 1f;

    [SerializeField]
    private bool reverseX = false;

    [SerializeField]
    private bool reverseY = false;

    [SerializeField]
    private bool reverseVerticalSwipe = false;

    [SerializeField]
    private bool reverseHorizontalSwipe = false;

    [SerializeField]
    private bool reverseVerticalSlide = false;

    [SerializeField]
    private bool reverseHorizontalSlide = false;

    [SerializeField]
    private float slideCooldown = 1.0f;

    #endregion

    #region Public Readonly Fields
    public readonly int port = 27002;

    #endregion

    #region Public Fields
    [Header("Important UDP values")]
    [Tooltip("Flag to check if user wanted to enable UDP or not. This is mostly for debugging.")]
    public bool isUDPActive = true;
    #endregion

    #region Monobehaviour Callbacks
    private void Awake()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        pointerCoordinate = defaultPointerCoordinate;
        actionCommand = defaultActionCommand;

        if (isUDPActive)
            InitiateUDPConnection();
    }

    // Update is called once per frame
    void Update()
    {
        if (isUDPActive)
        {
            PreprocessingReceivedData();

            SendingCommand();
        }
    }

    private void OnDestroy()
    {
        if (isUDPActive)
        {
            if (client != null)
            {
                client.Close();
                client = null;
            }

            if (receiveThread.IsAlive || receiveThread != null)
            {
                if (receiveThread.Join(100))
                {
                    Debug.Log("[UDP INFO] (UI) Thread closed");
                }
                else
                {
                    Debug.LogWarning("[UDP WARNING] Thread did not close, time out");
                    receiveThread.Abort();
                }
            }

            receiveThread = null;
        }

    }

    #endregion

    #region Private Methods
    private void InitiateUDPConnection()
    {
        Debug.LogWarning("[UDP INFO] Inititate UDP (UI) connection...");

        receiveThread = new Thread(new ThreadStart(ReceiveData))
        {
            IsBackground = true
        };

        receiveThread.Start();
    }

    private void ReceiveData()
    {
        client = new UdpClient(port);

        while (client != null)
        {
            IAsyncResult asyncResult = client.BeginReceive(null, null);
            asyncResult.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(receiveTimeOut));

            if (asyncResult.IsCompleted)
            {
                try
                {
                    //// !!! IMPORTANT !!!
                    //// Using IPAddress.any will lead to us receiving packets from Photon's servers instead.
                    // Since they also have ports 27001 and 27002. So instead of using 0.0.0.0, we'll only
                    // be using 127.0.0.1 (our local IP).
                    // https://doc.photonengine.com/server/current/operations/tcp-and-udp-port-numbers
                    // Reason being: We only wanted our UDP data to be sent from our Python app to here in order
                    // for our scripts to process our UDP Data into inputs. Since 0.0.0.0 takes any IPs, it can literally
                    // get UDP data from Photon Master Servers and Game Servers themselves, which we don't want.
                    //IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, port);

                    IPAddress ip = IPAddress.Parse("127.0.0.1");
                    IPEndPoint anyIP = new IPEndPoint(ip, port);
                    byte[] buffer = client.Receive(ref anyIP);

                    Debug.Log("[INFO] UDP buffer length received: " + buffer.Length);

                    if (buffer == null)
                    {
                        Debug.LogWarning("[UDP WARNING] UDP not received any data");
                        // dataReceived = "";
                        return;
                    }

                    dataReceived = Encoding.UTF8.GetString(buffer);

                    Debug.Log("[UDP INFO] UDP data received: " + dataReceived);
                }
                catch (Exception error)
                {
                    if (client != null)
                    {
                        Debug.LogError("[UDP ERROR] UDP Socket error: " + error);
                    }
                    else
                    {
                        Debug.LogWarning("[UDP WARNING] Thread is about to be terminate...");
                    }
                }
            }
            else
            {
                Debug.LogWarning("[UDP WARNING] Not received any data, timeout and begin new one");
                dataReceived = "";
            }
        }

        // while(client != null){
        //     // Debug.Log("Got client");
        //     try{
        //         // Debug.Log("Try receiving data");
        //         IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, port);
        //         byte[] buffer = client.Receive(ref anyIP);

        //         Debug.Log("[INFO] UDP buffer length received: " + buffer.Length);

        //         if (buffer == null){
        //             Debug.LogWarning("[UDP WARNING] UDP not received any data");
        //             dataReceived = "";
        //             return;
        //         }

        //         dataReceived = Encoding.UTF8.GetString(buffer);

        //         Debug.Log("[UDP INFO] UDP data received: " + dataReceived);
        //     } catch (Exception error) {
        //         if (client != null) {
        //             Debug.LogError("[UDP ERROR] UDP Socket error: " + error);
        //         } else {
        //             Debug.LogWarning("[UDP WARNING] Thread is about to be terminate...");
        //         }
        //     }
        //     // Debug.Log("Receive data complete");
        // }
    }

    private void PreprocessingReceivedData()
    {
        if (dataReceived != "")
        {
            string[] preprocessedData = dataReceived.Split('-');

            actionCommand = preprocessedData[0];
            PreprocessingIndexFingerCoordinate(
                preprocessedData[1],
                preprocessedData[2]
            );

            // dataReceived = "";
        }
        else
        {
            actionCommand = defaultActionCommand;
            pointerCoordinate = defaultPointerCoordinate;
        }
    }

    private void PreprocessingIndexFingerCoordinate(string x, string y)
    {
        float.TryParse(x, out pointerCoordinate.x);
        if (reverseX)
        {
            pointerCoordinate.x = 1 - pointerCoordinate.x;
        }

        float.TryParse(y, out pointerCoordinate.y);
        if (reverseY)
        {
            pointerCoordinate.y = 1 - pointerCoordinate.y;
        }

        pointerCoordinate *= new Vector2(Screen.width, Screen.height);
    }

    private void SendingCommand()
    {
        switch (actionCommand)
        {
            case "Wait":
                currentAction = "Wait";
                break;

            case "Return":
                if (currentAction != "Wait")
                {
                    break;
                }

                currentAction = "Return";
                MouseController.Instance.Back();
                break;

            case "Mouse":
                currentAction = "Mouse";
                MouseController.Instance.SetMousePosition(pointerCoordinate);
                break;

            case "Click":
                if (currentAction != "Wait" &&
                    currentAction != "Mouse")
                {
                    break;
                }

                currentAction = "Click";
                MouseController.Instance.Click();
                break;

            case "SwipeUp":
                if (currentAction != "Wait")
                {
                    break;
                }

                if (!this.reverseVerticalSwipe){
                    MouseController.Instance.ScrollUp();
                } else {
                    MouseController.Instance.ScrollDown();
                }
                
                currentAction = "SwipeUp";
                break;

            case "SwipeDown":
                if (currentAction != "Wait")
                {
                    break;
                }

                if (!this.reverseVerticalSwipe){
                    MouseController.Instance.ScrollDown();
                } else {
                    MouseController.Instance.ScrollUp();
                }

                currentAction = "SwipeDown";
                break;
            
            case "SwipeLeft":
                if (currentAction != "Wait")
                {
                    break;
                }

                if (!this.reverseHorizontalSwipe){
                    MouseController.Instance.ScrollLeft();
                } else {
                    MouseController.Instance.ScrollRight();
                }

                currentAction = "SwipeLeft";
                break;
            
            case "SwipeRight":
                if (currentAction != "Wait")
                {
                    break;
                }

                if (!this.reverseHorizontalSwipe){
                    MouseController.Instance.ScrollRight();
                } else {
                    MouseController.Instance.ScrollLeft();
                }
                currentAction = "SwipeRight";
                break;
            
            case "SlideUp":
                if (this.isCooldownSlide){
                    return;
                }

                if (currentAction != "Wait" &&
                    currentAction != "SlideUp")
                {
                    break;
                }

                if (!this.reverseVerticalSlide) {
                    MouseController.Instance.SlideUp();
                } else {
                    MouseController.Instance.SlideDown();
                }
                currentAction = "SlideUp";
                StartCoroutine(CooldownSlide());
                break;
            
            case "SlideDown":
                if (this.isCooldownSlide){
                    return;
                }

                if (currentAction != "Wait" &&
                    currentAction != "SlideDown")
                {
                    break;
                }

                if (!this.reverseVerticalSlide) {
                    MouseController.Instance.SlideDown();
                } else {
                    MouseController.Instance.SlideUp();
                }
                currentAction = "SlideDown";
                StartCoroutine(CooldownSlide());
                break;
            
            case "SlideLeft":
                if (this.isCooldownSlide){
                    return;
                }

                if (currentAction != "Wait" &&
                    currentAction != "SlideLeft")
                {
                    break;
                }

                if (!this.reverseHorizontalSlide) {
                    MouseController.Instance.SlideLeft();
                } else {
                    MouseController.Instance.SlideRight();
                }
                currentAction = "SlideLeft";
                StartCoroutine(CooldownSlide());
                break;
            
            case "SlideRight":
                if (this.isCooldownSlide){
                    return;
                }

                if (currentAction != "Wait" &&
                    currentAction != "SlideRight")
                {
                    break;
                }

                if (!this.reverseHorizontalSlide) {
                    MouseController.Instance.SlideRight();
                } else {
                    MouseController.Instance.SlideLeft();
                }
                currentAction = "SlideRight";
                StartCoroutine(CooldownSlide());
                break;
        }
    }

    #endregion

    #region IEnumerator Methods
    IEnumerator CooldownSlide(){
        this.isCooldownSlide = true;
        yield return new WaitForSeconds(this.slideCooldown);
        this.isCooldownSlide = false;
    }

    #endregion
}
