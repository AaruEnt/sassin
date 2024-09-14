using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurnHelper : MonoBehaviour
{
    public Burnable crystal;

    public void CallStartBurn()
    {
        crystal.StartBurnManual();
    }
}
