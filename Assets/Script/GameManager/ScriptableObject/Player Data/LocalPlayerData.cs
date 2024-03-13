using UnityEngine;

[CreateAssetMenu]
public class LocalPlayerData : ScriptableObject
{
    [SerializeField]
    private Cars cars;
    [SerializeField]
    private int carIndex;
    [SerializeField]
    private int variantIndex;

    public void SetCarIndex(int index) {
        this.carIndex = index;
    }

    public void SetVariantIndex(int index) {
        this.variantIndex = index;
    }

    public string GetPlayerPrefabName() {
        return this.cars.name;
    }

    public GameObject GetPlayerPrefab() {
        return this.cars.GetCarPrefab(carIndex);
    }
}
