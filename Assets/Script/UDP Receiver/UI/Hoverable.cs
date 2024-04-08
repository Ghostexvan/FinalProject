using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Hoverable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    //[SerializeField]
    private SoundHandler soundHandler;
    private bool isExit;

    private void Awake()
    {
        // This will work since SFX Handler GameObject is active the whole time, meaning
        // its activeSelf and activeIH are True. Meaning you can find it  using GameObject.Find
        soundHandler = GameObject.Find("SFX Handler").GetComponent<SoundHandler>();

        // Awake won't run for some GameObjects IF they're inactive (activeIH = False), but if
        // they do manage to become active then we'll still be able to get SoundHandler nonetheless.
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("[MOUSE INFO] Mouse hovering over: " + this.gameObject.name);

        if (TryGetComponent<Button>(out Button button) || TryGetComponent<TMP_Dropdown>(out TMP_Dropdown dropdown)){
            MouseController.Instance.SetHoveringButton(this.gameObject);
            soundHandler.PlaySoundHover();
        }
        
        if (TryGetComponent<ScrollRect>(out ScrollRect scrollRect) || TryGetComponent<CarSelectView>(out CarSelectView carSelectView) || TryGetComponent<TMP_Dropdown>(out TMP_Dropdown _dropdown)){
            MouseController.Instance.SetHoveringScrollView(this.gameObject);
            soundHandler.PlaySoundHover();
        }
        
        if (TryGetComponent<Slider>(out Slider slider)){
            MouseController.Instance.SetHoveringSlider(this.gameObject);
            soundHandler.PlaySoundHover();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("[MOUSE INFO] Mouse exit: " + this.gameObject.name);
        if (TryGetComponent<Button>(out Button button)){
            MouseController.Instance.UnsetButtonHovering();
        }

        if (TryGetComponent<TMP_Dropdown>(out TMP_Dropdown dropdown)){
            MouseController.Instance.UnsetButtonHovering();

            if (isExit){
                isExit = false;
                dropdown.Hide();
            } else {
                isExit = true;
            }
        }
        
        if (TryGetComponent<ScrollRect>(out ScrollRect scrollRect) || TryGetComponent<CarSelectView>(out CarSelectView carSelectView) || TryGetComponent<TMP_Dropdown>(out TMP_Dropdown _dropdown)){
            MouseController.Instance.UnsetScrollViewHovering();
        }
        
        if (TryGetComponent<Slider>(out Slider slider)){
            MouseController.Instance.UnsetSliderHovering();
        }
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        Debug.Log("[MOUSE INFO] Mouse hovering over: " + this.gameObject.name);
        if (TryGetComponent<Button>(out Button button) || TryGetComponent<TMP_Dropdown>(out TMP_Dropdown dropdown)){
            MouseController.Instance.SetHoveringButton(this.gameObject);
        }
        
        if (TryGetComponent<ScrollRect>(out ScrollRect scrollRect) || TryGetComponent<CarSelectView>(out CarSelectView carSelectView) || TryGetComponent<TMP_Dropdown>(out TMP_Dropdown _dropdown)){
            MouseController.Instance.SetHoveringScrollView(this.gameObject);
        }
        
        if (TryGetComponent<Slider>(out Slider slider)){
            MouseController.Instance.SetHoveringSlider(this.gameObject);
        }
    }
}
