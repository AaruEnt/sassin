using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.AI;

public class OvergourdNetworked : MonoBehaviourPunCallbacks
{
    public NavMeshAgent agent;
    public GameObject[] players;
    public List<GameObject> sortedPlayers = new List<GameObject>();

    private float timeToTeleport = 6f;
    private float teleportCD = 0f;

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
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }
        teleportCD += Time.deltaTime;
        if (teleportCD > timeToTeleport)
        {
            teleportCD = 0f;
            InitiateTeleport();
        }
    }

    void CreatePlayerList()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (!sortedPlayers.Contains(player.transform.root.gameObject))
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
}
