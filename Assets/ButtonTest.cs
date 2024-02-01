using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonTest : MonoBehaviour
{
    public Color wantedColor;
    private Button button;
    private Color originalColor;
    private ColorBlock colorBlock;

    private void Awake() {
        this.button = this.GetComponent<Button>();
    }

    private void Start() {
        colorBlock = button.colors;
        originalColor = colorBlock.selectedColor;
    }

    public void ChangeColorOnHover(){
        colorBlock.selectedColor = wantedColor;
        button.colors = colorBlock;
    }

    public void ChangeColorOnLeaves(){
        colorBlock.selectedColor = originalColor;
        button.colors = colorBlock;
    }
}
