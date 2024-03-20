using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSelectView : MonoBehaviour
{
    [SerializeField]
    private CarSelectButton carSelectButton;

    public void SwipeNext(){
        carSelectButton.NextCar();
    }

    public void SwipeBack(){
        carSelectButton.PreviousCar();
    }
}
