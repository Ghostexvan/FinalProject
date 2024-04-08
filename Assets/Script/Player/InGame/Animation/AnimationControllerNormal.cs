using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationControllerNormal : MonoBehaviour
{
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private CarControlNormal ccn;

    private void Awake()
    {
        ccn = GameObject.FindGameObjectWithTag("Player").GetComponent<CarControlNormal>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetBool("IsMoving", ccn.GetSpeed() > 0.1f);
        animator.SetFloat("ForwardSpeed", ccn.GetForwardSpeed());
        animator.SetFloat("SteeringAngle", ccn.hInput);
    }
}
