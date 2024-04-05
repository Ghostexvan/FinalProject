using UnityEngine;
using UnityEngine.UI;

using JSAM;

public class PaginationButton : MonoBehaviour
{
    private CarSelectButton carSelectButton;
    private int buttonIndex;

    // This bool is to prevent PlaySound from coming off when PaginationButton is initialized and THEN had its isOn property to True.
    // (Code is in CarSelectButton.cs --> SpawnPagination --> (if index == this.variantIndex))
    // Initializing PaginationButton and letting isOn = True actually triggers the Toggle's OnValueChanged callback. Which in turn actually
    // calls PaginationButton.OnClick --> CarSelectButton.SetVariant
    
    // Initailly, all Toggles with PaginationButton would have isPlaySound = True.
    // Only the one with isOn = true will have its isPlaySound be changed to False.
    public bool isPlaySound = true;

    public void OnClick(){
        if (!this.GetComponent<Toggle>().isOn){
            //isPlaySound = true;
            return;
        }

        // isPlaySound will be False when initialized, preventing PlaySound from being called.
        // On the second time a PaginationButton Toggle's bool value is changed, this 
        if (isPlaySound)
        {
            PlaySoundClick();
        }


        carSelectButton.SetVariant(this.buttonIndex);
        this.GetComponent<Toggle>().interactable = false;
        
        // After having been initialized with isOn = True (CarSelectButton.SpawnPagination last line), we'll change isPlaySound to True
        // to allow the SFX to be played when the Toggle is clicked on the second time.
        this.isPlaySound = true;
    }

    public void SetButtonInfo(CarSelectButton master, int buttonIndex){
        this.carSelectButton = master;
        this.buttonIndex = buttonIndex;
    }

    public void PlaySoundClick()
    {

        //// Test: Car Select Button sound (MOVED TO PaginationButton.cs)
        /// I added this since we can't really access the Color Variants in the Inspector
        /// rather these Toggles are spawned using CarSelectButton.cs --> SpawnPagination()
        /// And since these are prefabs, again (just like RoomInfo prefabs with RoomButton.OnClick), we play
        /// the sound when OnClick is called. We can't access Scene GameObjects here so we can't really call SoundHandler.cs
        /// AudioManager (kinda) works since it's a Singleton...? Also because of the JSAM import at the start I think.
        AudioManager.StopSoundIfPlaying(MainGameSounds.menu_accept);
        AudioManager.PlaySound(MainGameSounds.menu_accept); //Test

        //Debug.LogWarning("Toggle is Toggled on!");
    }
}

/* Basically the flow will be like this:
- When CarSelectButton is ENABLED - or - CarSelectButton.NextCar()/PreviousCar() is called: CSB.SpawnPagination() is also called.
- Toggles with PaginationButton are initialized with all 4 of them having:
        isOn = False, PaginationButton.isPlaySound = True.
- On the last line of CSB.SpawnPagination(), if a Toggle has (index == variantIndex) then it will have: isOn = True, isPlaySound = False.
- When Toggle isOn = True, its OnValueChanged callback is triggered, hence PaginationButton.OnClick is called.
- PaginationButton.OnClick will call CSB.SetVariant; with isPlaySound = False, it won't call PlaySound.
- Then after having ran everything, isPlaySound will be True, allowing the Toggle to call PlaySound when it's clicked on the 2nd time.

- The other Toggles with isOn = False will have their isPlaySound = true by default, so that they can play their SFX when they're pressed.
- Note that isOn = False won't let PaginationButton.OnClick run.
 */