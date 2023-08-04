using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System.Diagnostics;

public class AleRefill : MonoBehaviour
{
    [Header("References")]
    public GameObject aleCollider;
    public MeshRenderer aleRenderer;
    public GameObject alePourEffect;
    public List<Transform> aleSpawnLocations = new List<Transform>();

    [Header("Fill controls")]
    public float localYMin = 0f;
    public float localYMax = 0f;
    public float fillSpeed = 0.1f;
    public bool randomFill = false;

    [Header("Liquid Behavior")]
    public float sloshSpeed = 60f;
    public float rotationSpeed = 15f;
    public float clampVal = 25f;

    [ShowNonSerializedField]
    private float fillPercent;
    bool particleCollided = false;

    private Quaternion startRotation;

    void Start()
    {
        if (randomFill)
        {
            fillPercent = (float)Randomizer.GetDouble(1);
            aleRenderer.material.SetFloat("_fillPercent", fillPercent);
        }
        fillPercent = aleRenderer.material.GetFloat("_fillPercent");
        startRotation = Quaternion.Euler(transform.localRotation.x, 0, transform.localRotation.z);
    }


    // Update is called once per frame
    void Update()
    {
        Quaternion tmp = Quaternion.Euler(transform.localRotation.x, 0, transform.localRotation.z);
        float angle = Quaternion.Angle(startRotation, tmp);
        float maxAngle = ((1 - fillPercent) * 50) + 20;
        if (angle * 100f > maxAngle && fillPercent > 0)
        {
            Transform lowestLoc = aleSpawnLocations[0];
            foreach (var t in aleSpawnLocations)
            {
                if (t.position.y < lowestLoc.position.y)
                {
                    lowestLoc = t;
                }
            }
            alePourEffect.transform.position = lowestLoc.position;
            alePourEffect.SetActive(true);
            if (angle * 100f > maxAngle + 10)
                fillPercent -= fillSpeed * Time.deltaTime * 3;
            else
                fillPercent -= fillSpeed * Time.deltaTime;
        }
        else
        {
            alePourEffect.SetActive(false);
        }

        if (fillPercent <= 0.05)
        {
            aleRenderer.enabled = false;
        } else
        {
            aleRenderer.enabled = true;
        }


        if (particleCollided && fillPercent < 1f)
        {
            fillPercent += fillSpeed * Time.deltaTime;
        }



        particleCollided = false;

        aleRenderer.material.SetFloat("_fillPercent", fillPercent);
    }

    void OnParticleCollision(GameObject other)
    {
        if (other.CompareTag("Ale"))
        {
            particleCollided = true;
        }
    }

    private void Slosh()
    {
        Quaternion iRotation = Quaternion.Inverse(transform.localRotation);
        UnityEngine.Debug.Log(string.Format("Inverse: {0}", iRotation));

        Vector3 fRotation = Quaternion.RotateTowards(aleCollider.transform.localRotation, iRotation, sloshSpeed * Time.deltaTime).eulerAngles;
        UnityEngine.Debug.Log(string.Format("Pre clamp: {0}", fRotation));

        fRotation.x = ClampRotationValue(fRotation.x, clampVal);
        fRotation.y = 0f;
        fRotation.z = ClampRotationValue(fRotation.z, clampVal);

        UnityEngine.Debug.Log(string.Format("Post clamp: {0}", fRotation));


        aleCollider.transform.localEulerAngles = fRotation;
    }

    private float ClampRotationValue(float value, float clampValue)
    {
        float returnVal = 0f;

        if (value > 180)
        {
            returnVal = Mathf.Clamp(value, 360 - clampValue, 360);
        } else
        {
            returnVal = Mathf.Clamp(value, 0, clampValue);
        }

        return returnVal;
    }
}
