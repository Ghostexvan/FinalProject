using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Cars : ScriptableObject
{
    [SerializeField]
    private List<Car> cars = new List<Car>();

    public GameObject GetCarPrefab(int index) {
        return cars[index].carPrefab;
    }

    public string GetCarName(int index) {
        return cars[index].carName;
    }

    public CarVariants GetCarVariants(int carIndex) {
        return cars[carIndex].carVariants;
    }
}

[Serializable]
public struct Car {
    public string carName;
    public GameObject carPrefab;
    public CarVariants carVariants;
}