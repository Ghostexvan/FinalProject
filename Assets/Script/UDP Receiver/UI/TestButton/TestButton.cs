using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TestButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private void Awake() {
        this.gameObject.GetComponent<Button>().onClick.AddListener(this.OnClick);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClick(){
        Debug.Log("[TEST BUTTON INFO] You clicked me!");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // if (eventData.pointerCurrentRaycast.gameObject != null)
        // {
        //     // Debug.Log("[MOUSE INFO] Mouse hovering over: " + eventData.pointerCurrentRaycast.gameObject.name);

        //     MouseController.Instance.SetHoveringObject(this.gameObject);
        // }

        Debug.Log("[MOUSE INFO] Mouse hovering over: " + this.gameObject.name);
        MouseController.Instance.SetHoveringObject(this.gameObject);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("[MOUSE INFO] Mouse exit: " + this.gameObject.name);
        MouseController.Instance.UnsetHoveringObject();
    }
}
