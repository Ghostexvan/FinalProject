using UnityEngine;
using UnityEngine.UI;

public class PaginationButton : MonoBehaviour
{
    private CarSelectButton carSelectButton;
    private int buttonIndex;

    public void OnClick(){
        if (!this.GetComponent<Toggle>().isOn){
            return;
        }
        
        carSelectButton.SetVariant(this.buttonIndex);
        this.GetComponent<Toggle>().interactable = false;
    }

    public void SetButtonInfo(CarSelectButton master, int buttonIndex){
        this.carSelectButton = master;
        this.buttonIndex = buttonIndex;
    }
}
