// chua don dep
// chua comment
using UnityEngine;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    #region Private Fields

    [Tooltip("UI Text to display Player's Name")]
    [SerializeField]
    private TMP_Text playerNameText;

    private CarControl target;

    [Tooltip("Pixel offset from the player target")]
    [SerializeField]
    private Vector3 screenOffset = new Vector3(0f, 30f, 0f);

    #endregion

    #region MonoBehaviour Callbacks
    void Awake()
    {

    }

    void Update()
    {
        // Destroy itself if the target is null, It's a fail safe when Photon is destroying Instances of a Player over the network
        if (target == null)
        {
            Destroy(this.gameObject);
            return;
        }
    }

    void LateUpdate()
    {
        transform.LookAt(Camera.main.transform);
        this.transform.Rotate(0f, 180f, 0f);
    }

    #endregion

    #region Public Methods
    public void SetTarget(CarControl _target)
    {
        if (_target == null)
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> CarControl target for PlayerUI.SetTarget.", this);
            return;
        }

        // Cache references for efficiency
        target = _target;
        if (playerNameText != null)
        {
            playerNameText.text = target.photonView.Owner.NickName;
        }

        transform.SetParent(_target.gameObject.transform);
        transform.position = _target.transform.position + screenOffset;
    }

    #endregion
}
