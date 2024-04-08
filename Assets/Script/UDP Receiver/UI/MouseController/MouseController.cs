using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

using JSAM;     // Is only used for the Back() method


public class MouseController : MonoBehaviour
{
    public static MouseController Instance;

    [SerializeField]
    private GameObject buttonHoveringOn = null;

    [SerializeField]
    private GameObject scrollViewHoveringOn = null;

    [SerializeField]
    private GameObject sliderHoveringOn = null;

    // [SerializeField]
    // private GameObject 

    [SerializeField]
    private float mouseDistanceSensity = 5.0f;
    
    [SerializeField]
    private float verticalSlideStep = 0.1f;

    [SerializeField]
    private float horizontalSlideStep = 0.1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[MOUSECONTROLLER] Exist an instance of this already, name: " + Instance.name);
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetHoveringButton(GameObject gameObject)
    {
        this.buttonHoveringOn = gameObject;
    }

    public void UnsetButtonHovering()
    {
        this.buttonHoveringOn = null;
    }

    public void SetHoveringScrollView(GameObject gameObject)
    {
        this.scrollViewHoveringOn = gameObject;
    }

    public void UnsetScrollViewHovering()
    {
        this.scrollViewHoveringOn = null;
    }

    public void SetHoveringSlider(GameObject gameObject)
    {
        this.sliderHoveringOn = gameObject;
    }

    public void UnsetSliderHovering()
    {
        this.sliderHoveringOn = null;
    }

    public void Click()
    {
        if (this.buttonHoveringOn == null){
            return;
        }

        try {
            if (this.buttonHoveringOn.TryGetComponent<Toggle>(out Toggle toggle)){
                if (toggle.interactable){
                    toggle.isOn = true;
                }
            } else if (this.buttonHoveringOn.TryGetComponent<Button>(out Button button)){
                button.onClick.Invoke();
            } else if (this.buttonHoveringOn.TryGetComponent<TMP_Dropdown>(out TMP_Dropdown dropdown)){
                if (dropdown.interactable){
                    dropdown.Show();
                }
            };
        } catch (Exception error) {
            Debug.LogWarning("[MOUSE WARNING] There's an error while trying to click the button: " + error);
        }
    }

    public void SetMousePosition(Vector2 targetPosition)
    {
        Debug.Log("[MOUSE INFO] Current mouse poisiton: " + Mouse.current.position.ReadValue());
        Debug.Log("[MOUSE INFO] Target position: " + targetPosition);

        if (Vector2.Distance(Mouse.current.position.ReadValue(), targetPosition) > mouseDistanceSensity)
            Mouse.current.WarpCursorPosition(
                targetPosition
            );
    }

    public void ScrollUp()
    {
        if (this.scrollViewHoveringOn == null)
        {
            return;
        }

        try
        {
            if (this.scrollViewHoveringOn.TryGetComponent<ViewScrollable>(out ViewScrollable viewScroll)){
                viewScroll.ScrollVertical(-1);
            } else if (this.scrollViewHoveringOn.TryGetComponent<TMP_Dropdown>(out TMP_Dropdown dropdown)){
                dropdown.value = (dropdown.value + 1) % dropdown.options.Count;
                dropdown.RefreshShownValue();
            }
        }
        catch (Exception error)
        {
            Debug.LogWarning("[MOUSE WARNING] There's an error while trying to scroll the panel: " + error);
        }
    }

    public void ScrollDown()
    {
        if (this.scrollViewHoveringOn == null)
        {
            return;
        }

        try
        {
            if (this.scrollViewHoveringOn.TryGetComponent<ViewScrollable>(out ViewScrollable viewScroll)){
                viewScroll.ScrollVertical(1);
            } else if (this.scrollViewHoveringOn.TryGetComponent<TMP_Dropdown>(out TMP_Dropdown dropdown)){
                dropdown.value = dropdown.value == 0 ?
                                 dropdown.options.Count - 1 :
                                 dropdown.value - 1;
                dropdown.RefreshShownValue();
            }
        }
        catch (Exception error)
        {
            Debug.LogWarning("[MOUSE WARNING] There's an error while trying to scroll the panel: " + error);
        }
    }

    public void ScrollLeft()
    {
        if (this.scrollViewHoveringOn == null)
        {
            return;
        }

        try
        {
            if (this.scrollViewHoveringOn.TryGetComponent<ViewScrollable>(out ViewScrollable viewScroll)){
                viewScroll.ScrollHorizontal(-1);
            } else {
                if (this.scrollViewHoveringOn.TryGetComponent<CarSelectView>(out CarSelectView carSelectView)){
                    carSelectView.SwipeNext();
                }
            }
        }
        catch (Exception error)
        {
            Debug.LogWarning("[MOUSE WARNING] There's an error while trying to scroll the panel: " + error);
        }
    }

    public void ScrollRight()
    {
        if (this.scrollViewHoveringOn == null)
        {
            return;
        }

        try
        {
            if (this.scrollViewHoveringOn.TryGetComponent<ViewScrollable>(out ViewScrollable viewScroll)){
                viewScroll.ScrollHorizontal(1);
            } else {
                if (this.scrollViewHoveringOn.TryGetComponent<CarSelectView>(out CarSelectView carSelectView)){
                    carSelectView.SwipeBack();
                }
            }
        }
        catch (Exception error)
        {
            Debug.LogWarning("[MOUSE WARNING] There's an error while trying to scroll the panel: " + error);
        }
    }

    public void SlideUp(){
        if (this.sliderHoveringOn == null)
        {
            return;
        }

        try
        {
            if (this.sliderHoveringOn.TryGetComponent<Slider>(out Slider slider)){
                slider.normalizedValue += verticalSlideStep;
            }
        }
        catch (Exception error)
        {
            Debug.LogWarning("[MOUSE WARNING] There's an error while trying to scroll the panel: " + error);
        }
    }

    public void SlideDown(){
        if (this.sliderHoveringOn == null)
        {
            return;
        }

        try
        {
            if (this.sliderHoveringOn.TryGetComponent<Slider>(out Slider slider)){
                slider.normalizedValue -= verticalSlideStep;
            }
        }
        catch (Exception error)
        {
            Debug.LogWarning("[MOUSE WARNING] There's an error while trying to scroll the panel: " + error);
        }
    }

    public void SlideLeft(){
        if (this.sliderHoveringOn == null)
        {
            return;
        }

        try
        {
            if (this.sliderHoveringOn.TryGetComponent<Slider>(out Slider slider)){
                slider.normalizedValue -= horizontalSlideStep;
            }
        }
        catch (Exception error)
        {
            Debug.LogWarning("[MOUSE WARNING] There's an error while trying to scroll the panel: " + error);
        }
    }

    public void SlideRight(){
        if (this.sliderHoveringOn == null)
        {
            return;
        }

        try
        {
            if (this.sliderHoveringOn.TryGetComponent<Slider>(out Slider slider)){
                slider.normalizedValue += horizontalSlideStep;
            }
        }
        catch (Exception error)
        {
            Debug.LogWarning("[MOUSE WARNING] There's an error while trying to scroll the panel: " + error);
        }
    }

    public void Back(){
        try {
            if (LauncherManager.Instance != null){
                LauncherManager.Instance.Back();

                AudioManager.StopSoundIfPlaying(MainGameSounds.Back);
                AudioManager.PlaySound(MainGameSounds.Back);
            }
        } catch (Exception error){
            Debug.LogError("[MOUSE WARNING] Cannot backing: " + error);
        }
    }
}