using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;


// Allows an object with a navmeshagent to patrol to preset waypoints on most efficient path
public class Enemy : MonoBehaviour
{
    // All waypoints
    public Transform[] wayPoints;
    // Waypoints auto generated while searching
    private Vector3[] searchWaypoints;
    // The navmeshagent component
    private NavMeshAgent agent;
    // index of current waypoint
    public int currWaypoint = 0;
    // index of search waypoint
    private int searchWaypoint = 0;
    // Is the agent currently waiting
    private bool isWaiting = false;
    // is the agent currently chasing
    private bool isChasing = false;
    // Agent's suspicion of player
    public float suspicion = 0f;
    public LayerMask mask;
    // minimum suspicion
    private float minSuspicion = 0f;
    // maximum suspicion
    private float maxSuspicion = 8f;
    // which state the enemy is in
    public EnemyState state = EnemyState.patrol;
    // Last position player was seen
    public Transform lastDetectedArea;
    // Player - used for smoother chasing
    private GameObject player;
    // spin timer
    private float t = 0f;
    // used for cancelling coroutines
    private Coroutine c;
    // is a coroutine running
    private bool cr_running = false;
    // should the agent look around
    private bool dontLook = false;
    private bool dontIncrement = false;
    private bool reachedThreshhold = false;
    private float startMoveSpeed;
    public float minMoveSpeed;
    // Initializes agent and pathing. Unparents waypoints
    void Start()
    {
        wayPoints[0].parent.parent = null;
        agent = GetComponent<NavMeshAgent>();
        agent.autoBraking = true;
        agent.autoRepath = true;
        if (wayPoints.Length <= 0) {
            Debug.Log(string.Format("No waypoints set for object {}", transform.gameObject.name));
        }
        SetNextWaypoint(wayPoints[0].position);
        startMoveSpeed = minMoveSpeed < agent.speed ? minMoveSpeed : agent.speed;
    }

    // State switching
    void Update()
    {
        // Clamp suspicion to min and max
        suspicion = Mathf.Clamp(suspicion, minSuspicion, maxSuspicion);

        if (suspicion < 1f && (state != EnemyState.patrol && state != EnemyState.alert)) {
            dontIncrement = true;
            if (reachedThreshhold)
                state = EnemyState.alert;
            else
                state = EnemyState.patrol;
        }
        else if (suspicion >= 1f && state != EnemyState.search && lastDetectedArea && !isChasing) { // Lost line of sight
            state = EnemyState.search;
            reachedThreshhold = true;
        }
        else if (suspicion >= 1f && state == EnemyState.patrol) {
            reachedThreshhold = true;
            state = EnemyState.alert;
        }
        if ((state == EnemyState.patrol || state == EnemyState.alert) && !agent.pathPending && agent.remainingDistance < 0.5f && !isWaiting) { // If at patrol waypoint
            isWaiting = true;
            if (suspicion > minSuspicion) {
                suspicion -= Time.deltaTime / 5;
            }
            SearchForPlayer();
        }
        else if (state == EnemyState.search && !agent.pathPending && agent.remainingDistance < 0.5f) { // If at search waypoint
            if (searchWaypoint + 1 >= searchWaypoints.Length && suspicion >= minSuspicion) // If at last waypoint
                suspicion -= Time.deltaTime / 2;
            if (!isWaiting)
                SearchForPlayer();
        }
        else if (state == EnemyState.chase) { // If chasing the player
            chasePlayer();
        }

        if (state == EnemyState.alert) {
            minMoveSpeed = startMoveSpeed + 1f;
            minSuspicion = minSuspicion <= 0.75f ? 0.75f : minSuspicion;
        }
        else if (state == EnemyState.chase) {
            minMoveSpeed = startMoveSpeed + 1.5f;
            minSuspicion = minSuspicion <= 0.9f ? 0.9f : minSuspicion;
        }
        agent.speed = minMoveSpeed;
    }

    void SetNextWaypoint(Vector3 point) {
        agent.SetDestination(point);
        isWaiting = false;
    }

    void DoAnAction() {
        if (state == EnemyState.search && searchWaypoint + 1 < searchWaypoints.Length) {
            if (searchWaypoint + 2 >= searchWaypoints.Length)
                searchWaypoint = searchWaypoints.Length - 1;
            else
                searchWaypoint = (searchWaypoint + 2) % searchWaypoints.Length;
            SetNextWaypoint(searchWaypoints[searchWaypoint]);
        }
        else if (state == EnemyState.patrol || state == EnemyState.alert) {
            if (!dontIncrement)
                currWaypoint = (currWaypoint + 1) % wayPoints.Length;
            dontIncrement = false;
            SetNextWaypoint(wayPoints[currWaypoint].position);
        }
        isWaiting = false;
    }
    public void attractAttention(Transform pos, float addedSuspicion) {
        dontLook = true;
        minSuspicion = minSuspicion <= 0.5f ? 0.5f : minSuspicion;
        suspicion += addedSuspicion;
        if (suspicion >= 1f) {
            if (cr_running) {
                StopCoroutine(c);
                isWaiting = false;
            }
            agent.isStopped = true;
            agent.ResetPath();
            lastDetectedArea = pos;
            NavMeshPath path = new NavMeshPath();
            NavMesh.CalculatePath(transform.position, lastDetectedArea.position, NavMesh.AllAreas, path);
            searchWaypoints = path.corners;
        }
        if (suspicion >= 2f) {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, 20f);
            foreach (var collider in hitColliders) {
                if (collider.gameObject.tag == "Enemy") {
                    if (collider.gameObject != this.gameObject)
                        collider.gameObject.GetComponent<Enemy>().attractAttention(lastDetectedArea, suspicion / 2);
                }
            }
        }
    }
    public void SearchForPlayer() {
        isWaiting = true;
        if (!dontLook)
            c = StartCoroutine(LookAround());
        else
            DoAnAction();
        dontLook = false;
    }

    public void chasePlayer() {
        isChasing = true;
        agent.isStopped = true;
        agent.ResetPath(); // Constantly update path to point towards player
        RaycastHit hitinfo;
        Physics.Linecast(transform.position, player.transform.position, out hitinfo, mask);
        if (hitinfo.collider.gameObject.tag == "Player") { // if line of sight present from enemy to player
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, 20f);
            foreach (var collider in hitColliders) {
                if (collider.gameObject.tag == "Enemy") { // Attract other nearby enemies if LoS maintained
                    Enemy e = collider.gameObject.GetComponent<Enemy>();
                    e.suspicion += 3;
                    e.state = EnemyState.chase;
                    e.lastDetectedArea = lastDetectedArea;
                    reachedThreshhold = true;
                }
            }
            lastDetectedArea = player.transform; // log last detected are is case LoS lost
            SetNextWaypoint(player.transform.position);
        }
        else {
            state = EnemyState.search; // swap to searching if LoS lost
            reachedThreshhold = true;
            isChasing = false;
            searchWaypoints = new Vector3[] {lastDetectedArea.position};
            searchWaypoint = 0;
            SetNextWaypoint(searchWaypoints[searchWaypoint]); // Move to last known position of player
        }
    }

    // When something enters the look trigger
    void OnTriggerEnter (Collider col) {
        RaycastHit hitinfo;
        Debug.Log(col.gameObject.name);
        if (col.gameObject.tag == "Player") { // If player in look cone
            Physics.Linecast(transform.position, col.transform.position, out hitinfo);
            //If line of sight present start chasing player
            if (hitinfo.collider.gameObject.tag == "Player" && hitinfo.collider.gameObject.GetComponent<PlayerState>().state == PlayerStates.suspicious) {
                state = EnemyState.chase;
                reachedThreshhold = true;
                player = col.gameObject;
                if (suspicion < maxSuspicion)
                    suspicion += 5f;
                minSuspicion = minSuspicion <= 0.5f ? 0.5f : minSuspicion;
                chasePlayer();
            }
        }
    }

    // Looks left to right when called
    IEnumerator LookAround() {
        cr_running = true;
        t = 0;
        float RotationSpeed = 90f;
        Waypoint wp = null;
        if (state == EnemyState.patrol || state == EnemyState.alert)
            wp = wayPoints[currWaypoint].GetComponent<Waypoint>();
        yield return new WaitForSeconds(0.2f);
        if (!wp || wp.turnLeft) {
            while (t <= 90) {
                transform.Rotate (Vector3.up * (RotationSpeed * Time.deltaTime));
                t += RotationSpeed * Time.deltaTime;
                yield return null;
            }
            t = 0;
            yield return new WaitForSeconds(0.5f);
            while (t <= 90) {
                transform.Rotate (-Vector3.up * (RotationSpeed * Time.deltaTime));
                t += RotationSpeed * Time.deltaTime;
                yield return null;
            }
        }
        t = 0;
        if (!wp || wp.turnRight) {
            while (t <= 90) {
                transform.Rotate (-Vector3.up * (RotationSpeed * Time.deltaTime));
                t += RotationSpeed * Time.deltaTime;
                yield return null;
            }
            t = 0;
            yield return new WaitForSeconds(0.5f);
            while (t <= 90) {
                transform.Rotate (Vector3.up * (RotationSpeed * Time.deltaTime));
                t += RotationSpeed * Time.deltaTime;
                yield return null;
            }
        }
        DoAnAction();
        cr_running = false;
    }

    // When the enemy bumps into something
    void OnCollisionEnter(Collision col) {
        if (col.gameObject.tag == "Player") { // Needs changing, reboots the scene for now
            if (col.gameObject.GetComponent<PlayerState>().state == PlayerStates.suspicious)
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        if (state == EnemyState.search) {
            if (col.gameObject.tag == "Distraction") {
                Destroy(col.gameObject); // Needs to be swapped for a check later
                suspicion = minSuspicion;
                if (reachedThreshhold)
                    state = EnemyState.alert;
                else
                    state = EnemyState.patrol;
                dontIncrement = true;
            }
        }
    }
}
