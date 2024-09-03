using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class OvergourdNetworked : MonoBehaviourPunCallbacks
{
    public NavMeshAgent agent;
    public GameObject[] players;
    public List<GameObject> sortedPlayers = new List<GameObject>();
    public LayerMask playerHitMask;
    public AudioSource audio;

    [SerializeField, Tooltip("spawn point for spell attacks")]
    private Transform spellSpawnPoint;

    [SerializeField, Tooltip("prefab for the spell to be used")]
    private GameObject spellPrefab;

    private float timeToTeleport = 6f;
    private float teleportCD = 0f;
    private float helperCD;

    private bool isCasting = false;
    private bool isHoldingSpell = false;
    private GameObject lastTarget;
    private NetworkSpell? lastCastSpell = null;
    private Coroutine? c = null;
    private Coroutine? sc = null;

    private bool lastFrameDistance = false;

    // Start is called before the first frame update
    void Start()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }
        agent = GetComponent<NavMeshAgent>();
        CreatePlayerList();
    }

    // Update is called once per frame
    void Update()
    {
        CheckIsHoldingSpell();
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }
        teleportCD += Time.deltaTime;
        helperCD += Time.deltaTime;
        if (teleportCD > timeToTeleport && !isCasting && !isHoldingSpell)
        {
            teleportCD = 0f;
            helperCD = 0f;
            InitiateTeleport();
        }
        if (helperCD >= 1f && !isCasting && !isHoldingSpell && Vector3.Distance(transform.position, GetClosestPlayer().position) >= 30f)
        {
            teleportCD = 0f;
            helperCD = 0f;
            InitiateTeleport();
        }
        if (!isCasting && !isHoldingSpell && sc == null) // 2f is magic number, just trying to make sure they aren't casting when they could teleport soon instead
        {
            sc = StartCoroutine(SpellAttack());
        }
        Transform cp = GetClosestPlayer();
        var targetConverted = new Vector3(cp.position.x, transform.position.y, cp.position.z);
        transform.LookAt(targetConverted);
    }

    private void LateUpdate()
    {
        if (isHoldingSpell && lastCastSpell && Vector3.Distance(lastCastSpell.transform.position, spellSpawnPoint.transform.position) >= 0.1f)
        {
            if (lastFrameDistance)
                DestroyLastSpell();
            else
                lastFrameDistance = true;
        }
        else
            lastFrameDistance = false;
    }

    void CreatePlayerList()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (!sortedPlayers.Contains(player.transform.root.gameObject) && player.transform.root.GetComponent<PhotonView>())
            {
                sortedPlayers.Add(player.transform.root.gameObject);
            }
        }
    }

    void InitiateTeleport()
    {
        Transform target = GetClosestPlayer();
        Vector3 randomDir = new Vector3((float)Randomizer.GetDouble(1), 0f, (float)Randomizer.GetDouble(1));
        float distance = (float)Randomizer.RandomRange(5f, 20f);
        this.photonView.RPC("DoTeleport", RpcTarget.All, target.position, randomDir, distance);
    }

    Transform GetClosestPlayer()
    {
        Dictionary<float, Transform> map = new Dictionary<float, Transform>();
        foreach (GameObject player in sortedPlayers)
        {
            map.Add(Vector3.Distance(transform.position, player.transform.position), player.transform);
        }
        List<float> keys = new List<float>(map.Keys);
        keys.Sort();
        return map[keys[0]];
    }

    [PunRPC]
    void DoTeleport(Vector3 target, Vector3 dir, float distance)
    {
        transform.position = target + (dir * distance);
        var lookPos = target - transform.position;
        lookPos.y = 0;
        var rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = rotation;
        StartCoroutine(FlickerAgent());
        StartCoroutine(TeleportSound());
    }

    private IEnumerator FlickerAgent()
    {
        var agent = GetComponent<NavMeshAgent>();
        agent.enabled = true;
        yield return null;
        agent.enabled = false;
    }

    public override void OnPlayerEnteredRoom(Player other)
    {
        CreatePlayerList();
    }

    public override void OnPlayerLeftRoom(Player other)
    {
        CreatePlayerList();
    }

    private void CheckIsHoldingSpell()
    {
        if (lastCastSpell && !lastCastSpell.isThrown)
            isHoldingSpell = true;
        else
        {
            if (c == null)
            {
                c = StartCoroutine(SetIsHoldingSpellFalse());
            }
        }
    }

    private IEnumerator SetIsHoldingSpellFalse()
    {
        yield return new WaitForSeconds(0.25f);
        lastCastSpell = null;

        isHoldingSpell = false;
        c = null;
    }

    private IEnumerator SpellAttack()
    {
        isCasting = true;
        var tmp = PhotonNetwork.Instantiate(spellPrefab.name, spellSpawnPoint.position, Quaternion.identity, 0);
        yield return new WaitForSeconds(1 / 90f);
        var s = tmp.GetComponent<NetworkSpell>();
        lastCastSpell = s;
        var f = tmp.AddComponent<FollowObjectWithOffset>();


        tmp.GetComponent<Rigidbody>().velocity = Vector3.zero;
        //s.origin = this;
        s.targetPosition = GetClosestPlayer().position;
        s.waitFireOnSight = false;
        s.fireMask = playerHitMask;
        f.Parent = spellSpawnPoint.gameObject.transform;
        f.enabled = true;
        f.pos = Vector3.zero;
        f._startPos = Vector3.zero;
        
        StartCoroutine(SpellCooldown(s.chargeTime));
        s.c = StartCoroutine(s.ThrowSpell(GetClosestPlayer()));
        yield return null;
    }

    private IEnumerator SpellCooldown(float time)
    {
        yield return new WaitForSeconds(time);
        isCasting = false;
        sc = null;
    }

    public void DestroyLastSpell()
    {
        if (lastCastSpell != null)
        {
            PhotonNetwork.Destroy(lastCastSpell.gameObject);
        }
        lastCastSpell = null;
    }

    IEnumerator TeleportSound()
    {
        yield return new WaitForSeconds(1/90f);
        audio.Play();
    }
}
