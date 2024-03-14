using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class MouseController : MonoBehaviour
{
    public static MouseController Instance;

    [SerializeField]
    private GameObject objectCurrentlyHoveringOn = null;

    [SerializeField]
    private float mouseDistanceSensity = 5.0f;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Debug.LogWarning("[MOUSECONTROLLER] Exist an instance of this already, name: " + Instance.name);
            Destroy(this.gameObject);
        } else {
            Instance = this;
        }

        DontDestroyOnLoad(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetHoveringObject(GameObject gameObject){
        this.objectCurrentlyHoveringOn = gameObject;
    }

    public void UnsetHoveringObject(){
        this.objectCurrentlyHoveringOn = null;
    }

    public void Click() {
        if (this.objectCurrentlyHoveringOn == null){
            return;
        }

        try {
            this.objectCurrentlyHoveringOn.GetComponent<Button>().onClick.Invoke();
        } catch (Exception error) {
            Debug.LogWarning("[MOUSE WARNING] There's an error while trying to click the button: " + error);
        }
    }

    public void SetMousePosition(Vector2 targetPosition){
        Debug.Log("[MOUSE INFO] Current mouse poisiton: " + Mouse.current.position.ReadValue());
        Debug.Log("[MOUSE INFO] Target position: " + targetPosition);

        if (Vector2.Distance(Mouse.current.position.ReadValue(), targetPosition) > mouseDistanceSensity)
            Mouse.current.WarpCursorPosition(
                targetPosition
            );
    }
}
