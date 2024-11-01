using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacoEaten : MonoBehaviour
{
    private TacoFire tacofire;
    // Start is called before the first frame update
    void Start()
    {
        tacofire = FindObjectOfType<TacoFire>();
    }

    public void EatTaco()
    {
        tacofire?.EatenTaco();
    }
}
