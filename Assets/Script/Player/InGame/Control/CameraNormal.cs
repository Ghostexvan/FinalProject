using Photon.Pun;
using UnityEngine;

// Dieu khien camera di chuyen theo nguoi choi tuong ung
public class CameraNormal : MonoBehaviourPun
{
    #region Private Serializable Fields
    // Region chua nhung truong private co the serialize
    // Serialize co the hieu don gian la cac truong co the duoc hien thi trong tab Inspector trong Unity
    // Khoang cach theo truc x, z tu nguoi choi den camera
    [Tooltip("The distance in the local x-z plane to the target")]
    [SerializeField]
    private float distance = 7.0f;

    // Chenh lech do cao giua nguoi choi va camera
    [Tooltip("The height we want the camera to be above the target")]
    [SerializeField]
    private float height = 3.0f;

    // Chenh lech theo chieu ngang giua camera va nguoi choi
    [Tooltip("Allow the camera to be offseted vertically from the target, for example giving more view of the sceneray and less ground")]
    [SerializeField]
    private Vector3 centerOffset = Vector3.zero;

    // Cho phep camera theo nguoi choi tu luc bat dau
    [Tooltip("Set this as false if a component of a prefab being instanciated by Photon Network, and manually call OnStartFollowing() when and if needed")]
    [SerializeField]
    private bool followOnStart = false;

    // Do muot cua camera khi theo nguoi choi
    [Tooltip("The Smoothing for the camera to follow the target")]
    [SerializeField]
    private float smoothSpeed = 0.125f;

    #endregion

    #region Private Fields
    // Region chua nhung truong private
    // Luu vi tri cua camera
    private Transform cameraTransform;

    // Trang thai cua camera co dang theo sau nguoi choi
    bool isFollowing;

    // Do chech lech giua camera va nguoi choi
    Vector3 cameraOffset = Vector3.zero;

    Camera cameraNorm;

    #endregion

    #region MonoBehaviour Callbacks
    private void Awake()
    {
        cameraNorm = GameObject.Find("Main Camera").GetComponent<Camera>();
    }

    // Region chua nhung ham CallBacks trong Unity
    // Ham nay duoc goi dau tien sau giai doan khoi tao Object
    void Start()
    {
        // Lay instance tuong ung cua client
        CameraNormal _camera= this.gameObject.GetComponent<CameraNormal>();

        // Kiem tra instance va client
        if (_camera != null)
        {
            //if (photonView.IsMine)
            //{
                // Cho camera theo client tuong ung
            _camera.OnStartFollowing();
            //}
        }
        else
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> CameraNormal script component on playerPrefab", this);
        }
    }

    // Ham nay duoc goi moi frame 1 lan, sau ham Update()
    void LateUpdate()
    {
        // Kiem tra xem da bat dau cho camera theo nguoi choi
        if (cameraTransform == null && isFollowing)
        {
            OnStartFollowing();
        }

        // Cho camera theo nguoi choi
        if (isFollowing)
        {
            Follow();
        }
    }

    #endregion

    #region Public Methods
    // Region chua nhung ham public
    // Su dung khi muon bat dau cho camera theo nguoi choi
    public void OnStartFollowing()
    {
        cameraTransform = cameraNorm.transform;
        isFollowing = true;

        // Lap tuc di chuyen camera den vi tri nguoi choi
        Cut();
    }

    #endregion

    #region Private Methods
    // Region chua nhung ham private
    // Dieu khien camera di chuyen theo nguoi choi
    private void Follow()
    {
        // Lay do chenh lech giua camera va nguoi choi
        cameraOffset.z = -distance;
        cameraOffset.y = height;

        // Di chuyen camera theo nguoi choi
        cameraTransform.position = Vector3.Lerp(cameraTransform.position,
                                                this.transform.position + this.transform.TransformVector(cameraOffset),
                                                smoothSpeed * Time.deltaTime);

        // Huong camera vao nguoi choi
        cameraTransform.LookAt(this.transform.position + centerOffset);
    }

    // Lap tuc di chuyen camera den vi tri nguoi choi
    private void Cut()
    {
        // Lay do chenh lech giua camera va nguoi choi
        cameraOffset.z = -distance;
        cameraOffset.y = height;

        // Nhay camera thang den vi tri nguoi choi
        cameraTransform.position = this.transform.position + this.transform.TransformVector(cameraOffset);

        // Huong camera vao nguoi choi
        cameraTransform.LookAt(this.transform.position + centerOffset);
    }

    #endregion
}
