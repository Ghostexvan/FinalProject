// Chua comment
using System;
using System.Globalization;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// [RequireComponent(typeof(UDPSocketTest1))]
/// I removed this since it added another instance of UDPSocketTest1 into the car object
/// (I forgot how this worked and it was like 3am lol)
/// I'm just going to add if (udpsock != null), since I don't want this to be too dependent on UDPSocketTest1
/// </summary>
public class CarControlNormal : MonoBehaviourPunCallbacks, IPunObservable
{
    public bool isStop = false;
    public static GameObject LocalPlayerInstance;
    public float motorTorque = 2000;
    public float currentMotorTorque = 0;
    public float brakeTorque = 2000;
    public float maxSpeed = 20;
    public float steeringRange = 30;
    public float steeringRangeAtMaxSpeed = 10;
    public float centreOfGravityOffset = -1f;

    public float heightOffset = 50f;

    WheelControl[] wheels;
    Rigidbody rigidBody;
    GameObject minimapCamera;   //// Yet to be implemented, initial script (CarControl) does have it though.

    public float vInput = 0f,
                 hInput = 0f;
    private float forwardSpeed;

    [Tooltip("The Player's UI GameObject Prefab")]
    [SerializeField]
    public GameObject PlayerUiPrefab;   //// Yet to be implemented, initial script (CarControl) does have it though.

    #region UDP Variables
    [Header("Important UDP values")]
    [HideInInspector]
    [Tooltip("Flag to check if user wanted to enable UDP or not.\nThis will be checked from UDPSocketTest1 script/component.")]
    public bool isUDPActive;
    
    [Tooltip("Said UDP script.")]
    private UDPSocketTest1 udpsock;

    [HideInInspector]
    [Tooltip("Flag to check whether keyboard input is currently active or not.")]
    public bool isKeyboardInput;   // Idea is to Input.anyKey to give a 5s timeframe for a keystroke.
                                   // This will be checked from UDPSocketTest1
                                   // ESC puts isUDPActive in here and in the UDPSocketTest1 to false (TO BE IMPLEMENTED, PROLLY NEVER)
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

    // This is used to test the vInput
    private float old_v_input;


    // Currently unrelated right now, will be used for Photon after to send vInput and hInput to the main server
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(vInput);
            stream.SendNext(hInput);

            //stream.SendNext(isBrakeCalled);
            //stream.SendNext(brakeVal);
        }
        else
        {
            this.vInput = (float)stream.ReceiveNext();
            this.hInput = (float)stream.ReceiveNext();

            //this.isBrakeCalled = (bool)stream.ReceiveNext();
            //this.brakeVal = (float)stream.ReceiveNext();
        }
    }

    private void Awake()
    {
        //if (photonView.IsMine)
        //{
        //    CarControl.LocalPlayerInstance = this.gameObject;

        //    minimapCamera = GameObject.FindGameObjectWithTag("MinimapCamera");
        //    minimapCamera.transform.SetParent(this.transform);
        //    minimapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 90f);
        //}

        // DontDestroyOnLoad(this.gameObject);

        udpsock = GameObject.Find("Claiomh").GetComponent<UDPSocketTest1>();

        old_v_input = 0f;
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
        else
        {
            Debug.LogWarning("! Either you're not the local player or there is no UDPSocketTest_Control components found anywhere. !");
            if (photonView.IsMine)
            {
                Debug.LogWarning("! UDPSocketTest_Control component is not found !");
            }
            else
            {
                Debug.LogWarning("! Not local player !");
            }
        }


        //if (isUDPActive)
        isKeyboardInput = false;

        // I can put this in Awake but it'd still initialize if this script is disabled, so I kinda opted for... here, in Start
        isBrakeCalled = false;
        brakeVal = 0.0f;

    }

    // Update is called once per frame
    void Update()
    {
        //if (!GameManager.Instance.GetGameStatus() || isStop)
        //{
        //    return;
        //}

        //if (photonView.IsMine)
        //{

        // This will be constantly checked from UDPSocketTest1
        // Việc isKeyboardInput là true hay false sẽ được xử lý ở UDPSocketTest1
        // Script này chỉ việc nhận về giá trị từ UDPSocketTest1 (cái này cũng áp dụng với các biến còn lại như vInput, hInput, isBrakeCalled, brakeVal)
        isKeyboardInput = udpsock.isKeyboardInput;


        //// Gameplay Input
        // If UDP is active then our input values are based around udpsock.cudInput,
        // which is UDPSocketTest1's ConvertedUDPData object
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
                if (old_v_input != vInput)
                    print("V-INPUT: " + vInput);


                old_v_input = vInput;
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

            if (old_v_input != vInput)
                print("V-INPUT: " + vInput);


            old_v_input = vInput;
        }

           
        //}

        //}

        // Yeah we have this, now please do the keyboard thing later
        CarEngine(vInput, hInput, isBrakeCalled, brakeVal);
        //print("Forward Speed" + GetSpeed());
 

        // Is used in here since Time.deltaTime is recommended for use in Update
        if (ifFlippedRespawn)
        {
            RespawnFlippedCar();
        }

    }

    private void FixedUpdate()
    {
        if (isStop)
        {
            rigidBody.velocity = Vector3.zero;
        }

    }

    private void LateUpdate()
    {
        //if (photonView.IsMine)
        //{
        //    minimapCamera.transform.position = this.transform.position + new Vector3(0f, heightOffset, 0f);
        //}
    }

    // This is used for normal keyboard inputs (and UDP, since we are passing 4 of these values in)
    public void CarEngine(float vInput, float hInput, bool brakeCall, float brakeVal)
    {
        // Calculate current speed in relation to the forward direction of the car
        // (this returns a negative number when traveling backwards)
        forwardSpeed = Vector3.Dot(transform.forward, rigidBody.velocity);
        print("forwardSpeed: " + forwardSpeed);
        print("vInput: " + vInput);

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
        // Và CHỈ thực hiện so sánh hướng khi độ chênh lệch lớn hơn threshold.
        // Nếu nhỏ hơn thì ta mặc định là xe đang đứng yên, và khi đó, ta muốn xe di chuyển nên isAccelerating sẽ trả giá trị True.
        bool isAccelerating = Mathf.Abs(forwardSpeed) - Mathf.Abs(vInput) - 0.01f > 0 ? Mathf.Sign(vInput) == Mathf.Sign(forwardSpeed) : true;

        //print("speedFactor:" + speedFactor);
        //print("currentMotorTorque: " + currentMotorTorque);
        //print("Mathf.Sign(vInput): " + Mathf.Sign(vInput));
        //print("Mathf.Sign(forwardSpeed): " + Mathf.Sign(forwardSpeed));

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
                wheel.WheelCollider.brakeTorque = 0f;
                //isAccelerating = true;
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

    public float GetForwardSpeed()
    {
        return this.forwardSpeed;
    }

    public float GetVelocity()
    {
        return this.rigidBody.velocity.magnitude;
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

    public void RespawnFlippedCar()
    {
        if (ifFlippedRespawn)
        {
            // If car roof is pointing slightly downwards, making the y axis and V3.Down have an angle of more than 90deg
            // This is what we'd usually call tích vô hướng
            if (Vector3.Dot(this.transform.up, Vector3.down) > 0)
            {
                elapsedRespawnTime -= Time.deltaTime;   // 

                if (elapsedRespawnTime == 0)
                {
                    this.transform.position = new Vector3(-2, 4.03f, 49.8f);
                    this.transform.rotation = Quaternion.Euler(0, 0, 0);

                    elapsedRespawnTime = 5f;
                    return;
                }
            }
            else
            {
                if (elapsedRespawnTime < 5)
                    elapsedRespawnTime = 5f;
                return;
            }
        }
        else
        {
            if (elapsedRespawnTime < 5)
                elapsedRespawnTime = 5f;
            return;
        }
    }


    public override void OnDisable()
    {
        // Always call the base to remove callbacks
        base.OnDisable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}