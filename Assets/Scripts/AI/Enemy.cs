using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using NaughtyAttributes;


// Allows an object with a navmeshagent to patrol to preset waypoints on most efficient path
public class Enemy : MonoBehaviour
{
    [Header("References")]
    [SerializeField, Tooltip("All waypoints this AI moves to")]
    private Transform[] wayPoints;

    
    [Header("Booleans")]
    [SerializeField, Tooltip("if true sets the agent to use melee attacks instead of spells")]
    private bool meleeAttack = false;

    [ShowIf("meleeAttack")]
    [SerializeField, Tooltip("")]
    private float meleeDamage = 1f;


    [Header("Spell Attack")]
    [HideIf("meleeAttack")]
    [SerializeField, Tooltip("spawn point for spell attacks")]
    private Transform spellSpawnPoint;

    [HideIf("meleeAttack")]
    [SerializeField, Tooltip("prefab for the spell to be used")]
    private GameObject spellPrefab;

    [HideIf("meleeAttack")]
    [SerializeField, Tooltip("")]
    private float spellDamage = 1f;


    [Header("Variables")]
    [SerializeField, Tooltip("index of current waypoint")]
    private int currWaypoint = 0;

    [SerializeField, Tooltip("Agent's suspicion of player")]
    private float suspicion = 0f;

    [SerializeField, Tooltip("minimum suspicion")]
    private float minSuspicion = 0f;

    [SerializeField, Tooltip("Maximum suspicion - Recommend all are the same for consistency")]
    private float maxSuspicion = 8f;

    [SerializeField, Tooltip("minimum move speed")]
    private float minMoveSpeed;


    [Header("Other Var Types")]
    [SerializeField, Tooltip("mask used for linecasts")]
    private LayerMask mask;
    
    [SerializeField, Tooltip("which state the enemy is in")]
    internal EnemyState state = EnemyState.patrol;
    
    [SerializeField, Tooltip("Last position player was seen")]
    private Transform lastDetectedArea;

    public Animator anim;

    public randNum randomNumberGenerator;

    public bool pickRandomWaypoint = false;

    public GameObject eyeLinePosition;

    public Stats stats;


    // Unserialized vars

    // Waypoints auto generated while searching
    private Vector3[] searchWaypoints;

    // The navmeshagent component
    private NavMeshAgent agent;

    // index of search waypoint
    private int searchWaypoint = 0;

    // Is the agent currently waiting
    private bool isWaiting = false;

    // is the agent currently chasing
    private bool isChasing = false;

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

    // prevents the agent from incrementing waypoint count while doing other things
    private bool dontIncrement = false;

    // has the agent reached the threshold for the alert state
    private bool reachedThreshhold = false;

    // starting move speed
    private float startMoveSpeed;

    // is the agent currently capable of starting an attack - currently only used for spells
    internal bool canAttack = true;

    private Coroutine searchCoroutine;

    private bool scr_running = false;

    private bool chasedThisFrame = false;

    private EnemyState savedState;

    [Button]
    private void DoKneel() { Kneel(); }


    void Start()
    {
        wayPoints[0].parent.parent = null;
        agent = GetComponent<NavMeshAgent>();
        agent.autoBraking = true;
        agent.autoRepath = true;
        if (wayPoints.Length <= 0) {
            Debug.LogWarning(string.Format("No waypoints set for object {}", transform.gameObject.name));
        }
        SetNextWaypoint(wayPoints[0].position);
        startMoveSpeed = minMoveSpeed < agent.speed ? minMoveSpeed : agent.speed;
        if (!spellSpawnPoint)
            spellSpawnPoint = transform;
        if (!spellPrefab && !meleeAttack) {
            Debug.LogError("Variable 'Spell Attack' is not defined");
        }
    }

    // State switching
    void Update()
    {
        chasedThisFrame = false;
        // Clamp suspicion to min and max
        suspicion = Mathf.Clamp(suspicion, minSuspicion, maxSuspicion);

        if (agent.path.status == NavMeshPathStatus.PathPartial && agent.velocity == Vector3.zero && state == EnemyState.chase && agent.path.corners.Length <= 2)
        {
            state = EnemyState.search;
        }

        if (suspicion < 1f && (state != EnemyState.patrol && state != EnemyState.alert)) {
            dontIncrement = true;
            if (reachedThreshhold)
                state = EnemyState.alert;
            else
                state = EnemyState.patrol;
        }
        else if (suspicion >= 1f && state != EnemyState.search && lastDetectedArea && !isChasing && state != EnemyState.wounded) { // Lost line of sight
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
            if ((searchWaypoints == null || searchWaypoint + 1 >= searchWaypoints.Length) && suspicion >= minSuspicion) {// If at last waypoint
                searchWaypoints = null;
                searchWaypoint = 0;
                suspicion -= Time.deltaTime / 2;
            }
            if (!isWaiting)
                SearchForPlayer();
        }
        else if (state == EnemyState.chase && !chasedThisFrame) { // If chasing the player
            chasePlayer();
        }

        float speed = minMoveSpeed;
        if (state == EnemyState.alert) {
            minMoveSpeed = startMoveSpeed + 1f;
            minSuspicion = minSuspicion <= 0.75f ? 0.75f : minSuspicion;
            speed = minMoveSpeed;
        }
        else if (state == EnemyState.chase) {
            speed = startMoveSpeed + 1.5f;
            minSuspicion = minSuspicion <= 0.9f ? 0.9f : minSuspicion;
        }
        agent.speed = speed;

        if (c != null && state == EnemyState.chase)
        {
            StopCoroutine(c);
            agent.updateRotation = true;
            cr_running = false;
            if (anim)
                anim.SetBool("IsLooking", false);
        }

        if (anim) {
            switch (state)
            {
                case EnemyState.patrol:
                    anim.SetBool("IsWalking", true);
                    anim.SetBool("IsChasing", false);
                    break;

                case EnemyState.alert:
                    anim.SetBool("IsWalking", true);
                    anim.SetBool("IsChasing", false);
                    break;

                case EnemyState.search:
                    anim.SetBool("IsWalking", false);
                    anim.SetBool("IsLooking", true);
                    anim.SetBool("IsChasing", false);
                    break;

                case EnemyState.chase:
                    anim.SetBool("IsWalking", false);
                    anim.SetBool("IsChasing", true);
                    break;

                default:
                    anim.SetBool("IsWalking", false);
                    anim.SetBool("IsChasing", false);
                    anim.SetBool("IsLooking", true);
                    break;
            }
        }
    }

    // Sets the next waypoint for the AI
    void SetNextWaypoint(Vector3 point) {
        NavMeshPath navMeshPath = new NavMeshPath();
        //create path and check if it can be done
        // and check if navMeshAgent can reach its target
        if (agent.CalculatePath(point, navMeshPath) && navMeshPath.status == NavMeshPathStatus.PathComplete)
        {
            //Debug.Log(navMeshPath.corners.Length);
            //move to target
            agent.SetPath(navMeshPath);
            isWaiting = false;
        }
        else
        {
            if (navMeshPath.corners.Length > 0)
            {
                agent.SetPath(navMeshPath);
            }
            else
            {
                if (state == EnemyState.chase)
                {
                    reachedThreshhold = true;
                    isChasing = false;
                    state = EnemyState.search;
                    searchWaypoints = navMeshPath.corners;
                    searchWaypoint = 0;
                    DoAnAction();
                }
            }
        }
    }

    // Determines next waypoint, and sets it
    void DoAnAction() {
        if (state == EnemyState.search && searchWaypoints != null && searchWaypoint + 1 < searchWaypoints.Length) {
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

            if (pickRandomWaypoint && randomNumberGenerator != null && randomNumberGenerator.rand != null)
            {
                int lastWaypoint = currWaypoint;
                while (currWaypoint == lastWaypoint)
                    currWaypoint = randomNumberGenerator.rand.Next((wayPoints.Length - 1));
            }
            SetNextWaypoint(wayPoints[currWaypoint].position);
        }
        isWaiting = false;
    }

    // Called by outside functions, attracts the attention of the AI
    public void attractAttention(Transform pos, float addedSuspicion) {
        dontLook = true;
        minSuspicion = minSuspicion <= 0.5f ? 0.5f : minSuspicion;
        suspicion += addedSuspicion;
        if (suspicion >= 1f) {
            if (cr_running) {
                StopCoroutine(c);
                agent.updateRotation = true;
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

    // Looks around for the player after losing LoS
    public void SearchForPlayer() {
        isWaiting = true;
        if (!dontLook)
            c = StartCoroutine(LookAround());
        else
            DoAnAction();
        dontLook = false;
    }

    // Chases the player while line of sight is maintained
    public void chasePlayer() {
        if (chasedThisFrame)
            return;
        chasedThisFrame = true;
        isChasing = true;
        agent.isStopped = true;
        agent.ResetPath(); // Constantly update path to point towards player
        if (!player) {
            state = EnemyState.alert;
            suspicion = minSuspicion * 2;
            DoAnAction();
            return;
        }
        RaycastHit hitinfo;
        Physics.Linecast(eyeLinePosition.transform.position, player.transform.position, out hitinfo, mask);
        if (hitinfo.collider && hitinfo.collider.gameObject.tag == "Player") { // if line of sight present from enemy to player
            if (scr_running == true)
            {
                StopCoroutine(searchCoroutine);
                scr_running = false;
            }
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
            if (hitinfo.distance <= 10f && !meleeAttack) {
                if (canAttack) {
                    StartCoroutine(SpellAttack());
                    canAttack = false;
                }
                return;
            }
            //Debug.Log("Waypoint set");
            SetNextWaypoint(player.transform.position);
        }
        else {
            lastDetectedArea = player.transform;
            SetNextWaypoint(player.transform.position);
            if (scr_running)
                searchCoroutine = StartCoroutine(LoSLostCheck());
        }
    }

    // When something enters the look trigger
    void OnTriggerEnter(Collider col)
    {
        RaycastHit hitinfo;
        //Debug.Log(col.gameObject.transform.root.gameObject.name);
        if (col.attachedRigidbody && col.attachedRigidbody.gameObject.tag == "Player")
        { // If player in look cone
            Physics.Linecast(eyeLinePosition.transform.position, col.transform.position, out hitinfo);
            //If line of sight present start chasing player
            if (hitinfo.collider.gameObject.tag == "Player" && hitinfo.collider.transform.root.gameObject.GetComponentInChildren<PlayerState>().state == PlayerStates.suspicious)
            {
                player = col.gameObject;
                if (suspicion < maxSuspicion)
                    suspicion += 5f;
                minSuspicion = minSuspicion <= 0.5f ? 0.5f : minSuspicion;
                if (state == EnemyState.wounded)
                {
                    savedState = EnemyState.chase;
                }
                else
                {
                    state = EnemyState.chase;
                    reachedThreshhold = true;
                    chasePlayer();
                }
            }
        }
    }

    // When something enters the look trigger
    void OnTriggerStay (Collider col) {
        RaycastHit hitinfo;
        //Debug.Log(col.gameObject.transform.root.gameObject.name);
        if (col.attachedRigidbody && col.attachedRigidbody.gameObject.tag == "Player") { // If player in look cone
            if (chasedThisFrame)
                return;
            Physics.Linecast(eyeLinePosition.transform.position, col.transform.position, out hitinfo);
            //If line of sight present start chasing player
            if (hitinfo.collider.gameObject.tag == "Player" && hitinfo.collider.transform.root.gameObject.GetComponentInChildren<PlayerState>().state == PlayerStates.suspicious) {
                player = col.gameObject;
                if (suspicion < maxSuspicion)
                    suspicion += 5f;
                minSuspicion = minSuspicion <= 0.5f ? 0.5f : minSuspicion;
                if (state == EnemyState.wounded)
                {
                    savedState = EnemyState.chase;
                }
                else
                {
                    state = EnemyState.chase;
                    reachedThreshhold = true;
                    chasePlayer();
                }
            }
        }
    }

    // Looks left to right when called
    IEnumerator LookAround() {
        agent.isStopped = true;
        if (anim)
            anim.SetBool("IsLooking", true);
        agent.updateRotation = false;
        cr_running = true;
        t = 0;
        yield return new WaitForSeconds(4.5f);
        //float RotationSpeed = 90f;
        //Waypoint wp = null;
        //if (state == EnemyState.patrol || state == EnemyState.alert)
        //    wp = wayPoints[currWaypoint].GetComponent<Waypoint>();
        //yield return new WaitForSeconds(0.2f);
        //if (!wp || wp.turnLeft) {
         //   while (t <= 90) {
         //       transform.Rotate (Vector3.up * (RotationSpeed * Time.deltaTime));
         //       t += RotationSpeed * Time.deltaTime;
        //yield return null;
           // }
           // t = 0;
        //yield return new WaitForSeconds(0.5f);
           // while (t <= 90) {
           //     transform.Rotate (-Vector3.up * (RotationSpeed * Time.deltaTime));
           //     t += RotationSpeed * Time.deltaTime;
           //     yield return null;
           // }
        //}
       // t = 0;
        //if (!wp || wp.turnRight) {
        //    while (t <= 90) {
        //        transform.Rotate (-Vector3.up * (RotationSpeed * Time.deltaTime));
        //        t += RotationSpeed * Time.deltaTime;
        //yield return null;
        //    }
        //    t = 0;
        //yield return new WaitForSeconds(0.5f);
       //     while (t <= 90) {
        //        transform.Rotate (Vector3.up * (RotationSpeed * Time.deltaTime));
         //       t += RotationSpeed * Time.deltaTime;
        //yield return null;
         //   }
       // }
        agent.updateRotation = true;
        DoAnAction();
        cr_running = false;
        if (anim)
            anim.SetBool("IsLooking", false);
        agent.isStopped = false;
    }

    // When the enemy bumps into something
    void OnCollisionEnter(Collision col) {
        if (col.gameObject.tag == "Player") { // Needs changing, reboots the scene for now
            if (col.gameObject.GetComponent<PlayerState>().state == PlayerStates.suspicious)
            {
                var s = col.gameObject.GetComponent<Stats>();
                if (s)
                {
                    s.OnDamageReceived(meleeDamage);
                }
            }
                
        }
        if (state == EnemyState.search) {
            if (col.gameObject.tag == "Distraction") {
                Destroy(col.gameObject); // Needs to be swapped for a check later
                suspicion = minSuspicion;
                lastDetectedArea = null;
                if (reachedThreshhold)
                    state = EnemyState.alert;
                else
                    state = EnemyState.patrol;
                dontIncrement = true;
            }
        }
    }

    // Starts an attack with a spell
    IEnumerator SpellAttack() {
        var tmp = Instantiate(spellPrefab, spellSpawnPoint.position, Quaternion.identity, spellSpawnPoint);
        var s = tmp.GetComponent<Spell>();
        var f = tmp.GetComponent<FollowObjectWithOffset>();

        tmp.GetComponent<Rigidbody>().velocity = Vector3.zero;
        s.origin = this;
        s.targetPosition = lastDetectedArea.position;
        s.damage = spellDamage;
        f.Parent = spellSpawnPoint.gameObject.transform;
        f.enabled = true;
        f.pos = Vector3.zero;
        f._startPos = Vector3.zero;
        StartCoroutine(SpellCooldown(s.chargeTime + 1f));
        StartCoroutine(s.ThrowSpell());
        yield return null;
    }

    // Cooldown after ending last spell before starting a new one
    IEnumerator SpellCooldown(float time) {
        yield return new WaitForSeconds(time);
        canAttack = true;
    }

    public void SetEnemyStateAlertManually()
    {
        state = EnemyState.alert;
    }

    public EnemyState GetCurrentState()
    {
        return state;
    }

    private IEnumerator LoSLostCheck()
    {
        //Debug.Log("LoS lost");
        scr_running = true;
        yield return new WaitForSeconds(1.5f);
        if (state == EnemyState.chase)
        {
            //Debug.Log("In check");
            RaycastHit hitinfo;
            Physics.Linecast(transform.position, player.transform.position, out hitinfo, mask);
            if (hitinfo.collider.gameObject.tag == "Player") { // if line of sight present from enemy to player
                //Debug.Log("Player seen");
                scr_running = false;
                yield break;
            }
            state = EnemyState.search; // swap to searching if LoS lost
            reachedThreshhold = true;
            isChasing = false;
            DoAnAction();
            //Debug.Log("Did an action");
        }
        scr_running = false;
    }

    public void SetLayer(int layer)
    {
        Transform[] children = gameObject.GetComponentsInChildren<Transform>(includeInactive: true);
        foreach (Transform child in children)
        {
            child.gameObject.layer = layer;
        }
    }

    public void OnTakeDamage()
    {
        if (state != EnemyState.chase)
            SearchForPlayer();
        if (state == EnemyState.patrol)
            SetEnemyStateAlertManually();
    }

    public void Kneel()
    {
        //Debug.Log("Kneel");
        anim.SetBool("WoundedKnee", true);
        suspicion = maxSuspicion;
        savedState = state;
        state = EnemyState.wounded;
        dontIncrement = true;
        agent.isStopped = true;
        StartCoroutine(UnKneel());
    }

    private IEnumerator UnKneel()
    {
        yield return new WaitForSeconds(5f);
        anim.SetBool("WoundedKnee", false);
        state = savedState;
    }
}
