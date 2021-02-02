using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Drone : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Transform target = GameObject.Find("Player").transform;
        GetComponent<NavMeshAgent>().destination = target.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
