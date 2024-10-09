using Autohand;
using Com.Aaru.Sassin;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class TutorialHelper : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform target;
    public Animator animator;
    public UnityEvent onFinishMove;
    public Launcher launcher;
    public QuestBoardInfo quest;
    public GameObject paper;
    public Transform paperParent;
    public MomentumController momentumController;
    [Scene]
    public string tavernScene;

    internal bool started = false;
    internal bool finished = false;
    internal bool paperBurned = false;
    private bool funniWalk = false;

    private float highestMomentumSeen = 0f;

    [Button]
    public void CallSetDestination() { SetDestination(); }
    // Start is called before the first frame update
    void Start()
    {
        animator.SetBool("tutorial", true);
    }

    // Update is called once per frame
    void Update()
    {
        // Check if we've reached the destination
        if (started && !finished)
        {
            if (!agent.pathPending)
            {
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                    {
                        // Done
                        UnityEngine.Debug.Log("Finished");
                        finished = true;
                        animator.SetBool("idle", true);
                        onFinishMove.Invoke();
                    }
                }
            }
        }
        var currM = momentumController.GetMomentum();
        highestMomentumSeen = currM > highestMomentumSeen ? currM : highestMomentumSeen;
        if (currM == 0f && funniWalk)
        {
            highestMomentumSeen = 0f;
            funniWalk = false;
            StartCoroutine(DisableFunniWalk());
        }
        if (highestMomentumSeen >= 850)
        {
            agent.speed = 0.5f;
            animator.SetBool("slowWalk", true);
            funniWalk = true;
        }
        else if (highestMomentumSeen >= 450f)
        {
            agent.speed = 3.5f;
        }
    }

    public void SetDestination()
    {
        if (agent != null && target != null && !started && !finished)
        {
            agent.SetDestination(target.position);
            started = true;
            if (animator)
                animator.SetBool("normalWalk", true);
        }
    }

    public void CreateNewPaper(Paper p)
    {
        if (paperBurned)
        {
            PlayerPrefs.SetInt("BeatTutorial", 1);
            StartCoroutine(DelayedLeaveScene());
            return;
        }
        paperBurned = true;
        StartCoroutine(CreateNewPaper());
        animator.SetBool("handoff", true);
    }

    private IEnumerator CreateNewPaper()
    {
        GameObject g = Instantiate(paper, paperParent.position, paperParent.rotation);
        yield return null;
        g.transform.parent = paperParent;
        PaperConstructor pc = g.GetComponentInChildren<PaperConstructor>();
        pc.SetText(quest.paperText);

        QuestStarter qs = g.AddComponent<QuestStarter>();
        qs.launcher = launcher;
        qs.sceneToLoad = quest.sceneToLoad;
        qs.ignoreStart = true;

        Burnable br = g.GetComponentInChildren<Burnable>();
        br.BurnStarted += CreateNewPaper;

        Grabbable gr = g.GetComponentInChildren<Grabbable>();
        gr.parentOnGrab = true;
        gr.OnGrabEvent += ResetAnimToIdle;
        gr.OnGrabEvent += EnableQuestPaperDelayed;

        Paper p = g.GetComponentInChildren<Paper>();
        p.startStabbed = true;

        Rigidbody rb = g.GetComponentInChildren<Rigidbody>();
        rb.isKinematic = true;

        g.transform.localPosition = Vector3.zero;
    }

    public void ResetAnimToIdle(Hand h, Grabbable g)
    {
        animator.SetBool("handoff", false);
        g.OnGrabEvent -= ResetAnimToIdle;
    }

    public void EnableQuestPaperDelayed(Hand h, Grabbable g)
    {
        g.OnGrabEvent -= EnableQuestPaperDelayed;
        StartCoroutine(EnableStart(g.gameObject));
    }

    private IEnumerator EnableStart(GameObject g)
    {
        var rb = g.GetComponentInChildren<Rigidbody>();
        rb.isKinematic = false;
        g.transform.parent = null;
        yield return new WaitForSeconds(1);
        var qs = g.GetComponent<QuestStarter>();
        if (qs)
            qs.ignoreStart = false;
    }

    private IEnumerator DelayedLeaveScene()
    {
        yield return new WaitForSeconds(3);
        var ls = GetComponent<MoveToLoadingScene>();
        ls.LoadLoadingScene(1);
    }

    private IEnumerator DisableFunniWalk()
    {
        yield return new WaitForSeconds(3);
        if (momentumController.GetMomentum() <= 850 && highestMomentumSeen < 850)
        {
            animator.SetBool("slowWalk", false);
            agent.speed = 1.5f;
        }
    }
}
