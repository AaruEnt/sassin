using NaughtyAttributes;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class Boat : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform targetPos;
    public Transform exitPos;
    public float waitTime = 30f;
    [SerializeField]
    private float waitTimer = 0f;

    public UnityEvent MasterOnBoatLeave;
    public UnityEvent InvaderOnBoatLeave;

    public AudioSource audio;
    public List<AudioClip> boatArriving;
    public List<AudioClip> boatDocked;
    public List<AudioClip> boatLeaving;

    private bool dockedClipPlayed = false;

    private bool leaving = false;
    private bool left = false;

    // Start is called before the first frame update
    void Start()
    {
        audio.clip = Randomizer.PickRandomObject(boatArriving);
        audio.Play();
        agent.SetDestination(targetPos.position);
    }

    // Update is called once per frame
    void Update()
    {
        if (!agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    // Done
                    if (leaving == true && !left)
                    {
                        if (PhotonNetwork.IsMasterClient)
                        {
                            left = true;
                            MasterOnBoatLeave.Invoke();
                        }
                        else
                        {
                            left = true;
                            InvaderOnBoatLeave.Invoke();
                        }
                    }
                    if (!dockedClipPlayed)
                    {
                        dockedClipPlayed=true;
                        audio.clip = Randomizer.PickRandomObject(boatDocked);
                        audio.Play();
                    }
                    waitTimer += Time.deltaTime;
                }
            }
        }
        if (waitTimer > waitTime && !leaving)
        {
            audio.clip = Randomizer.PickRandomObject(boatLeaving);
            audio.Play();
            agent.SetDestination(exitPos.position);
            leaving = true;
        }
    }
}
