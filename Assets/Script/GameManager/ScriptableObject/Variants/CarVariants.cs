using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CarVariants : ScriptableObject
{
    [SerializeField]
    private List<Variant> variants = new List<Variant>();

    public Material GetVariantMaterial(int index){
        return variants[index].variantMaterial;
    }

    public string GetVariantName(int index){
        return variants[index].variantName;
    }
}

[Serializable]
public struct Variant {
    public string variantName;
    public Material variantMaterial;
}