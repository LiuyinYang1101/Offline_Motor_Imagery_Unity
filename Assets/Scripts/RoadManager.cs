using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadManager : MonoBehaviour
{
    Road road;
    // Start is called before the first frame update
    void Start()
    {
        road = GetComponent<Road>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RoadTriggerEntered()
    {
        road.MoveRoads();
    }
}
