using UnityEngine;
using UnityEngine.EventSystems;

public class Hoverable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("[MOUSE INFO] Mouse hovering over: " + this.gameObject.name);
        MouseController.Instance.SetHoveringObject(this.gameObject);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("[MOUSE INFO] Mouse exit: " + this.gameObject.name);
        MouseController.Instance.UnsetHoveringObject();
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        Debug.Log("[MOUSE INFO] Mouse hovering over: " + this.gameObject.name);
        MouseController.Instance.SetHoveringObject(this.gameObject);
    }
}
