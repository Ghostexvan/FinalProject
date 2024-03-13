// Chua comment
// Chua don dep

using System;
using System.Collections;
using System.Collections.Generic;
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

    #endregion

    #region Private Serialize Fields
    [SerializeField]
    private string actionCommand;

    [SerializeField]
    private Vector2 pointerCoordinate;

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
    private float mouseSpeed = 1.0f;

    [SerializeField]
    private float mouseDistanceSensity = 5.0f;

    #endregion

    #region Public Readonly Fields
    public readonly int port = 27001;

    #endregion

    #region Monobehaviour Callbacks
    // Start is called before the first frame update
    void Start()
    {
        pointerCoordinate = defaultPointerCoordinate;
        actionCommand = defaultActionCommand;

        InitiateUDPConnection();
    }

    // Update is called once per frame
    void Update()
    {
        PreprocessingReceivedData();
        SetMousePosition();
    }

    #endregion

    #region Private Methods
    private void InitiateUDPConnection(){
        Debug.LogWarning("[UDP INFO] Inititate UDP connection...");

        receiveThread = new Thread(new ThreadStart(ReceiveData))
        {
            IsBackground = true
        };

        receiveThread.Start();
    }

    private void ReceiveData(){
        client = new UdpClient(port);
        
        while (client != null) {
            IAsyncResult asyncResult = client.BeginReceive(null, null);
            asyncResult.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(receiveTimeOut));

            if (asyncResult.IsCompleted){
                try{
                    IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, port);
                    byte[] buffer = client.Receive(ref anyIP);

                    Debug.Log("[INFO] UDP buffer length received: " + buffer.Length);

                    if (buffer == null){
                        Debug.LogWarning("[UDP WARNING] UDP not received any data");
                        // dataReceived = "";
                        return;
                    }
                        
                    dataReceived = Encoding.UTF8.GetString(buffer);

                    Debug.Log("[UDP INFO] UDP data received: " + dataReceived);
                } catch (Exception error) {
                    if (client != null) {
                        Debug.LogError("[UDP ERROR] UDP Socket error: " + error);
                    } else {
                        Debug.LogWarning("[UDP WARNING] Thread is about to be terminate...");
                    }
                }
            } else {
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

    private void PreprocessingReceivedData(){
        if (dataReceived != ""){
            string[] preprocessedData = dataReceived.Split('-');
            
            actionCommand = preprocessedData[0];
            PreprocessingIndexFingerCoordinate(
                preprocessedData[1],
                preprocessedData[2]
            );
            
            // dataReceived = "";
        }
        else{
            actionCommand = defaultActionCommand;
            pointerCoordinate = defaultPointerCoordinate;
        }
    }

    private void PreprocessingIndexFingerCoordinate(string x, string y){
        pointerCoordinate.x = float.Parse(x);
        if (reverseX) {
            pointerCoordinate.x = 1 - pointerCoordinate.x;
        }

        pointerCoordinate.y = float.Parse(y);
        if (reverseY) {
            pointerCoordinate.y = 1 - pointerCoordinate.y;
        }

        pointerCoordinate *= new Vector2(Screen.width, Screen.height);
    }

    private void SetMousePosition(){
        if (actionCommand != "Mouse")
            return;

        Debug.Log("[MOUSE INFO] Current mouse poisiton: " + Mouse.current.position.ReadValue());
        Debug.Log("[MOUSE INFO] Target position: " + pointerCoordinate);

        if (Vector2.Distance(Mouse.current.position.ReadValue(), pointerCoordinate) > mouseDistanceSensity)
            Mouse.current.WarpCursorPosition(
                pointerCoordinate
            );
    }

    #endregion
}
