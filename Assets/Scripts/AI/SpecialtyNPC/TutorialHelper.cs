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
    public AudioSource voice;
    public AudioSource specialVoice;
    // 0: wakeup 1: walking 2: bar entry 3: page selected
    public List<AudioClip> voicelines = new List<AudioClip>();
    // 0: running 1: running sassy 2: page prompt a 3: page prompt b 4: page prompt c 5: bring prompt a 6: bring prompt b 7: bring prompt c 8: whenever 9: burn prompt a 10: burn prompt b
    public List<AudioClip> specialVoiceLines = new List<AudioClip>();
    [Scene]
    public string tavernScene;

    internal bool started = false;
    internal bool finished = false;
    internal bool paperBurned = false;
    private bool funniWalk = false;

    private float highestMomentumSeen = 0f;
    private bool voicePauseState = false;
    private bool playedVoice1 = false;
    private bool playedVoice2 = false;

    private bool playedRunningVoice = false;
    private bool playedRunningVoiceSassy = false;

    private int pagePrompt = 0;
    private int bringPrompt = 0;

    private bool paperGrabbed = false;

    public float promptTimer = 0f;

    private List<Grabbable> grabbablesLogged = new List<Grabbable>();

    [Button]
    public void CallSetDestination() { SetDestination(); }
    // Start is called before the first frame update
    void Start()
    {
        animator.SetBool("tutorial", true);
        voice.clip = voicelines[0];
        voice.Play();
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
            if (!voice.isPlaying && !specialVoice.isPlaying && !playedVoice1 && !voicePauseState)
            {
                playedVoice1 = true;
                voice.clip = voicelines[1];
                voice.Play();
            }
        }
        if (voicePauseState && !specialVoice.isPlaying)
        {
            voicePauseState = false;
            voice.time = 0f;
            StartCoroutine(DelayStartVoice());
        }
        if (!voice.isPlaying && !specialVoice.isPlaying && playedVoice1 && !playedVoice2 && finished && !voicePauseState)
        {
            playedVoice2 = true;
            voice.clip = voicelines[2];
            voice.Play();
        }
        var currM = momentumController.GetMomentum();
        highestMomentumSeen = currM > highestMomentumSeen ? currM : highestMomentumSeen;
        if (currM == 0f && funniWalk)
        {
            highestMomentumSeen = 0f;
            funniWalk = false;
            StartCoroutine(DisableFunniWalk());
        }
        if (highestMomentumSeen >= 850 && !finished)
        {
            agent.speed = 0.5f;
            animator.SetBool("slowWalk", true);
            funniWalk = true;
            if (!playedRunningVoiceSassy)
            {
                playedRunningVoiceSassy = true;
                if (voice.isPlaying)
                {
                    voice.Pause();
                    voicePauseState = true;
                }
                if (specialVoice.isPlaying)
                    specialVoice.Stop();
                specialVoice.clip = specialVoiceLines[1];
                specialVoice.Play();
            }
        }
        else if (highestMomentumSeen >= 450f && !finished)
        {
            if (!playedRunningVoice)
            {
                playedRunningVoice = true;
                if (voice.isPlaying)
                {
                    voice.Pause();
                    voicePauseState = true;
                }
                if (specialVoice.isPlaying)
                    specialVoice.Stop();
                specialVoice.clip = specialVoiceLines[0];
                specialVoice.Play();
            }
            agent.speed = 3.5f;
        }
        if (playedVoice2 && !voice.isPlaying)
        {
            promptTimer += Time.deltaTime;
        }
        if (!paperGrabbed)
        {
            if (promptTimer >= (15f - (pagePrompt * 2)) && pagePrompt < 4)
            {
                if (voice.isPlaying)
                    voice.Pause();
                if (specialVoice.isPlaying)
                    specialVoice.Stop();
                specialVoice.clip = specialVoiceLines[pagePrompt + 2];
                specialVoice.Play();
                promptTimer = 0f;
                pagePrompt++;
            }
        } else if (paperGrabbed && !paperBurned)
        {
            if (promptTimer >= (15f - (pagePrompt + bringPrompt)) && bringPrompt < 4)
            {
                if (voice.isPlaying)
                    voice.Pause();
                if (specialVoice.isPlaying)
                    specialVoice.Stop();
                specialVoice.clip = specialVoiceLines[bringPrompt + 6];
                specialVoice.Play();
                promptTimer = 0f;
                bringPrompt++;
            }
        }
    }

    private IEnumerator DelayStartVoice()
    {
        yield return new WaitForSeconds(0.5f);

        voice.Play();
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
            if (voice.isPlaying)
                voice.Stop();
            if (specialVoice.isPlaying)
                specialVoice.Stop();
            specialVoice.clip = specialVoiceLines[11];
            specialVoice.Play();
            StartCoroutine(DelayedLeaveScene());
            return;
        }
        if (voice.isPlaying)
            voice.Pause();
        if (specialVoice.isPlaying)
            specialVoice.Stop();
        specialVoice.clip = specialVoiceLines[10];
        specialVoice.Play();
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
        qs.delayTime = 4f;

        Burnable br = g.GetComponentInChildren<Burnable>();
        br.BurnStarted += CreateNewPaper;

        Grabbable gr = g.GetComponentInChildren<Grabbable>();
        gr.parentOnGrab = true;
        gr.OnGrabEvent += ResetAnimToIdle;
        gr.OnGrabEvent += EnableQuestPaperDelayed;
        gr.OnGrabEvent += PaperGrabbedHelper;

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

    public void PaperGrabbedHelper(Hand h, Grabbable g)
    {
        if (grabbablesLogged.Contains(g)) return;
        grabbablesLogged.Add(g);
        paperGrabbed = true;
        if (pagePrompt >= 2)
            promptTimer = 15f;
        else
            promptTimer = 0f;
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
        yield return new WaitForSeconds(4);
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
