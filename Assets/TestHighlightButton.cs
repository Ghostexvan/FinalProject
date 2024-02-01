using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TestHighlightButton : MonoBehaviour
{
    [SerializeField]
    private GameObject[] buttonList;

    [SerializeField]
    private int selectIndex = 0;

    private void Awake() {
        buttonList = new GameObject[this.transform.childCount];
        for (int index = 0; index < buttonList.Length; index++){
            buttonList[index] = this.transform.GetChild(index).gameObject;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("w")) {
            MoveUp();
        } else if (Input.GetKeyDown("s")){
            MoveDown();
        }
        
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(buttonList[selectIndex]);
    }

    public void MoveUp(){
        this.selectIndex = this.selectIndex - 1 < 0 ? this.buttonList.Length - 1 : this.selectIndex - 1;
    }

    public void MoveDown(){
        this.selectIndex = this.selectIndex + 1 >= this.buttonList.Length ? 0 : this.selectIndex + 1;
    }

    public void SetButton(GameObject sender){
        this.selectIndex = sender.transform.GetSiblingIndex();
    }

    private void OnDisable() {
        this.selectIndex = 0;
        EventSystem.current.SetSelectedGameObject(null);
    }
}
