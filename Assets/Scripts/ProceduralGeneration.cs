using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;


public class ProceduralGeneration : MonoBehaviour
{

    public float leftThreshold = -28f; //threshold where obstacles are destroyed (offscreen behind player)
    public float spawnPosition = 13f; //start point of obstacle (offscreen in front of player) 
    public GameObject obstacleTop, obstacleBottom, coin;
    public Text pauseText;
    public AudioSource tone;

    private GameObject backObstacle;

    private float ceiling = 3f; //top of game area 
    private float depth = -12f; //depth of obstacles (z)
    private float dt1 = 0.5f; //seconds
    private float dt2 = 1.0f; //sec
    private float dt3 = 1.0f; //sec

    private float g = 2f; //gap between top and bottom obstacle
    private float h = 3f; //height from ceiling to midpoint of obstacle gap
    private float speed = 4f;
    private float deltaT = 0f;
    private float totalT = 0f; 
    private float spawnDistance = 10f;

    private float toneInterval = 30; //interval between tones to cue fatigue question (sec)
    private float toneTextOnScreenTime = 5;

    private bool order = true; //track obstacles for up and down 
    private bool doneCheck = true;

    public bool pause = true;

    public Button startButton;
    public Text prompt;

    // Start is called before the first frame update
    void Start()
    {
        
        generateNewObstacle(order);
        order = !order;
    }

    // Update is called once per frame
    void Update()
    {
        //check if any obstacles need to be deleted
        //check if back obstacle is dX away from spawn location

        if (pause)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }

        deltaT = Time.deltaTime;
        totalT = totalT + deltaT;

        if(totalT >= toneInterval - (toneTextOnScreenTime/2f) && totalT <= toneInterval + (toneTextOnScreenTime/2f))
        { 
           // tone.Play();
            prompt.enabled = true;

        }
        if (totalT > toneInterval + (toneTextOnScreenTime / 2f))
        {
            prompt.enabled = false;
            totalT = 0f;
        }
        checkObstaclePositions("Obstacle");
        checkObstaclePositions("Collectable");

        if (backObstacle.transform.position.x <= spawnPosition)
        {
            generateNewObstacle(order);
            order = !order;
        }
       
    }

    public void startGame()
    {
   
        startButton.interactable = false;
        pause = false;

    }

    

    void checkObstaclePositions(string tag)
    {
        
        GameObject[] objects;
        objects = GameObject.FindGameObjectsWithTag(tag);

        foreach (GameObject obj in objects)
        {
            obj.transform.Translate(new Vector3(-speed * deltaT, 0, 0));
           
            if (obj.transform.position.x < leftThreshold)
            {
                Debug.Log("Destroy");
                Debug.Log(obj.name);
                Destroy(obj, 0);
            }

        }
        return;
    }

    void generateNewObstacle(bool obstacleType)
    {
        Debug.Log("gen");

        //build obstacles to bring player down
        //TODO: update h, g
        //TODO: also adapt d, w, 
        float dx1 = dt1 * speed;
        float dx2 = dt2 * speed;
        float dx3 = dt3 * speed;

        Instantiate(obstacleTop, new Vector3(spawnPosition + 0.5f * dx3 + 0.5f * dx2 + dx1, 0.2f, -15f), Quaternion.identity);
        //Instantiate(obstacleBottom, new Vector3(spawnPosition + 0.5f * dx3 + 0.5f * dx2 + dx1, ceiling - h - (g / 2f), depth), Quaternion.identity);
        Instantiate(coin, new Vector3(spawnPosition + 0.5f * dx3 + 0.5f * dx2 + dx1, (ceiling - h), depth), Quaternion.identity);

        float y = ceiling - g / 2f;
        float ynext = (ceiling - h);
        float slope = (ynext - y) / (dx1);

        for (float i = 0f; i < 3f; i++)
        {
            float xc = spawnPosition + 0.5f * dx3 + (i / 2f) * dx1;
            Instantiate(coin, new Vector3(xc, slope * (xc - spawnPosition - 0.5f * dx3) + y, depth), Quaternion.identity);
        }

        //backObstacle = Instantiate(obstacleTop, new Vector3(spawnPosition + 2.0f * dx1 + dx2 + dx3, ceiling, depth), Quaternion.identity);
        //Instantiate(obstacleBottom, new Vector3(spawnPosition + 2.0f * dx1 + dx2 + dx3, ceiling - g, depth), Quaternion.identity);
        backObstacle =  Instantiate(coin, new Vector3(spawnPosition + 2.0f * dx1 + dx2 + dx3, (ceiling - g/2.0F), depth), Quaternion.identity);

        
        y = (ceiling - h);
        ynext = ceiling - g / 2f;
        slope = (ynext - y) / (dx1);

        for (float i = 0f; i < 3f; i++)
        {
            float xc = spawnPosition + 0.5f * dx3 + (i / 2f) * dx1 + dx2 + dx1;
            Instantiate(coin, new Vector3(xc, slope * (xc - spawnPosition - 0.5f * dx3 - dx2 - dx1) + y, depth), Quaternion.identity);
        }
        Debug.Log("Done gen");

        /*
        else if (obstacleType == false)
        {
            //build obstacles to bring player up 
            backObstacle = Instantiate(obstacleTop, new Vector3(spawnPosition, ceiling, depth), Quaternion.identity);
            Instantiate(obstacleBottom, new Vector3(spawnPosition, ceiling - g, depth), Quaternion.identity);

            //create coins along line 
            float y = ceiling - g / 2f;
            h = Random.Range(4f, 7f);
            float slope = (h - y) / (spawnDistance);

            for(float i = 0f; i < 4f; i++)
            {
                float xc = spawnPosition + (i / 4f) * spawnDistance;
                Instantiate(coin, new Vector3(xc, -slope*(xc-spawnPosition) + y, depth), Quaternion.identity);
            }
            Debug.Log("Done gen");

        }
        */


        //order = !order;
        return;

    }

    public void stopGame()
    {
        FindObjectOfType<TCPSocket>().sendMsg("G_STOP");
        Application.Quit();
    }

    public void sendMsg(string msg)
    {
        FindObjectOfType<TCPSocket>().sendMsg(msg);
        return;
    }

    public void onFrequencyChange(string freq)
    {
        //change break distance according to input text
        int val = int.Parse(freq);
        if (val > 0 && val < 10) dt3 = val;
    }

    public void onSlopeChange(string slope)
    {
        //change break distance according to input text
        float val = float.Parse(slope);
        if (val > 0.4 && val < 5) dt1 = val;
    }
}
