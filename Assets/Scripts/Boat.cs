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

    private bool leaving = false;
    private bool left = false;

    // Start is called before the first frame update
    void Start()
    {
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
                    waitTimer += Time.deltaTime;
                }
            }
        }
        if (waitTimer > waitTime)
        {
            agent.SetDestination(exitPos.position);
            leaving = true;
        }
    }
}
