using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Hoverable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    //[SerializeField]
    private SoundHandler soundHandler;

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
        MouseController.Instance.SetHoveringObject(this.gameObject);

        // This is pretty scuffed but it works
        // Basically this checks whether our cursor has entered (hovered) over a Button or not,
        // if it has then PlaySoundHover will be called
        if (this.gameObject.GetComponent<Button>() != null)
        {
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
        Debug.Log("[MOUSE INFO] Mouse hovering (moving) over: " + this.gameObject.name);
        MouseController.Instance.SetHoveringObject(this.gameObject);
    }
}
