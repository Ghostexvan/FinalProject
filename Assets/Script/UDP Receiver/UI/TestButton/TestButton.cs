using UnityEngine;
using UnityEngine.UI;

public class TestButton : MonoBehaviour
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
}
