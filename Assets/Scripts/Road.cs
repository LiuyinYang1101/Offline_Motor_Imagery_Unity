using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Road : MonoBehaviour
{
    public List<GameObject> roads;
    private float offset = 10.0f;
    // Start is called before the first frame update
    void Start()
    {
        if(roads != null && roads.Count > 0)
        {
            roads = roads.OrderBy(r => r.transform.position.z).ToList(); 
        }
    }

    public void MoveRoads()
    {
        GameObject moveRoad = roads[0];
        roads.Remove(moveRoad);
        float newZ = roads[roads.Count - 1].transform.position.z + offset;
        moveRoad.transform.localPosition = new Vector3(0,-4, newZ);
        
        roads.Add(moveRoad);
    }
}
