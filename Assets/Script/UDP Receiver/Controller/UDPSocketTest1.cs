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
using Unity.VisualScripting;

// This contains both UDP and changing Gameplay UI
public class UDPSocketTest1 : MonoBehaviour
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
    public TMP_Text vInputText;
    #endregion



    ////// Variables to allow user to use keyboard when UDP is running
    /* EXPLANATIONS HERE
     Khi UDP chạy, nó sẽ nhận DL liên tục. Nhưng khác với việc xài Send Message + Invoke và New Input System, ở đây ta chỉ cần
    đổi giá trị của vInput và hInput từ UDP qua Input.GetAxis("Vertical"/"Horizontal").

    Một trong những điều tiện lợi của phần này là các hàm di chuyển sử dụng chung các variables: vInput, hInput, isBrakeCalled và brakeVal.
    Khi ta thay đổi giữa Keyboard và UDP Controller thì các variables chỉ thay đổi cách nhận, thay vì phải gọi các hàm để Send Message và Invoke.
    (VD: vInput hoặc Input.GetAxis("Vertical") hoặc udpsock.cudInput.verticalInputValue lấy từ UDPSocketTest1)
     */
    // Bool for KBInput. Isn't used when not in UDP Control mode
    [HideInInspector]
    public bool isKeyboardInput;

    // Current point of time + Wait time; Ta có thể dựa vào việc xét thời điểm hoặc việc trừ elapsedTime xuống
    private float KBInputWaitTime;

    // Wait time (to get more keyboard inputs) - Khoảng tgian cố định để chờ xem có thêm Input nào từ Keyboard không
    public float TimeToWait = 5f;

    // Is used for displaying in the UI
    [HideInInspector]
    public float elapsedKBTime;


    #region Public Readonly Field (UDP Socket Port)
    public readonly int port = 27001;
    #endregion

    public ConvertedUDPData_ cudInput;

    [Header("Important UDP values")]
    [Tooltip("Flag to check if user wanted to enable UDP or not")]
    public bool isUDPActive = true;
    [Tooltip("This is purely to check whether V-Input value was different cuz I wouldn't want it to be spammed")]
    private float old_v_inp;

    [Tooltip("This is to access the CarControl script, mainly to get the brakingRate for use in BRAKE and BRAKEHOLD labels")]
    private CarControlNormal ccn;

    /// <summary>
    /// The 3 rates, related to UDP car controlling. See more details in CarControlNormal script
    /// </summary>
    private float brakingRate, accelRateUDP, reverseRateUDP;


    private void Awake()
    {
        labelText = GameObject.Find("Label (TMP)").GetComponent<TMP_Text>();
        angleText = GameObject.Find("Angle (TMP)").GetComponent<TMP_Text>();
        rotationText = GameObject.Find("Rotation (TMP)").GetComponent<TMP_Text>();
        vInputText = GameObject.Find("V-INPUT (TMP)").GetComponent<TMP_Text>();

        // Grabs the CarControlNormal component
        //ccn = GameObject.Find("Race Car Byst").GetComponent<CarControlNormal>();
        ccn = GameObject.FindGameObjectWithTag("Player").GetComponent<CarControlNormal>();
    }

    // Start is called before the first frame update
    void Start()
    {
        // isUDPActive = true;
        old_v_inp = 0.0f;
        isKeyboardInput = false;

        if (isUDPActive)
        {
            InitUDPSocket();

            cudInput = new ConvertedUDPData_();
        }

        // Initialize the rates
        brakingRate = ccn.brakingRate;
        accelRateUDP = ccn.accelRateUDP;
        reverseRateUDP = ccn.reverseRateUDP;
    }

    // Update is called once per frame
    void Update()
    {
        if (isUDPActive)
        {
            ////// If there are Keyboard Inputs, do something here
            // Sử dụng hiển thị cho UI
            // If elapsedKBTime is > 0, we decrement it bit by bit with Time.deltaTime
            if (elapsedKBTime > 0)
                elapsedKBTime -= Time.deltaTime;

            // If there are Keyboard inputs
            if (Input.anyKeyDown || Input.anyKey)
            {
                // Nếu isKeyboardInput là false --> Gọi hàm SetKeyboardInput
                if (isKeyboardInput == false)
                    SetKeyboardInput();
                // Nếu isKeyboardInput là true --> Cập nhật KBInputWaitTime (thêm 5s) và elapsedKBTime
                else
                {
                    KBInputWaitTime = Time.time + TimeToWait;
                    //print("KBInputWaitTime: " + KBInputWaitTime);
                    elapsedKBTime = TimeToWait;
                }
            }

            // Nếu thời điểm hiện tại > Thời điểm dùng để chờ thêm KB Input ==> isKBInput = false, sd lại đc UDP
            if (Time.time > KBInputWaitTime)
            {
                isKeyboardInput = false;
            }
            ////// END of Keyboard Input time limit processing
            

            ////// We put this first, BEFORE we process our new data
            // old_v_inp will have the vInput value in the frame prior, hence why we put it BEFORE the UDP_DataConvert.
            // (Note that old_v_inp is only used for printing/logging)
            old_v_inp = cudInput.verticalInputValue;

            ////// Now we process our data
            PreprocessUDPData(dataReceived);
            // cudInput.UDP_DataConvert(label, steerAngle);     // Old ver

            
            cudInput.UDP_DataConvert_v2(label, steerAngle, brakingRate, accelRateUDP, reverseRateUDP);   
            ////// Added brakingRate from CarControlNormal for last param, is used to determine how fast the player wants to brake their vehicle.
            // With brakingRate at 0.1, it'd increment 0.1 every frame, so it takes 10 FRAMES for the car to completely stop.
            // (Since we're going for wheel.WheelCollider.brakeTorque = (brakeVal += brakingRate) * brakeTorque,
            // with (brakeVal += brakingRate) being 1 at max.)

            ////// Now we comapre old_v_inp with our newly converted data in this frame
            if (old_v_inp != cudInput.verticalInputValue)
                print(cudInput.verticalInputValue);

            ////// If them UI elements don't exist --> Don't run this method
            if (labelText != null && angleText != null && rotationText != null && vInputText != null)
                ChangeLabel_v2(cudInput, label, steerAngle, cudInput.normalizedWheelRotation); //normalizedRota

            // Now we had already gotten the data we needed
            // (vInput -- normalizedWheelRotation; hInput -- verticalInputValue; isBrakeCalled -- callBrake; brakeVal -- brakeValue)
            // , we need to (somehow) transfer it to our CarControlNormal script

            //ChangeLabel();
        }
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
                //IPAddress ip = IPAddress.Parse("127.0.0.1");
                //IPEndPoint anyIP = new IPEndPoint(ip, port);
                byte[] buffer = udpClient.Receive(ref anyIP);

                //print("[UDP INFO] UDP buffer length received: " + buffer.Length);

                if (buffer == null)
                {
                    Debug.LogWarning("[UDP WARNING] UDP has not received any data");
                    // dataReceived = "";
                    return;
                }

                dataReceived = Encoding.UTF8.GetString(buffer);

                //print("[UDP INFO] UDP Data received: " + dataReceived);
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

    public void PreprocessUDPData(string dataReceived)
    {
        if (dataReceived != "")
        {
            string[] splitData = dataReceived.Split('$');

            label = splitData[0];
            steerAngle = float.Parse(splitData[1], CultureInfo.InvariantCulture);
            normalizedRota = (float)Math.Round(float.Parse(splitData[1], CultureInfo.InvariantCulture) / 90f, 2);
        }
        else
        {
            label = "";
            steerAngle = 0.0f;
            normalizedRota = 0.0f;
        }
    }

    private void ChangeLabel()
    {
        if (dataReceived != "")
        {
            string[] splitData = dataReceived.Split('$');

            labelText.text = splitData[0];
            angleText.text = splitData[1];
            rotationText.text = "<b>" + "H-Input: " + "</b>" + "<i><size=90%>" + (Math.Round(float.Parse(splitData[1], CultureInfo.InvariantCulture) / 90f, 2)).ToString() + "</i>";

            //<b>V-INPUT:</b> <i><size=90%>0.00</i>
            //vInputText.text = "V-Input: " + 
        }
        else
        {
            labelText.text = "SAMPLE TEXT";
            angleText.text = "SAMPLE ANGLE";
            rotationText.text = "SAMPLE ROTATION";
        }
    }

    private void ChangeLabel_v2(ConvertedUDPData_ cudObj, string labelTxt, float angleTxt, float normRota)
    {
        // I can use the global variables in this class (UDPSocketTest1's vars) but I'm putting them as params instead
        // since I wanted to test things.
        if (isUDPActive)
        {
            if (dataReceived != "")
            {
                labelText.text = labelTxt;
                angleText.text = angleTxt.ToString();
                rotationText.text = "H-Input: " + normRota; //(cudObj.normalizedWheelRotation).ToString();
                vInputText.text = "V-Input: " + (cudObj.verticalInputValue).ToString();
            }
            else
            {
                labelText.text = "SAMPLE TEXT";
                angleText.text = "SAMPLE ANGLE";
                rotationText.text = "SAMPLE ROTATION";
                vInputText.text = "New Text";
            }
        }
    }

    private void OnDestroy()
    {
        if (isUDPActive)
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

    }

    //private void OnApplicationQuit()
    //{
    //    if (isUDPActive)
    //    {
    //        //isCameraActive = false;
    //        if (udpClient != null)
    //        {
    //            udpClient.Close();

    //            udpClient = null;
    //        }
    //        // Client must be TURNED OFF before CLOSING THREAD
    //        if (receiveThread.IsAlive || receiveThread != null)
    //        {
    //            if (receiveThread.Join(100))
    //            {
    //                print("UDP Thread has closed successfully - OnDestroy");
    //            }
    //            else
    //            {
    //                print("UDP Thread did not close in 100ms, abort - OnDestroy");
    //                receiveThread.Abort();
    //            }
    //            //receiveThread.Abort();
    //            ////receiveThread.Join();

    //            receiveThread = null;

    //        }
    //    }
    //}
    #endregion

    #region Keyboard Input when UDP is active
    public void SetKeyboardInput()
    {
        // If UDP Controller is not active, this won't be called
        if (!isUDPActive)
            return;

        // 
        isKeyboardInput = true;

        // Thời điểm để xét có thêm input từ Keyboard hay không
        //KBInputWaitTime = Time.realtimeSinceStartup + TimeToWait;
        KBInputWaitTime = Time.time + TimeToWait;
        print("KBInputWaitTime: " + KBInputWaitTime);

        // Giá trị time để hiển thị trên UI
        elapsedKBTime = TimeToWait;
    }
    #endregion
}

// There will be fixes to this soon, once I'm able to improve my CAR ENGINE code that is.
[System.Serializable]
public class ConvertedUDPData_
{
    public Vector2 directionInput;
    public float steeringAngle;

    // Since most of the cars have different max rotation angles, they rely on values from -1 --> 1 to rotate.
    // This is technically the hInput
    public float normalizedWheelRotation;

    // This is the vInput
    public float verticalInputValue;


    // Bool/Flag to call CarControl's brake
    public bool callBrake;

    // This is used for multiplication with CarControlNormal's brakeTorque (brakeTorque is pretty much used for braking the car)
    // Since the 4 wheels' brake value is determined using: wheel.WheelCollider.brakeTorque = Math.Abs(brakeValue) * brakeTorque
    // When this reaches 1, brakeTorque will be at its max value, meaning our vehicle will stop completely.
    public float brakeValue;
    // This will be incremented by "brakingRate" (taken from CarControlNormal component) every frame


    public ConvertedUDPData_()
    {
        directionInput = Vector2.zero;
        steeringAngle = 0.0f;

        // Current car operating variables
        normalizedWheelRotation = 0.0f;
        verticalInputValue = 0.0f;
        callBrake = false;
        brakeValue = 0.0f;
    }

    public ConvertedUDPData_(Vector2 v2, float angle)
    {
        directionInput = new Vector2(v2.x, v2.y);
        steeringAngle = angle;

        // Current car operating variables
        normalizedWheelRotation = (float)Math.Round((steeringAngle / 90), 2);
        verticalInputValue = 0.0f;
        callBrake = false;
        brakeValue = 0.0f;
    }

    /// <summary>
    /// The OLD version of the DataConvert method. (DEPRECATED, USE v2)
    /// Is used to convert the "label$angle" received from the Python app (using UDP) to data that our CarControlNormal script
    /// can use to operate our vehicle.
    /// 
    /// Then again, this version is old and isn't really usable with the newly modified CarControlNormal (which also had methods
    /// for braking instead of having the user press the opposite key to brake).
    /// </summary>
    /// <param name="label"></param>
    /// <param name="angle"></param>
    public void UDP_DataConvert(string label, float angle)
    {
        // Default vars
        Vector2 defaultV2 = Vector2.zero;
        float defaultAngle = 0.0f;

        // This is the default case / When dataReceived == ""
        if (label == "" && angle == 0.0f)
        {
            normalizedWheelRotation = 0.0f;
            verticalInputValue = 0.0f;
            return;
        }


        // label switch case
        switch(label.ToUpper())
        {
            case "IDLE":
                directionInput = Vector2.zero;
                steeringAngle = 0.0f;

                // Current Input system
                normalizedWheelRotation = 0.0f;
                verticalInputValue = 0.0f;
                break;
            case "STATICSTRAIGHT":
                directionInput = new Vector2(1, 0);
                steeringAngle = 0f;
                // For use in future I hope
                //if (normalizedWheelRotation < 0)
                //    normalizedWheelRotation = Math.Min(steeringAngle += .2f, 0f);
                //else
                //    normalizedWheelRotation = Math.Max(steeringAngle -= .2f, 0f);

                normalizedWheelRotation = 0.0f;
                verticalInputValue = Math.Min(verticalInputValue += 0.05f, 1f);
                //if (verticalInputValue < 1)
                //    verticalInputValue += 0.1f;
                //else
                //    verticalInputValue = 1f;
                break;
            case "LSTEER":
                directionInput = new Vector2(-1, 1);
                steeringAngle = angle;

                normalizedWheelRotation = (float)Math.Round((steeringAngle / 90), 2);
                // idk if I needed to put verticalInputValue here since steering doesn't really affect vertical input
                // unless you Brake, I doubt you'd want to decrease your vertical input
                break;
            case "RSTEER":
                directionInput = new Vector2(1, 1);
                steeringAngle = angle;

                normalizedWheelRotation = (float)Math.Round((steeringAngle / 90), 2);
                // idk if I needed to put verticalInputValue here since steering doesn't really affect vertical input
                // unless you Brake, I doubt you'd want to decrease your vertical input
                break;
            // "BRAKE" and "BRAKEHOLD" might be different in the future
            case "BOOST":

                break;

            case "BRAKE":
                directionInput = Vector2.zero;
                steeringAngle = angle;

                normalizedWheelRotation = (float)Math.Round((steeringAngle / 90), 2);
                if (verticalInputValue < 0)
                    verticalInputValue = Math.Min(verticalInputValue += 0.1f, 0f);
                else if (verticalInputValue > 0)
                    verticalInputValue = Math.Max(verticalInputValue -= 0.1f, 0f);
                else
                    verticalInputValue = 0f;
                break;
            case "BRAKEHOLD":
                directionInput = Vector2.zero;
                steeringAngle = angle;

                //normalizedWheelRotation = 0.0f;
                //normalizedWheelRotation = (float)Math.Round((steeringAngle / 90), 2);
                if (normalizedWheelRotation < 0)
                    normalizedWheelRotation = Math.Min(steeringAngle += .2f, 0f);
                else
                    normalizedWheelRotation = Math.Max(steeringAngle -= .2f, 0f);
                verticalInputValue = 0.0f;
                break;
            case "REVERSE":
                directionInput = new Vector2();
                steeringAngle = angle;

                normalizedWheelRotation = (float)Math.Round((steeringAngle / 90), 2);
                verticalInputValue = Math.Max(verticalInputValue -= 0.05f, -1f);
                break;
            // Now idrk what to do with this since this shows up when either:
            // There's no body landmarks detected
            // Or when 
            case "NONE":
                if (normalizedWheelRotation == 0)
                    normalizedWheelRotation = 0f;

                if (verticalInputValue == 0)
                    verticalInputValue = 0f;

                break;

            default:
                directionInput = defaultV2;
                steeringAngle = defaultAngle;
                normalizedWheelRotation = 0.0f;
                break;
        }
    }
    // DEPRECATED, USE v2!!!

    // I can't access the 3 last variables since this class (ConvertedUDPData_) can't access them from here. Reason being it's not a MonoBeh.
    // So I had to pass them as variables: by default they'd be 0.1f _ 0.05f _ 0.05f
    public void UDP_DataConvert_v2(string label, float angle, float brakingRate, float accelRateUDP, float reverseRateUDP)
    {
        // (NOTE: brakingRate param will be taken from the CarControlNormal component)

        // Default vars
        Vector2 defaultV2 = Vector2.zero;
        float defaultAngle = 0.0f;

        // This is the default case / When dataReceived == ""
        if (label == "" && angle == 0.0f)
        {
            normalizedWheelRotation = 0.0f;
            verticalInputValue = 0.0f;
            return;
        }


        // label switch case
        switch (label.ToUpper())
        {
            case "IDLE":
                directionInput = Vector2.zero;
                steeringAngle = 0.0f;

                ////// Current Car controlling system
                /// I mean... We ARE idling, are we not? Makes sense the car will be stopping right then and there.
                normalizedWheelRotation = 0.0f;
                verticalInputValue = 0.0f;
                brakeValue = 1.0f;
                callBrake = true;
                break;
            case "STATICSTRAIGHT":
                directionInput = new Vector2(1, 0);
                steeringAngle = 0f;
                // For use in future I hope
                //if (normalizedWheelRotation < 0)
                //    normalizedWheelRotation = Math.Min(steeringAngle += .2f, 0f);
                //else
                //    normalizedWheelRotation = Math.Max(steeringAngle -= .2f, 0f);
                //if (verticalInputValue < 1)
                //    verticalInputValue += 0.1f;
                //else
                //    verticalInputValue = 1f;

                normalizedWheelRotation = 0.0f;
                verticalInputValue = Math.Min(verticalInputValue += accelRateUDP, 1f);
                brakeValue = 0.0f;
                callBrake = false;

                break;
            case "LSTEER":
                directionInput = new Vector2(-1, 1);
                steeringAngle = angle;

                ////// Current Car controlling system
                // - Putting angle != 0 in case the webcam in the UDP app wasn't able to get the 2 hand landmarks, thus not being
                // able to calculate the angle of 2 hands/the steering angle.
                // - This will let normalizedWheelRotation keep its old value (value from the frame prior).
                if (angle != 0)
                    normalizedWheelRotation = (float)Math.Round((steeringAngle / 90), 2);
                verticalInputValue = Math.Min(verticalInputValue += accelRateUDP, 1f);
                // - idk if I needed to put verticalInputValue here since steering doesn't really affect vertical input
                // unless you Brake or Reverse, I doubt you'd want to decrease your vertical input.
                // - In short, steering is a combination of forward + angle. So we have both hInput and vInput here.
                brakeValue = 0.0f;
                callBrake = false;
                break;
            case "RSTEER":
                directionInput = new Vector2(1, 1);
                steeringAngle = angle;

                // - Putting angle != 0 in case the webcam in the UDP app wasn't able to get the 2 hand landmarks, thus not being
                // able to calculate the angle of 2 hands/the steering angle.
                // - This will let normalizedWheelRotation keep its old value (value in the frame prior).
                if (angle != 0)
                    normalizedWheelRotation = (float)Math.Round((steeringAngle / 90), 2);
                verticalInputValue = Math.Min(verticalInputValue += accelRateUDP, 1f);
                // - idk if I needed to put verticalInputValue here since steering doesn't really affect vertical input
                // unless you Brake or Reverse, I doubt you'd want to decrease your vertical input
                // - In short, steering is a combination of forward + angle. So we have both hInput and vInput here.
                brakeValue = 0.0f;
                callBrake = false;
                break;
            // "BRAKE" and "BRAKEHOLD" might be different in the future
            case "BOOST":

                ////// Current Car controlling system
                brakeValue = 0.0f;
                callBrake = false;
                break;

            case "BRAKE":
                directionInput = Vector2.zero;
                steeringAngle = angle;

                /* NOTE: THIS IS DEPRECATED, I WAS JUST TESTING THINGS OUT
                if (currentCarSpeed > 0.0001)
                {
                    // hInput
                    if (normalizedWheelRotation < 0)
                        normalizedWheelRotation = Math.Min(normalizedWheelRotation += .2f, 0f);
                    else
                        normalizedWheelRotation = Math.Max(normalizedWheelRotation -= .2f, 0f);
                    // vInput
                    if (verticalInputValue < 0)
                        verticalInputValue = Math.Min(verticalInputValue += 0.1f, 1f);
                    else
                        verticalInputValue = Math.Max(verticalInputValue -= 0.1f, -1f);
                    //else
                    //    verticalInputValue = 0f;
                }
                else
                {
                    normalizedWheelRotation = (float)Math.Round((steeringAngle / 90), 2);
                    //if (verticalInputValue < 0)
                    //    verticalInputValue = Math.Min(verticalInputValue += 0.1f, 0f);
                    //else if (verticalInputValue > 0)
                    //    verticalInputValue = Math.Max(verticalInputValue -= 0.1f, 0f);
                    //else
                    verticalInputValue = 0f;
                }
                */

                ////// Current Car controlling system
                // vInput and hInputs are somewhat not affected, but I'll make it so that vInput is 0
                callBrake = true;
                brakeValue = Math.Min(brakeValue += brakingRate, 1f);
                break;
            case "BRAKEHOLD":
                directionInput = Vector2.zero;
                steeringAngle = angle;

                /* NOTE: THIS IS DEPRECATED, I WAS JUST TESTING THINGS OUT  
                normalizedWheelRotation = 0.0f;
                normalizedWheelRotation = (float)Math.Round((steeringAngle / 90), 2);
                if (currentCarSpeed > 0.0001)
                {
                    // hInput
                    if (normalizedWheelRotation < 0)
                        normalizedWheelRotation = Math.Min(normalizedWheelRotation += .1f, 0f);
                    else
                        normalizedWheelRotation = Math.Max(normalizedWheelRotation -= .1f, 0f);
                    // vInput
                    if (verticalInputValue < 0)
                        verticalInputValue = Math.Min(verticalInputValue += 0.1f, 1f);
                    else if (verticalInputValue > 0)
                        verticalInputValue = Math.Max(verticalInputValue -= 0.1f, -1f);
                    else
                        verticalInputValue = 0f;
                }
                else
                {
                    //normalizedWheelRotation = 0f;   // Slightly different from BRAKE (im lazy)

                    if (normalizedWheelRotation < 0)
                        normalizedWheelRotation = Math.Min(steeringAngle += .2f, 0f);
                    else
                        normalizedWheelRotation = Math.Max(steeringAngle -= .2f, 0f);
                    //if (verticalInputValue < 0)
                    //    verticalInputValue = Math.Min(verticalInputValue += 0.1f, 0f);
                    //else if (verticalInputValue > 0)
                    //    verticalInputValue = Math.Max(verticalInputValue -= 0.1f, 0f);
                    //else
                    verticalInputValue = 0f;
                }

                if (normalizedWheelRotation < 0)
                    normalizedWheelRotation = Math.Min(steeringAngle += .2f, 0f);
                else
                    normalizedWheelRotation = Math.Max(steeringAngle -= .2f, 0f);
                verticalInputValue = 0.0f;
                */

                ////// Current Car controlling system
                callBrake = true;
                brakeValue = Math.Min(brakeValue += brakingRate, 1f);
                break;
            case "REVERSE":
                directionInput = new Vector2();
                steeringAngle = angle;

                ////// Current Car controlling system
                normalizedWheelRotation = (float)Math.Round((steeringAngle / 90), 2);
                verticalInputValue = Math.Max(verticalInputValue -= reverseRateUDP, -1f);
                callBrake = false;
                brakeValue = 0.0f;
                break;
            // Now idrk what to do with this since this shows up when either:
            // There's no body landmarks detected
            // Or when our detections' accuracy are below our threshold (this is defined in the Python app, default to be 0.5 or 0.6)
            case "NONE":

                ////// Current Car controlling system
                if (normalizedWheelRotation == 0)
                    normalizedWheelRotation = 0f;

                if (verticalInputValue == 0)
                    verticalInputValue = 0f;
                // No braking here
                break;

            default:
                directionInput = defaultV2;
                steeringAngle = defaultAngle;

                ////// Current Car controlling system
                normalizedWheelRotation = 0.0f;
                verticalInputValue = 0.0f;
                brakeValue = 0.0f;
                callBrake = false;
                break;
        }
    }
}
