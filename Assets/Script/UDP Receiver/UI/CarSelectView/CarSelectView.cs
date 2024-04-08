using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSelectView : MonoBehaviour
{
    [SerializeField]
    private CarSelectButton carSelectButton;

    private SoundHandler soundHandler;

    private void Awake()
    {
        // This will work since SFX Handler GameObject is active the whole time, meaning
        // its activeSelf and activeIH are True. Meaning you can find it  using GameObject.Find
        soundHandler = GameObject.Find("SFX Handler").GetComponent<SoundHandler>();

        // Awake won't run for some GameObjects IF they're inactive (activeIH = False), but if
        // they do manage to become active then we'll still be able to get SoundHandler nonetheless.
    }

    public void SwipeNext(){
        /// Test: Added purely for the UI UDP thing: UI_UDP_Recv --> HandController --> CarSelectView
        soundHandler.PlaySoundMove();
        carSelectButton.NextCar();
    }

    public void SwipeBack(){
        /// Test: Added purely for the UI UDP thing: UI_UDP_Recv --> HandController --> CarSelectView
        soundHandler.PlaySoundMove();
        carSelectButton.PreviousCar();
    }
}
