// Chua comment
using System;
using System.Globalization;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CarControl : MonoBehaviourPunCallbacks, IPunObservable
{
    public bool isStop = false;
    public static GameObject LocalPlayerInstance;
    public float motorTorque = 2000;
    [HideInInspector]
    public float currentMotorTorque;
    public float brakeTorque = 2000;
    public float maxSpeed = 20;
    public float steeringRange = 30;
    public float steeringRangeAtMaxSpeed = 10;
    public float centreOfGravityOffset = -1f;

    public float heightOffset = 50f;

    WheelControl[] wheels;
    Rigidbody rigidBody;
    GameObject minimapCamera;

    public float vInput = 0f,
                 hInput = 0f;
    private float forwardSpeed;

    [Tooltip("The Player's UI GameObject Prefab")]
    [SerializeField]
    public GameObject PlayerUiPrefab;

    #region UDP Variables
    [Header("Important UDP values")]
    [HideInInspector]
    [Tooltip("Flag to check if user wanted to enable UDP or not.\nThis will be checked from UDPSocketTest_Controller script/component.")]
    public bool isUDPActive;

    [Tooltip("Said UDP script.")]
    private UDPSocketTest_Controller udpsock;

    [HideInInspector]
    [Tooltip("Flag to check whether keyboard input is currently active or not.")]
    public bool isKeyboardInput;   // Idea is to Input.anyKey to give a 5s timeframe for a keystroke.
                                   // This will be checked from UDPSocket_Controller

    #endregion

    #region Car Flipped Respawn Variables
    [Tooltip("This is a flag for 'Respawns car if it's flipped'")]
    public bool ifFlippedRespawn = true;
    // This is used to respawn the car when it's flipped upside down
    private float elapsedRespawnTime;
    #endregion

    #region Car Braking Variables (Very Simple AND Scuffed)
    // This is used for Keyboard input braking
    private bool isBrakeCalled = false;
    private float brakeVal = 0.0f;
    #endregion

    #region UDP Controller Car Rates
    [Range(0.001f, 0.1f)]
    [Tooltip("Rate of brake, is used for Braking using SPACEBAR; is in the range between 0.001 - 0.1.\n" +
        "Default is 0.1. Determines how fast you can brake.")]
    // This might also be used for the UDP thing, I'm testing out things right now.
    //// To be more specific, this determines how fast you can actually brake.
    /// The car brakes completely when wheel.WheelColliders.brakeTorque = 1 * brakeTorque (public float brakeTorque, default is 2000 but we set it as 3000).
    /// To make braking somewhat more smooth, we "brakeVal": wheel.WheelColliders.brakeTorque = brakeVal * brakeTorque
    /// brakeVal starts at 0 initially, and increases to 1 at max, thus giving us ** 1 * brakeTorque **.
    //// Now with a brakingRate of 0.1f at default, brakeVal will increment 0.1 every frame. So it takes 10 frames for it to reach 1.
    public float brakingRate = 0.1f;

    [Range(0.001f, 0.1f)]
    [Tooltip("Rate of accel, is used for going forward (StaticStraight, LSteer, RSteer).\n" +
        "Is used EXCLUSIVELY for the UDP controller.")]
    //// Yes this is used EXCLUSIVELY for the UDP thing
    //// Same as brakingRate, but for going forward (or steering forward).
    /// Starts at 0 initially and increases to 1 at max, as per usual. vInput increments by accelRate (this variable, yep)
    /// We don't need this for our normal keyboard input since we already have vInput from Input.GetAxis
    public float accelRateUDP = 0.05f;

    // (Note: There's no hInput since we'd already been able to get it through the Python app)
    [Range(0.001f, 0.1f)]
    [Tooltip("Rate of reverse, is used for... Reverse.\n" +
        "Is used EXCLUSIVELY for the UDP controller.")]
    //// Used EXCLUSIVELY for the UDP thing (this is like the 3rd time I've used this)
    //// Same as brakingRate and accelRateUP
    /// We don't need this for our normal keyboard input since we already have vInput from Input.GetAxis
    public float reverseRateUDP = 0.05f;
    #endregion

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // Gửi theo thứ tự sao thì nhận (read) theo thứ tự vậy, vì ngoài bản thân LocalPlayer thì các người chơi khác
        // cũng sẽ write/SendNext theo cùng format/thứ tự này.
        if (stream.IsWriting)
        {
            stream.SendNext(vInput);
            stream.SendNext(hInput);

            stream.SendNext(isBrakeCalled);
            stream.SendNext(brakeVal);
        }
        else
        {
            this.vInput = (float)stream.ReceiveNext();
            this.hInput = (float)stream.ReceiveNext();

            this.isBrakeCalled = (bool)stream.ReceiveNext();
            this.brakeVal = (float)stream.ReceiveNext();
        }
    }

    private void Awake()
    {
        /* !!! Nếu photonView là của người chơi hiện tại (local player)
         * ==> LẤY LocalPlayerInstance, vì nó là static var nên ta có thể truy cập nó
         * ở các script khác, sử dụng CarControl.LocalPlayerInstance.
         * (Đồng thời init minimapCamera var)
          */
        if (photonView.IsMine)
        {
            CarControl.LocalPlayerInstance = this.gameObject;

            minimapCamera = GameObject.FindGameObjectWithTag("MinimapCamera");
            minimapCamera.transform.SetParent(this.transform);
            minimapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 90f);

            udpsock = GameObject.Find("GameManager").GetComponent<UDPSocketTest_Controller>();
        }
        
        // Init currentMotorTorque
        currentMotorTorque = 0f;

        // DontDestroyOnLoad(this.gameObject);

        //udpsock = GameObject.Find("GameManager").GetComponent<UDPSocketTest_Controller>();
    }

    // Start is called before the first frame update
    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        rigidBody = GetComponent<Rigidbody>();

        // Adjust center of mass vertically, to help prevent the car from rolling
        rigidBody.centerOfMass += Vector3.up * centreOfGravityOffset;

        // Find all child GameObjects that have the WheelControl script attached
        wheels = GetComponentsInChildren<WheelControl>();

        if (PlayerUiPrefab != null)
        {
            GameObject _uiGo = Instantiate(PlayerUiPrefab);
            _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
        }
        else
        {
            Debug.LogWarning("<Color=Red><a>Missing</a></Color> PlayerUiPrefab reference on player Prefab.", this);
        }


        // Check if UDP is active
        //isUDPActive = false;
        if (udpsock != null)
            isUDPActive = udpsock.isUDPActive;
        //if (isUDPActive)
        isKeyboardInput = false;

        // I can put this in Awake but it'd still initialize if this script is disabled, so I kinda opted for... here, in Start
        isBrakeCalled = false;
        brakeVal = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.Instance.GetGameStatus() || isStop)
        {
            return;
        }

        /* Nếu photonView là của người chơi hiện tại (local player) --> Nhận input từ phím (hoặc UDP) như bình thường*/
        if (photonView.IsMine)
        {
            // This will be constantly checked from UDPSocketTest_Controller
            // Việc isKeyboardInput là true hay false sẽ được xử lý ở UDPSocketTest_Controller
            // Script này chỉ việc nhận về giá trị từ UDPSocketTest và xử lý các giá trị này để cho xe chạy.
            // (cái này cũng áp dụng với các biến còn lại như vInput, hInput, isBrakeCalled, brakeVal)
            isKeyboardInput = udpsock.isKeyboardInput;

            // Nếu UDP có active --> Nhận theo UDP. Nếu không thì nhận từ phím như bình thường
            if (isUDPActive)
            {
                //
                if (isKeyboardInput == false)
                {
                    vInput = udpsock.cudInput.verticalInputValue;
                    hInput = udpsock.cudInput.normalizedWheelRotation;

                    isBrakeCalled = udpsock.cudInput.callBrake;
                    brakeVal = udpsock.cudInput.brakeValue;
                }
                else
                {
                    vInput = Input.GetAxis("Vertical");
                    hInput = Input.GetAxis("Horizontal");

                    // Added temp input for braking
                    if (Input.GetKey(KeyCode.Space))
                    {
                        //print("Spacebar down");
                        ApplyCarBrake(brakingRate);
                        //print(brakeVal);
                    }
                    else
                    {
                        if (isBrakeCalled == true) isBrakeCalled = false;
                        if (brakeVal > 0) brakeVal = 0.0f;
                    }
                }


                // print("UDP Brake Called: " + isBrakeCalled);
            }
            else
            {
                vInput = Input.GetAxis("Vertical");
                hInput = Input.GetAxis("Horizontal");

                // Added temp input for braking
                if (Input.GetKey(KeyCode.Space))
                {
                    //print("Spacebar down");
                    ApplyCarBrake(brakingRate);
                    //print(brakeVal);
                }
                else
                {
                    if (isBrakeCalled == true) isBrakeCalled = false;
                    if (brakeVal > 0) brakeVal = 0.0f;
                }
            }

            CarEngine(vInput, hInput, isBrakeCalled, brakeVal);
        }
        else
        {
            CarEngine(vInput, hInput, isBrakeCalled, brakeVal);
        }

        //// Hàm CarEngine được đặt ngoài phần input vì:
        /*
        - Các xe của người chơi khác vẫn sẽ gửi vInput, hInput, isBrakeCalled và brakeVal vào server Photon, và server
        sẽ gửi lại 4 giá trị này cho các người chơi còn lại.
        - Ngoài xe của LocalPlayerInstance thì các xe khác vẫn là GameObject trong Scene, và tụi nó chuyển động bằng cách
        nhận 4 giá trị này và đem vào hàm CarEngine cho các GameObject xe khác chạy --> Phản ánh đúng input của người chơi khác.
        
        ==> Giải thích dễ hiểu hơn (cho Bin): 
        + Ta có người chơi chính (LocalPlayerInstance) và 0-3 xe phụ của người chơi khác trong Scene.
        + Người chơi khác sẽ gửi input (4 giá trị: vInput, hInput, isBrakeCalled, brakeVal) vào Server. (kể cả bản thân mình cũng sẽ gửi)
        + Server sẽ gửi về cho LocalPlayer (và các người chơi khác) 4 input value của những người chơi còn lại
        + Các input value vừa nhận được từ Server sẽ được áp vào method CarEngine của các xe phụ (xe phụ đề cập ở dấu + thứ nhất)
        và các xe phụ sẽ chuyển động dựa vào 4 input value được gửi mỗi frame
        --> Phản ánh đúng input của người chơi khác
         */
        
    }

    private void FixedUpdate() {
        if (isStop)
        {
            rigidBody.velocity = Vector3.zero;
        }
    }
    
    // Cập nhật vị trí minimapCamera
    private void LateUpdate()
    {
        if (photonView.IsMine)
        {
            minimapCamera.transform.position = this.transform.position + new Vector3(0f, heightOffset, 0f);
        }
    }

    //----- Các hàm di chuyển xe ------------------------------------------------------------------
    // This is used for normal keyboard inputs (and UDP, since we are passing 4 of these values in)
    // This is also a general CarEngine method to move cars OTHER than the players'
    public void CarEngine(float vInput, float hInput, bool brakeCall, float brakeVal)
    {
        // Calculate current speed in relation to the forward direction of the car
        // (this returns a negative number when traveling backwards)
        forwardSpeed = Vector3.Dot(transform.forward, rigidBody.velocity);

        // Calculate how close the car is to top speed
        // as a number from zero to one
        float speedFactor = Mathf.InverseLerp(0, maxSpeed, forwardSpeed);

        // Use that to calculate how much torque is available 
        // (zero torque at top speed)
        currentMotorTorque = Mathf.Lerp(motorTorque, 0, speedFactor);

        // …and to calculate how much to steer 
        // (the car steers more gently at top speed)
        float currentSteerRange = Mathf.Lerp(steeringRange, steeringRangeAtMaxSpeed, speedFactor);

        // Check whether the user input is in the same direction 
        // as the car's velocity
        // Chỉ so sánh hướng khi độ chênh lệch giữa forwardSpeed và vInput lớn hơn 1 khoảng cho trước (threshold) là 0.01f.
        // Và CHỈ thực hiện so sánh hướng khi độ chênh leehcj lớn hơn threshold.
        // Nếu nhỏ hơn thì ta mặc định là xe đang đứng yên, và khi đó, ta muốn xe di chuyển nên isAccelerating sẽ trả giá trị True.
        bool isAccelerating = Mathf.Abs(forwardSpeed) - Mathf.Abs(vInput) - 0.01f > 0 ? Mathf.Sign(vInput) == Mathf.Sign(forwardSpeed) : true;

        foreach (var wheel in wheels)
        {
            // Apply steering to Wheel colliders that have "Steerable" enabled
            if (wheel.steerable)
            {
                wheel.WheelCollider.steerAngle = hInput * currentSteerRange;
            }

            // Check if braking is called, if it is, then brake (Newly added very bad)
            if (brakeCall)
            {
                wheel.WheelCollider.brakeTorque = Mathf.Abs(brakeVal) * brakeTorque;
                wheel.WheelCollider.motorTorque = 0;
                continue;   // Skipping the rest of the loop
            }
            else
            {
                // I think I forgot to re-adjust the value of brakeTorque WHEN we stopped braking.
                /// So when I brake using Spacebar, "brakeCall" will be True and wheel.WheelCollider.brakeTorque will increase.
                /// And when I'd completely stopped, "brakeCall" will be False, but wheel.WheelCollider.brakeTorque won't be changed (since I forgot to make it change).
                
                wheel.WheelCollider.brakeTorque = 0f;
                
            }

            if (isAccelerating)
            {
                // Apply torque to Wheel colliders that have "Motorized" enabled
                if (wheel.motorized)
                {
                    wheel.WheelCollider.motorTorque = vInput * currentMotorTorque;
                }
                wheel.WheelCollider.brakeTorque = 0;
            }
            else
            {
                // If the user is trying to go in the opposite direction
                // apply brakes to all wheels
                wheel.WheelCollider.brakeTorque = Mathf.Abs(vInput) * brakeTorque;
                wheel.WheelCollider.motorTorque = 0;
            }
        }
    }

    public void ApplyCarBrake(float brakingRate)
    {
        isBrakeCalled = true;
        brakeVal = Math.Min(brakeVal += brakingRate, 1f);
    }
    //------------------------------------------------------------------------------------------

    public void StopEngine()
    {
        isStop = true;
        rigidBody.velocity = Vector3.zero;
    }

    public void StartEngine()
    {
        isStop = false;
    }

    public float GetSpeed()
    {
        return Math.Abs(this.forwardSpeed);
    }

    public float GetForwardSpeed(){
        return this.forwardSpeed;
    }

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadingMode)
    {
        this.CalledOnLevelWasLoaded(scene.buildIndex);
    }

    void CalledOnLevelWasLoaded(int level)
    {
        GameObject _uiGo = Instantiate(this.PlayerUiPrefab);
        _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
    }

    public override void OnDisable()
    {
        // Always call the base to remove callbacks
        base.OnDisable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}