using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Cars : ScriptableObject
{
    [SerializeField]
    private List<Car> cars = new List<Car>();

    public GameObject GetCarPrefab(int carIndex, int variantIndex) {
        return cars[carIndex].carVariants.GetCarPrefab(variantIndex);
    }

    public string GetCarName(int index) {
        return cars[index].carName;
    }

    public CarVariants GetCarVariants(int carIndex) {
        return cars[carIndex].carVariants;
    }

    public string GetCarDesciption(int carIndex){
        return cars[carIndex].carDescription;
    }

    public int GetTotalCarNumhber(){
        return this.cars.Count;
    }
}

[Serializable]
public struct Car {
    public string carName;
    public string carDescription;
    public CarVariants carVariants;
}