using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using JSAM;

public class CarSelectButton : MonoBehaviour
{
    [SerializeField]
    private int carIndex;

    [SerializeField]
    private int variantIndex;

    [SerializeField]
    private TMP_Text displayCarName;

    [SerializeField]
    private TMP_Text displayCarDescription;

    [SerializeField]
    private List<GameObject> paginationButtons = new List<GameObject>();

    [SerializeField]
    private GameObject paginationButton;

    [SerializeField]
    private Transform paginationSpawnTransfom;

    [SerializeField]
    private bool isOnEnable = false;

    [SerializeField]
    private GameObject nextButton;

    [SerializeField]
    private GameObject previousButton;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void NextCar(){
        this.carIndex = this.carIndex == LauncherManager.Instance.GetCarsPool().GetTotalCarNumhber() - 1 ?
                        0 :
                        this.carIndex + 1;
        this.variantIndex = this.carIndex == LauncherManager.Instance.GetLocalPlayerData().GetPlayerCarIndex() ?
                            LauncherManager.Instance.GetLocalPlayerData().GetPlayerVariantIndex() :
                            0;

        this.displayCarName.text = LauncherManager.Instance.GetCarsPool().GetCarName(carIndex);
        this.displayCarDescription.text = LauncherManager.Instance.GetCarsPool().GetCarDesciption(carIndex);

        /// Test: Stops Hoshilo sound if there's any currently playing
        AudioManager.StopSoundIfPlaying(MainGameSounds.Hoshilo_Selected_);

        LauncherManager.Instance.SpawnCarModel(LauncherManager.Instance.GetCarsPool().GetCarPrefab(this.carIndex, this.variantIndex));
        SpawnPagination();
        SetButtonStatus();
    }

    public void PreviousCar(){
        this.carIndex = this.carIndex == 0 ?
                        LauncherManager.Instance.GetCarsPool().GetTotalCarNumhber() - 1:
                        this.carIndex - 1;
        this.variantIndex = this.carIndex == LauncherManager.Instance.GetLocalPlayerData().GetPlayerCarIndex() ?
                            LauncherManager.Instance.GetLocalPlayerData().GetPlayerVariantIndex() :
                            0;

        this.displayCarName.text = LauncherManager.Instance.GetCarsPool().GetCarName(carIndex);
        this.displayCarDescription.text = LauncherManager.Instance.GetCarsPool().GetCarDesciption(carIndex);


        /// Test: Stops Hoshilo sound if there's any currently playing
        AudioManager.StopSoundIfPlaying(MainGameSounds.Hoshilo_Selected_);

        LauncherManager.Instance.SpawnCarModel(LauncherManager.Instance.GetCarsPool().GetCarPrefab(this.carIndex, this.variantIndex));
        SpawnPagination();
        SetButtonStatus();
    }

    public void SetVariant(int variantIndex){
        this.variantIndex = variantIndex;

        for (int index = 0; index < paginationButtons.Count; index++){
            if (index != this.variantIndex){
                this.paginationButtons[index].GetComponent<Toggle>().isOn = false;
                this.paginationButtons[index].GetComponent<Toggle>().interactable = true;
            }
        }

        if (!this.isOnEnable){
            this.isOnEnable = true;
            return;
        }

        //// Test: Car Select Button sound (MOVED TO PaginationButton.cs since this conflicted with NextCar and PrevCar's SFX)
        /// I added this since we can't really access the Color Variants in the Inspector
        /// rather these Toggles are spawned using CarSelectButton.cs --> SpawnPagination()
        //AudioManager.StopSoundIfPlaying(MainGameSounds.menu_accept);
        //AudioManager.PlaySound(MainGameSounds.Hoshilo_Selected_1_); //Test

        LauncherManager.Instance.SpawnCarModel(LauncherManager.Instance.GetCarsPool().GetCarPrefab(this.carIndex, this.variantIndex));
        SetButtonStatus();
    }

    public void SetPlayerIndex(){
        LauncherManager.Instance.GetLocalPlayerData().SetPlayerIndex(carIndex, variantIndex);
        LauncherManager.Instance.GetLocalPlayerData().SetPlayerPrefab(LauncherManager.Instance.GetCarsPool().GetCarPrefab(carIndex, variantIndex));

        PlayerPrefs.SetInt("Player Car Index", carIndex);
        PlayerPrefs.SetInt("Player Variant Index", variantIndex);

        //// Test: Plays sound when Hoshilo is selected
        if (carIndex == 2)
        {
            AudioManager.StopSoundIfPlaying(MainGameSounds.Hoshilo_Selected_);
            AudioManager.PlaySound(MainGameSounds.Hoshilo_Selected_);
        }
        else
        {
            AudioManager.StopSoundIfPlaying(MainGameSounds.Hoshilo_Selected_);
        }

        SetButtonStatus();
    }

    private void OnEnable() {
        LocalPlayerData localPlayerData = LauncherManager.Instance.GetLocalPlayerData();
        carIndex = localPlayerData.GetPlayerCarIndex();
        variantIndex = localPlayerData.GetPlayerVariantIndex();

        this.displayCarName.text = LauncherManager.Instance.GetCarsPool().GetCarName(carIndex);
        this.displayCarDescription.text = LauncherManager.Instance.GetCarsPool().GetCarDesciption(carIndex);
        SpawnPagination();
        SetButtonStatus();
    }

    private void OnDisable() {    
        if (this.carIndex != LauncherManager.Instance.GetLocalPlayerData().GetPlayerCarIndex() ||
            this.variantIndex != LauncherManager.Instance.GetLocalPlayerData().GetPlayerVariantIndex()
        ) {
            LauncherManager.Instance.SpawnCarModel(
                LauncherManager.Instance.GetCarsPool().GetCarPrefab(
                    LauncherManager.Instance.GetLocalPlayerData().GetPlayerCarIndex(),
                    LauncherManager.Instance.GetLocalPlayerData().GetPlayerVariantIndex()
                )
            );
        }

        this.isOnEnable = false;
    }

    private void SpawnPagination(){
        if (this.paginationButtons.Count >= 0){
            while (this.paginationButtons.Count > 0) {
                Destroy(this.paginationButtons[0]);
                this.paginationButtons.RemoveAt(0);
            }
        }

        for (int index = 0; index < LauncherManager.Instance.GetCarsPool().GetCarVariants(this.carIndex).GetTotalVariants(); index++){
            this.paginationButtons.Add(
                Instantiate(paginationButton, paginationSpawnTransfom)
            );

            Debug.Log("Spawn pagination at index " + index + "/" + LauncherManager.Instance.GetCarsPool().GetCarVariants(this.carIndex).GetTotalVariants());
            this.paginationButtons[index].GetComponent<PaginationButton>().SetButtonInfo(this, index);
            
            // Initialize all the Toggles with PaginationButton.isPlaySound as true
            this.paginationButtons[index].GetComponent<PaginationButton>().isPlaySound = true;
            this.paginationButtons[index].transform.GetChild(0).GetComponent<Image>().color = new Color32(
                LauncherManager.Instance.GetCarsPool().GetCarVariants(this.carIndex).GetVariantColor(index).r, 
                LauncherManager.Instance.GetCarsPool().GetCarVariants(this.carIndex).GetVariantColor(index).g,
                LauncherManager.Instance.GetCarsPool().GetCarVariants(this.carIndex).GetVariantColor(index).b,
                LauncherManager.Instance.GetCarsPool().GetCarVariants(this.carIndex).GetVariantColor(index).a
            );

            if (index == this.variantIndex){
                //// Changing PaginationButton's isPlaySound to False FIRST (before the line below)
                /// to prevent SFX from triggering, since the line below counts as a OnValueChanged callback.
                /// More details in the PaginationButton.cs script.
                this.paginationButtons[index].GetComponent<PaginationButton>().isPlaySound = false;
                /// The rest of the PaginationButton has isPlaySound = true as default though.
                /// Only this one (with isOn = true) had gotten its isPlaySound changed.

                //// This counts as a OnValueChanged callback
                this.paginationButtons[index].GetComponent<Toggle>().isOn = true;


            }
        }
    }

    private void SetButtonStatus(){
        if (this.carIndex == LauncherManager.Instance.GetLocalPlayerData().GetPlayerCarIndex() &&
            this.variantIndex == LauncherManager.Instance.GetLocalPlayerData().GetPlayerVariantIndex())
        {
            this.GetComponent<Button>().interactable = false;
            this.transform.GetChild(0).GetComponent<TMP_Text>().text = "Selected";
        } else {
            this.GetComponent<Button>().interactable = true;
            this.transform.GetChild(0).GetComponent<TMP_Text>().text = "Select";
        }
    }

    public int GetCurrentCarIndex(){
        return this.carIndex;
    }
}
