using Photon.Pun;
using UnityEngine;

public class PlayerAnimatorManager : MonoBehaviourPun
{
    #region Private Serializable Fields
    // Region chua nhung truong private co the serialize
    // Serialize co the hieu don gian la cac truong co the duoc hien thi trong tab Inspector trong Unity
    // Thoi gian chuyen huong di chuyen
    [SerializeField]
    private float directionDampTime = 0.25f;

    #endregion

    #region Private Fields
    // Region chua nhung truong private
    // Animator can dieu khien
    private Animator animator;
    
    #endregion

    #region MonoBehaviour Callbacks
    // Region chua nhung ham CallBacks trong Unity
    // Ham nay duoc goi dau tien sau giai doan khoi tao Object
    void Start()
    {
        // Lay component Animator cua GameObject
        animator = GetComponent<Animator>();
        // Neu khong ton tai -> Bao loi
        if (!animator){
            Debug.LogError("PlayerAnimatorManager is missing Animator component", this);
        }
    }

    // Ham nay duoc goi moi frame 1 lan
    void Update()
    {
        // Kiem tra xem nguoi choi co phai la client tuong ung khong
        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true){
            return;
        }

        // Khong co animator -> Khong xu ly nham tranh loi
        if (!animator){
            return;
        }

        // Xu ly viec nhay
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        // Chi nhay khi dang chay
        if (stateInfo.IsName("Base Layer.Run")){
            if (Input.GetButtonDown("Fire2")){
                animator.SetTrigger("Jump");
            }
        }

        // Hien tai dang su dung phuong phap lay input cu
        // Se sua lai sau
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        if (v < 0){
            v = 0;
        }
        animator.SetFloat("Speed", h * h + v * v);
        animator.SetFloat("Direction", h, directionDampTime, Time.deltaTime);
    }

    #endregion
}
