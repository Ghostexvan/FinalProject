using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    [SerializeField]
    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetBool("IsMoving", CarControl.LocalPlayerInstance.GetComponent<CarControl>().GetSpeed() > 0.1f);
        animator.SetFloat("ForwardSpeed", CarControl.LocalPlayerInstance.GetComponent<CarControl>().GetForwardSpeed());
        animator.SetFloat("SteeringAngle", CarControl.LocalPlayerInstance.GetComponent<CarControl>().hInput);
    }
}
