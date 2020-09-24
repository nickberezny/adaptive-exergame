using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class SaveData : MonoBehaviour
{

    private string h;
    private StreamWriter writer;
    private float deltaT = 0f;
    private static int score;
    private float position;
    private bool coin_passed = false;
    private bool coin_collected = false;
    private float coin_position;
    private bool started = false;

    private GameObject gameObject;

    // Start is called before the first frame update
    void Start()
    {
        string path = "game_data.txt";
        

        writer = new StreamWriter(path, false);
        
        writer.WriteLine("time(s),score,player,coin");

        gameObject = FindObjectOfType<ProceduralGeneration>().gameObject;

    }

    // Update is called once per frame
    void Update()
    {

        deltaT += Time.deltaTime;
        h = deltaT.ToString();

        score = PlayerController.score;
        h = h + "," + score.ToString();

        GameObject object1 = GameObject.FindGameObjectWithTag("Player");

        if(object1 != null)
        {
            Animator animator = object1.GetComponent<Animator>();

            //Debug.Log(animator.GetCurrentAnimatorStateInfo(0).normalizedTime);

            // h = h + "," + animator.GetCurrentAnimatorStateInfo(0).normalizedTime;

            h = h + "," + position.ToString();
        }
        else
        {
            h = h + "," + "null";
        }

        coin_passed = false;

        GameObject[] objects2;
        objects2 = GameObject.FindGameObjectsWithTag("Collectable");
        foreach (GameObject obj in objects2)
        {
            if (obj.transform.position.x < -4f)
            {
                obj.tag = "Obstacle"; //ensure coin pos is only recorded once 
                h = h + "," + obj.transform.position.y.ToString();
                coin_passed = true;
            }
        }

        if (coin_passed == false && coin_collected == false)
        {
            h = h + "," + "null";
        }
        else if(coin_passed == false && coin_collected == true)
        {

            h = h + "," + coin_position.ToString();
            coin_collected = false;
        }

        else
        {
            h = h + ",";
        }




        if (!gameObject.GetComponent<ProceduralGeneration>().pause)
        {
            if(!started)
            {
                writer.WriteLine(DateTime.Now.Hour.ToString()+","+ DateTime.Now.Minute.ToString() + "," + DateTime.Now.Second.ToString() + "," + DateTime.Now.Millisecond.ToString());
                started = true;
                
            }
            writer.WriteLine(h);
        }
        
        

    }

    public void setPosition(float pos)
    {
        position = pos;
        return;
    }

    public void coinCollected(float pos)
    {
        coin_collected = true;
        coin_position = pos;
        return;

    }





}
