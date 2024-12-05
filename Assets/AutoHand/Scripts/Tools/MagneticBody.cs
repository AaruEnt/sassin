using Autohand;
using UnityEngine;

[ HelpURL("https://app.gitbook.com/s/5zKO0EvOjzUDeT2aiFk3/auto-hand/extras/magnetic-forces")]
public class MagneticBody : MonoBehaviour
{
    public Rigidbody body;
    public int magneticIndex = 0;
    public float strengthMultiplyer = 1f;
    public UnityMagneticEvent magneticEnter;
    public UnityMagneticEvent magneticExit;

    private void Start() {
        if(body == null)
            body = GetComponent<Rigidbody>();
    }
}
