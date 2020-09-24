using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{

    Animator animator;
    float speed = 0.0f;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        speed = 0f;
        if(Input.GetKey(KeyCode.UpArrow))
        {
            speed = 0.5f;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            speed = -0.5f;
        }
       

        animator.SetFloat("speed", speed);
    }
}
