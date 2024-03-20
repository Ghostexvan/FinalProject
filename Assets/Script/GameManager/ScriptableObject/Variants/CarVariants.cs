using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CarVariants : ScriptableObject
{
    [SerializeField]
    private List<Variant> variants_ = new List<Variant>();

    public GameObject GetCarPrefab(int index){
        return this.variants_[index].variantPrefab;
    }

    public int GetTotalVariants(){
        return this.variants_.Count;
    }

    public Color32 GetVariantColor(int index){
        return this.variants_[index].variantColor;
    }
}

[Serializable]
public struct Variant{
    public GameObject variantPrefab;
    public Color32 variantColor;
}