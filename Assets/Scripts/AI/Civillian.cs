using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Civillian : MonoBehaviour
{
    [Header("References")]
    [SerializeField, Tooltip("File containing list of all possible destinations. Program will try to find if not set")]
    private Destinations dests;
    
    [SerializeField, Tooltip("This civillian's destination")]
    private Vector3 dest;

    [SerializeField, Tooltip("rand num generator. Program will try to find if not set")]
    private randNum r;


    [Header("Variables")]
    [SerializeField, Tooltip("How suspicious the civillian is of the player")]
    private float suspicion = 0f;

    [SerializeField, Tooltip("Threshhold for when they become nosy")]
    private float suspicionThreshhold = 0f;
    
    [SerializeField, Tooltip("finite states for civillian")]
    private CivillianState state = CivillianState.normal;

    [SerializeField, Tooltip("speed in default state")]
    private float slowSpeed = 1.5f;

    [SerializeField, Tooltip("speed in nosy state")]
    private float fastSpeed = 3f;
    

    // Unserialized vars

    // Navmesh agent
    private NavMeshAgent agent;

    // has a random destination been generated
    private bool destGenerated = false;

    // prevents wait coroutine being called multiple times at once
    private bool notStartedWait = true;


    // initializes dests and rand num generator objects
    void Start()
    {
        if (!r)
            r = FindObjectsOfType<randNum>()[0];
        if (!dests)
            dests = FindObjectsOfType<Destinations>()[0];
        agent = GetComponent<NavMeshAgent>();
        dests.onDestinationsGenerated += generateDest;
    }

    // Unsubscribes on disable
    void OnDisable() {
        dests.onDestinationsGenerated -= generateDest;
    }

    // Generates the civillian destination on destinations finished generating
    void generateDest(object sender, EventArgs e)
    {
        dest = dests.destinationPoints[r.rand.Next(dests.destinationPoints.Count)];
        agent.autoBraking = true;
        agent.autoRepath = true;
        agent.SetDestination(dest);
        destGenerated = true;
    }

    // State swapping
    void Update() { 
        if (suspicion > suspicionThreshhold) {
            state = CivillianState.nosy;
            agent.speed = fastSpeed;
        }
        if (state == CivillianState.normal) {
            agent.speed = slowSpeed;
            if (destGenerated && !agent.pathPending && agent.remainingDistance < 0.5f)
                Destroy(this.gameObject);
        }
        else {
            if (!agent.pathPending && agent.remainingDistance < 2f && notStartedWait) {
                notStartedWait = false;
                StartCoroutine(wait());
            }
        }
        if (!destGenerated)
            generateDest(this, null);
    }

    // Attracts the attention of the civillian
    public void attractAttention(Transform pos, float addedSuspicion) {
        suspicion += addedSuspicion;
        if (suspicion >= 1f) {
            agent.isStopped = true;
            agent.ResetPath();
            agent.SetDestination(pos.position);
        }
    }

    // Waits for 2 seconds, then moves to original destination
    IEnumerator wait() {
        //agent.updateRotation = false;
        //transform.rotation = Quaternion.LookRotation((Vector3)(agent.destination - transform.position).normalized);
        agent.isStopped = true;
        agent.ResetPath();
        yield return new WaitForSeconds(2);
        //agent.updateRotation = true;
        suspicion = 0f;
        agent.SetDestination(dest);
        state = CivillianState.normal;
        notStartedWait = true;
    }
}
