using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Security.Cryptography;

public class PlayerController : MonoBehaviour
{
    public GameObject coinDrop;
    public Text scoreText;

    private float speed = 5f;
    public AudioSource coinSource;
    public AudioSource collideSource;

    public static int score = 0;



    // Start is called before the first frame update
    void Start()
    {

        //source = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {

        scoreText.text = "Score: " + score.ToString();
        float delta = Time.deltaTime; 
        //check collisions 

        //TODO: get position from UDP
        /*
        if (Input.GetKey("w") || Input.GetKey(KeyCode.UpArrow))
        {
            transform.Translate(new Vector3(0, speed* Time.deltaTime, 0));
        }
        else if (Input.GetKey("s") || Input.GetKey(KeyCode.DownArrow)) 
        {
            transform.Translate(new Vector3(0, -speed* Time.deltaTime, 0));
        }
        else if (Input.GetKey("x"))
        {
            Application.Quit();
        }
        */



        /*
        if (rangeSet == 2 && newPosition == 1)
        {
            
            float temp = float.Parse(stringArray[1]);
            float prev = position;
            x = -10f * (temp - range[0]) / (range[1] - range[0]) + 5f;
            velocity = (position - prev) / delta;

            
        }
        else if(rangeSet == 2 && newPosition == 0)
        {
            //position = position + velocity * delta;

            Debug.Log(delta);
        }

        position = position + (k / m) * (x - position) * Mathf.Pow(delta,2f);

        newPosition = 0;
        transform.SetPositionAndRotation(new Vector3(0, position, -10), Quaternion.identity);
          */


    }

    void OnCollisionEnter2D(Collision2D col)
    {
        switch(col.gameObject.tag)
        {
            case "Collectable":
                score += 1;
                coinSource.Play();
                GameObject.FindGameObjectWithTag("Logger").GetComponent<SaveData>().coinCollected(col.gameObject.transform.position.y);
                Destroy(col.gameObject);

                //add to score, animation?
                break;
            case "Obstacle":
                int i = 0;
                collideSource.Play();
                StartCoroutine(Blink(1.0f));
                while (score > 0 && i < 6 )
                {
                    //Instantiate(coinDrop, transform.position, Quaternion.identity);
                    i += 1;
                    score -= 1;
                    
                }
                
                //todo drop right number of coins, decrease score
                break;
            default:
                break;

        }
        

       // Debug.Log(score);
    }



    private IEnumerator Blink(float waitTime)
    {
        var endTime = Time.time + waitTime;
       // Debug.Log("End Time: " + endTime);
       // Debug.Log("Time: " + Time.time);
        while (Time.time < endTime)
        {
           
            GameObject go1 = GameObject.Find("Beta_Joints");
            GameObject go2 = GameObject.Find("Beta_Surface");
            Renderer r1 = go1.GetComponent<Renderer>();
            Renderer r2 = go2.GetComponent<Renderer>();

            r1.material.shader = Shader.Find("Self-Illumin/Outlined Diffuse");
            r2.material.shader = Shader.Find("Self-Illumin/Outlined Diffuse");
            //r1.enabled = false;
           // r2.enabled = false;
            yield return new WaitForSeconds(0.15f);
            //r1.enabled = true;
            //r2.enabled = true;
            r1.material.shader = Shader.Find("Diffuse");
            r2.material.shader = Shader.Find("Diffuse");
            yield return new WaitForSeconds(0.15f);
            
        }
    }




}
