// Chua comment
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointController : MonoBehaviour
{
    #region Private Fields
    [SerializeField]
    private int num;

    #endregion

    #region MonoBehaviour Callbacks

    private void Awake() {
        this.num = Int32.Parse(this.name);
    }
    
    #endregion
}
