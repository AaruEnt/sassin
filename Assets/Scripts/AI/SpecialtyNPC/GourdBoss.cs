using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class GourdBoss : MonoBehaviour
{
    public List<GameObject> spawns = new List<GameObject> ();
    public SphereCollider arena;
    public Transform player;
    public float runDistance = 50f;
    public AudioSource tp;
    public LayerMask playerHitMask;

    public bool disableTeleport = false;

    [SerializeField, Tooltip("spawn point for spell attacks")]
    private Transform spellSpawnPoint;

    [SerializeField, Tooltip("prefab for the spell to be used")]
    private GameObject spellPrefab;

    [SerializeField, Tooltip("")]
    private float spellDamage = 100f;

    private bool isHoldingSpell = false;
    private bool isCasting = false;
    private float teleportCD = 10f;
    private float cd = 0f;
    private bool isRunning = false;
    private GameObject lastTarget;
    private Spell lastCastSpell = null;
    private Coroutine? c = null;

    // Start is called before the first frame update
    void Start()
    {
        lastTarget = spawns[0];
    }

    // Update is called once per frame
    void Update()
    {
        CheckIsHoldingSpell();
        if (!isRunning && Vector3.Distance(transform.position, player.position) < runDistance) {
            isRunning = true;
        }

        if (isRunning)
        {
            Vector3 tmp1 = player.position;
            tmp1.y = transform.position.y;
            transform.LookAt(tmp1);
            cd -= Time.deltaTime;
            float distance = Vector3.Distance(transform.position, player.position);
            if (!isCasting && !isHoldingSpell && (cd >= 0.5f || distance >= 10f)) // 2f is magic number, just trying to make sure they aren't casting when they could teleport soon instead
            {
                StartCoroutine(SpellAttack());
            }
            else if (!isCasting && !isHoldingSpell && cd <= 0f && distance < 10f && !disableTeleport)
            {
                cd = teleportCD;
                List<GameObject> tmp2 = new List<GameObject>(spawns);
                tmp2.Remove(lastTarget);
                lastTarget = Randomizer.PickRandomObject(tmp2);
                Teleport(lastTarget.transform);
            }
        }
    }

    void Teleport(Transform target)
    {
        transform.position = target.position;
        tp.Play();
    }

    // Starts an attack with a spell
    IEnumerator SpellAttack()
    {
        isCasting = true;
        var tmp = Instantiate(spellPrefab, spellSpawnPoint.position, Quaternion.identity, spellSpawnPoint);
        var s = tmp.GetComponent<Spell>();
        lastCastSpell = s;
        var f = tmp.GetComponent<FollowObjectWithOffset>();

        tmp.GetComponent<Rigidbody>().velocity = Vector3.zero;
        //s.origin = this;
        s.targetPosition = player.position;
        s.damage = spellDamage;
        s.waitFireOnSight = true;
        s.fireMask = playerHitMask;
        f.Parent = spellSpawnPoint.gameObject.transform;
        f.enabled = true;
        f.pos = Vector3.zero;
        f._startPos = Vector3.zero;
        StartCoroutine(SpellCooldown(s.chargeTime));
        StartCoroutine(s.ThrowSpell(player.transform));
        yield return null;
    }

    // Cooldown after ending last spell before starting a new one
    IEnumerator SpellCooldown(float time)
    {
        yield return new WaitForSeconds(time);
        isCasting = false;
    }

    public void DestroyLastSpell()
    {
        UnityEngine.Debug.Log(string.Format("Last spell: {0}", lastCastSpell?.transform.name));
        if (lastCastSpell != null)
        {
            Destroy(lastCastSpell.gameObject);
        }
    }

    private void CheckIsHoldingSpell()
    {
        if (spellSpawnPoint.childCount > 0)
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
        lastCastSpell = null;
        yield return new WaitForSeconds(0.25f);

        isHoldingSpell = false;
        c = null;
    }
}
