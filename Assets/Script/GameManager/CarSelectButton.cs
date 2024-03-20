using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

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

        LauncherManager.Instance.SpawnCarModel(LauncherManager.Instance.GetCarsPool().GetCarPrefab(this.carIndex, this.variantIndex));
        SetButtonStatus();
    }

    public void SetPlayerIndex(){
        LauncherManager.Instance.GetLocalPlayerData().SetPlayerIndex(carIndex, variantIndex);
        LauncherManager.Instance.GetLocalPlayerData().SetPlayerPrefab(LauncherManager.Instance.GetCarsPool().GetCarPrefab(carIndex, variantIndex));

        PlayerPrefs.SetInt("Player Car Index", carIndex);
        PlayerPrefs.SetInt("Player Variant Index", variantIndex);
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
            this.paginationButtons[index].transform.GetChild(0).GetComponent<Image>().color = new Color32(
                LauncherManager.Instance.GetCarsPool().GetCarVariants(this.carIndex).GetVariantColor(index).r, 
                LauncherManager.Instance.GetCarsPool().GetCarVariants(this.carIndex).GetVariantColor(index).g,
                LauncherManager.Instance.GetCarsPool().GetCarVariants(this.carIndex).GetVariantColor(index).b,
                LauncherManager.Instance.GetCarsPool().GetCarVariants(this.carIndex).GetVariantColor(index).a
            );

            if (index == this.variantIndex){
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
