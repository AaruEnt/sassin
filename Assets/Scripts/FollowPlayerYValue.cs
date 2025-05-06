using Autohand;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayerYValue : MonoBehaviour
{
    public AutoHandPlayer player;
    public float yOffset;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (player.IsGrounded())
        {
            var tmp = transform.position;
            tmp.y = player.transform.position.y - yOffset;
            transform.position = tmp;
        }
    }

    public void Reset()
    {
        transform.position = new Vector3 (0, -25f, 0);
    }
}
