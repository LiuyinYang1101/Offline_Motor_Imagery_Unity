using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperimentController : MonoBehaviour
{
    
    private int noLeft = 10;
    private int noRight = 10;
    private int noRest = 5;
    private int repetition = 3;
    private bool ifRepeat = true;
    private int lastTrial = -1;
    private List<int> leftRightTrials = new List<int>();
    public static bool experimentRun = true;
    public static bool readyToNextStimulus = true;
    public static bool leftTrial = false;
    public static bool rightTrial = false;
    public static bool restTrial = false;
    public static int duration = 0;
    public static bool experimentFinished = false;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i<noLeft; i++)
        {
            leftRightTrials.Add(0);
        }

        for (int i = 0; i < noRight; i++)
        {
            leftRightTrials.Add(1);
        }

        for (int i = 0; i < noRest; i++)
        {
            leftRightTrials.Add(2);
        }

        leftRightTrials = leftRightTrials.OrderBy(i => Guid.NewGuid()).ToList();
    }

    // Update is called once per frame
    void Update()
    {
        if ((leftRightTrials.Count() == 0) && readyToNextStimulus && repetition == 0) // terminating condition
        {
            experimentFinished = true;
            return;
        }

        else if (readyToNextStimulus) // next stimulus starts
        {
            readyToNextStimulus = false;
            if (ifRepeat)
            {
                if (repetition > 0)
                {
                    // REPEAT LAST TRIAL
                    switch (lastTrial)
                    {
                        case -1:
                            int thisTrial = leftRightTrials.Last();
                            lastTrial = thisTrial;
                            leftRightTrials.RemoveAt(leftRightTrials.Count() - 1);
                            if (thisTrial == 0) { leftTrial = true; }
                            else if(thisTrial == 1) { rightTrial = true; }
                            else { restTrial = true; }
                            break;
                        case 0:
                            leftTrial = true;
                            break;
                        case 1:
                            rightTrial = true;
                            break;
                        case 2:
                            restTrial = true;
                            break;

                    }
                    repetition--;
                }
                else
                {
                    repetition = 2;
                    int thisTrial = leftRightTrials.Last();
                    lastTrial = thisTrial;
                    leftRightTrials.RemoveAt(leftRightTrials.Count() - 1);
                    if (thisTrial == 0) { leftTrial = true; }
                    else if (thisTrial == 1) { rightTrial = true; }
                    else { restTrial = true; }
                }

            }
            else
            {
                int thisTrial = leftRightTrials.Last();
                leftRightTrials.RemoveAt(leftRightTrials.Count() - 1);
                if (thisTrial == 0) { leftTrial = true; }
                else { rightTrial = true; }
            }
           
        }
        else
        {
            //do nothing
        }
    }
}
