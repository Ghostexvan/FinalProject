using UnityEngine;

[CreateAssetMenu]
public class LocalPlayerData : ScriptableObject
{
    [SerializeField]
    private GameObject playerPrefab;

    [SerializeField]
    private int playerCarIndex;

    [SerializeField]
    private int playerVariantIndex;

    public string GetPlayerPrefabName() {
        return this.playerPrefab.name;
    }

    public GameObject GetPlayerPrefab() {
        return this.playerPrefab;
    }

    public int GetPlayerCarIndex(){
        return this.playerCarIndex;
    }

    public int GetPlayerVariantIndex(){
        return this.playerVariantIndex;
    }

    public void SetPlayerPrefab(GameObject carPrefab){
        this.playerPrefab = carPrefab;
    }

    public void SetPlayerIndex(int carIndex, int variantIndex){
        this.playerCarIndex = carIndex;
        this.playerVariantIndex = variantIndex;
    }
}
