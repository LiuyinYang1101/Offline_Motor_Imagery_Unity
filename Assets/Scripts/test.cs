using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class test : MonoBehaviour
{
    private float speed = 2f;

    private Rigidbody rb;
    private Animator anim;

    // ui text
    public TMP_Text leftText;
    public TMP_Text rightText;
    public TMP_Text timeText;


    public RoadManager roadManager;
    public AudioSource beeper;
    public AudioSource done;

    enum State
    {
        StartWait,
        Idle,
        SpaceWait,
        PrepareCount,
        PerformCount,
        HoldCount,
        FeedBack,
        Terminate,
    }
    private State curState = State.StartWait;

    private long lastTime;
    private long currentTime;
    private DateTime now;

    private Vector3 moveDir;
    private Vector3 moveAmout;
    private StreamWriter writer;
    private String cue;
    private System.Random rnd;
    private int jitter;
    // Start is called before the first frame update
    void Start()
    {

        now = DateTime.UtcNow;
        currentTime = new DateTimeOffset(now).ToUnixTimeMilliseconds();
        lastTime = currentTime;

        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();

        leftText.text = "";
        rightText.text = "";
        timeText.text = "";

        moveDir = new Vector3(0, 0, 1).normalized;
        rnd = new System.Random();
        writer = new StreamWriter("stimulusLog.txt", false);
        cue = "";

    }

    // Update is called once per frame
    void Update()
    {
        if (ExperimentController.experimentFinished) { 
            curState = State.Terminate;
            timeText.text = "Please wait..";
        }
        switch (curState)
        {
            case State.StartWait:
                timeText.text = "Please wait..";
                currentTime = getCurrentTimestamp();
                long offset = currentTime - lastTime;
                if(offset>=15000) // wait for at least 30 seconds
                {
                    curState = State.Idle;
                    lastTime = getCurrentTimestamp();
                }
                break;
            case State.Idle:
                /* output */
               // timeText.text = "Press Space to Start";
                // do nothing
                /* transition */
                if (ExperimentController.leftTrial)
                {
                    cue = "left ";
                    leftText.text = "Left Motor";
                    timeText.text = "Please wait";
                    lastTime = getCurrentTimestamp();
                    curState = State.SpaceWait;
                }
                else if (ExperimentController.rightTrial)
                {
                    cue = "right ";
                    rightText.text = "Right Motor";
                    timeText.text = "Please wait";
                    lastTime = getCurrentTimestamp();
                    curState = State.SpaceWait;
                }
                else if (ExperimentController.restTrial) {
                    cue = "rest ";
                    timeText.text = "Please wait";
                    lastTime = getCurrentTimestamp();
                    curState = State.SpaceWait;
                }

                break;

            case State.SpaceWait:
                /* output */
                currentTime = getCurrentTimestamp();
                offset = currentTime - lastTime;
                if (offset>=500)
                {
                    timeText.text = "Start in 2s";
                    lastTime = getCurrentTimestamp();
                    curState = State.PrepareCount;
                }
                break;
               
            case State.PrepareCount:
                /* output */
                currentTime = getCurrentTimestamp();
                offset = currentTime - lastTime;
                switch (offset)
                {
                    case >= 2000:
                        curState = State.PerformCount;
                        beeper.Play();
                        writer.WriteLine(cue + getCurrentTimestamp().ToString());
                        timeText.text = "Finish in 2s";
                        lastTime = getCurrentTimestamp();
                        break;
                    case >= 1000:
                        timeText.text = "Start in 1s";
                        break;
                    case >= 500:
                        //timeText.text = "Start in 2s";
                        break;
                }
                break;
            /* transition */

            case State.PerformCount:
                /* output */
                currentTime = getCurrentTimestamp();
                switch (currentTime - lastTime)
                {
                    case >= 2000:
                        done.Play();
                        jitter = rnd.Next(1000, 3000);
                        curState = State.FeedBack;
                        leftText.text = "";
                        rightText.text = "";
                        timeText.text = "";
                        lastTime = getCurrentTimestamp();
                        break;
                    case >= 1000:
                        timeText.text = "Finish in 1s";
                        break;
                    //case >= 1000:
                        //timeText.text = "Finish in 2s";
                       // break;
                }
                break;
            case State.HoldCount:
                currentTime = getCurrentTimestamp();
                switch (currentTime - lastTime)
                {
                    case >= 4000:
                        curState = State.FeedBack;
                        timeText.text = "Please rest";
                        leftText.text = "";
                        rightText.text = "";
                        lastTime = getCurrentTimestamp();
                        if (ExperimentController.leftTrial) anim.SetBool("walkLeft", true);
                        else if (ExperimentController.rightTrial) anim.SetBool("walkRight", true);
                        break;
                    case >= 3000:
                        timeText.text = "Wait for 1s";
                        break;
                    case >= 2000:
                        timeText.text = "Wait for 2s";
                        break;
                }
                break;
            case State.FeedBack:
                //anim.SetBool("walkLeft", false);
                //anim.SetBool("walkRight", false);
                //moveAmout = moveDir * speed * Time.deltaTime;
                //rb.MovePosition(rb.position + moveAmout);

                currentTime = getCurrentTimestamp();
                if (currentTime - lastTime >= jitter)
                {
                    timeText.text = "Wait to restart";
                    curState = State.Idle;
                    lastTime = currentTime;
                    ExperimentController.leftTrial = false;
                    ExperimentController.rightTrial = false;
                    ExperimentController.restTrial = false;
                    ExperimentController.readyToNextStimulus = true;
                }
                break;
            case State.Terminate:
                currentTime = getCurrentTimestamp();
                switch (currentTime - lastTime)
                {
                    case >= 15000:
                        Application.Quit();
                        break;
                    case >= 10000:
                        ExperimentController.experimentRun = false;
                        break;
                }
                break;
        }
      
 
    }

    void OnDestroy()
    {
        writer.WriteLine("End "+getCurrentTimestamp().ToString());
        writer.Close();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Wall")
        {
            print("touched a wall");
            roadManager.RoadTriggerEntered();
        }
            
    }

    private long getCurrentTimestamp()
    {
        now = DateTime.UtcNow;
        return new DateTimeOffset(now).ToUnixTimeMilliseconds();
    }
}
