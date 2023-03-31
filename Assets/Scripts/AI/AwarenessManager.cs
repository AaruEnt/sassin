using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AwarenessManager : MonoBehaviour
{
    [SerializeField, Tooltip("The enemy attached to the current object. If unset, will attempt to find on its own.")]
    internal Enemy enemy;

    [SerializeField, Tooltip("The awareness trigger for when the enemy is alert or higher")]
    private GameObject alertAwareTrigger;
    // Start is called before the first frame update
    void Start()
    {
        if (!enemy)
            enemy = GetComponent<Enemy>();
        if (!enemy)
        {
            Debug.LogWarning(string.Format("No enemy found on object {0}", this.gameObject.name));
            Destroy(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (enemy.GetCurrentState() != EnemyState.patrol)
            alertAwareTrigger.SetActive(true);
    }
}
