using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleController : MonoBehaviour
{

    private float speed = 3f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        float deltaT = Time.deltaTime;
        speed = speed + 0.01f;
        //Debug.Log(speed);
        //transform.Translate(new Vector3(-speed* deltaT, 0 , 0));

    }


}
