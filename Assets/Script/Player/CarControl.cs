// Chua comment
using System;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CarControl : MonoBehaviourPunCallbacks, IPunObservable
{
    public bool isStop = false;
    public static GameObject LocalPlayerInstance;
    public float motorTorque = 2000;
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

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(vInput);
            stream.SendNext(hInput);
        }
        else
        {
            this.vInput = (float)stream.ReceiveNext();
            this.hInput = (float)stream.ReceiveNext();
        }
    }

    private void Awake()
    {
        if (photonView.IsMine)
        {
            CarControl.LocalPlayerInstance = this.gameObject;

            minimapCamera = GameObject.FindGameObjectWithTag("MinimapCamera");
            minimapCamera.transform.SetParent(this.transform);
            minimapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 90f);
        }

        // DontDestroyOnLoad(this.gameObject);
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
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.Instance.GetGameStatus())
        {
            return;
        }

        if (photonView.IsMine)
        {
            vInput = Input.GetAxis("Vertical");
            hInput = Input.GetAxis("Horizontal");
        }

        if (isStop)
        {
            rigidBody.velocity = Vector3.zero;
            return;
        }

        // Calculate current speed in relation to the forward direction of the car
        // (this returns a negative number when traveling backwards)
        forwardSpeed = Vector3.Dot(transform.forward, rigidBody.velocity);

        // Calculate how close the car is to top speed
        // as a number from zero to one
        float speedFactor = Mathf.InverseLerp(0, maxSpeed, forwardSpeed);

        // Use that to calculate how much torque is available 
        // (zero torque at top speed)
        float currentMotorTorque = Mathf.Lerp(motorTorque, 0, speedFactor);

        // …and to calculate how much to steer 
        // (the car steers more gently at top speed)
        float currentSteerRange = Mathf.Lerp(steeringRange, steeringRangeAtMaxSpeed, speedFactor);

        // Check whether the user input is in the same direction 
        // as the car's velocity
        bool isAccelerating = Mathf.Sign(vInput) == Mathf.Sign(forwardSpeed);

        foreach (var wheel in wheels)
        {
            // Apply steering to Wheel colliders that have "Steerable" enabled
            if (wheel.steerable)
            {
                wheel.WheelCollider.steerAngle = hInput * currentSteerRange;
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

    private void LateUpdate()
    {
        if (photonView.IsMine)
        {
            minimapCamera.transform.position = this.transform.position + new Vector3(0f, heightOffset, 0f);
        }
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