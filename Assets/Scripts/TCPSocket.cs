using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using System.Net;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class TCPSocket : MonoBehaviour
{

    private bool readEMG = true; 

    private TcpListener tcpListener;
    private Thread tcpListenerThread;
    private Thread recieveThread1;
    private Thread recieveThread2;

    NetworkStream streamE;
    NetworkStream streamT;

    private int streamsSet = 0;

    private string[] stringArray;
    private float position = 2.5f;
    private float[] range = new float[2];
    int rangeSet = 0;
    int newPosition = 0;
    private int length = 5;
    private float[] x;
    private int state = -1;
    private bool pause = true;
    private bool runThread = true;


    public Button runButton;

    //float speed = 0.0f;
    float pos = 0.0f;

    Animator animator;
    private GameObject player;

    void Start()
    {
        // Start TcpServer background thread 		
        tcpListenerThread = new Thread(new ThreadStart(ListenForIncommingRequests));
        tcpListenerThread.IsBackground = true;
        tcpListenerThread.Start();

        x = new float[length];
        runButton.interactable = false;
    }

    private void Update()
    {
        float delta = Time.deltaTime;

        if (Time.timeScale == 0) { pause = true; }
        else { pause = false; }

        if (rangeSet == 1)
        {
            runButton.interactable = true;
        }


        //Debug.Log("rangeset " + rangeSet + "newposition " + newPosition);

        if (!pause  && newPosition == 1)
        {
            
            
            if(float.TryParse(stringArray[0], out float temp))
            {
                position = (temp - range[0]) / (range[1] - range[0]);
                //Debug.Log("pos+temp: " + position.ToString() + "," + temp.ToString());
            }

            //if(temp > range[0]  && temp < range[1])
            

            if (position > 1) position = 1;
            if (position < 0) position = 0; 
            //position = -10f * (temp - range[0]) / (range[1] - range[0]) + 5f;
            //Debug.Log("position" + position);
            newPosition = 0;

        }

        
        if(player == null && !pause)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
        if (player != null)
        {
            animator = player.GetComponent<Animator>();
            animator.Play("New State", -1, position * 0.5f);
            GameObject.FindGameObjectWithTag("Logger").GetComponent<SaveData>().setPosition(position * 0.5f);
           // Debug.Log("Animate to " + position.ToString());
        }

    }

    public void pauseGame()
    {
        pause = !pause;
        //return;
    }

    private void OnDestroy()
    {
        runThread = false;
    }

    public void sendMsg(string msg)
    {
        
        byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(msg);
        streamT.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
        if(readEMG) streamE.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);

        if(msg == "G_SET2")
        {
            state = 2;
        }
    }




    private void ListenForIncommingRequests()
    {
        try
        {
            // Create listener on localhost port 8052. 			
            tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 8081);
            tcpListener.Start();
            //Debug.Log("Server is listening");
            int i = 0;
            
            while (runThread)
            {
                TcpClient connectedTcpClient;
                connectedTcpClient = tcpListener.AcceptTcpClient();
                ThreadPool.QueueUserWorkItem(RecieveIncomingMessages, connectedTcpClient);

            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("SocketException " + socketException.ToString());
        }
    }

    private void RecieveIncomingMessages(object Data)
    {
        Debug.Log("accepted");
        Byte[] bytes = new Byte[1024];
        TcpClient connectedTcpClient = (TcpClient) Data;
        NetworkStream stream = connectedTcpClient.GetStream();



        int length;
        int client = 0; // 1 = EMG; 2 = Tracker
            
        // Read incomming stream into byte arrary. 						
        while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
        {
            var incommingData = new byte[length];
            Array.Copy(bytes, 0, incommingData, 0, length);
            // Convert byte array to string message. 							
            string clientMessage = Encoding.ASCII.GetString(incommingData);
            //stringArray = clientMessage.Split(',');
           Debug.Log("client message received as: " + clientMessage);

            if(state == -1 || state == 0)
            {
                if (clientMessage == "E_AWAKE")
                {
                    state++;
                    client = 1;
                    Debug.Log("E_AWAKE!");
                    streamE = stream;

                }
                else if(clientMessage == "T_AWAKE")
                {
                    state++;
                    client = 2;
                    Debug.Log("T_AWAKE!");
                    streamT = stream;
                }
            }

            if(client == 1)
            {
                
            }else if(client == 2)
            {
                
                //handle tracker
                if(rangeSet == 1) newPosition = 1;
                //Debug.Log("Reset new position");
                stringArray = clientMessage.Split(',');
               
                

                if (rangeSet == 1 && float.TryParse(stringArray[0], out float j))
                {
                   // rangeSet = 2;
                }

                if (rangeSet == 0 && float.TryParse(stringArray[0], out float i))
                {
                    range[0] = Math.Min(float.Parse(stringArray[0]), float.Parse(stringArray[1]));
                    range[1] = Math.Max(float.Parse(stringArray[0]), float.Parse(stringArray[1]));
                    rangeSet = 1;
                }
            }

                

        }
        
    }

}