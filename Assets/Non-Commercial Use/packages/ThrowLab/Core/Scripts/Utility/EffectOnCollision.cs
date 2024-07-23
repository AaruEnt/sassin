using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectOnCollision : MonoBehaviour {

    public ParticleSystem _particleEffect;
    public AudioSource _audio;
    public MeshRenderer _flashRenderer;


    private void OnCollisionEnter(Collision collision)
    {
        Effect();    
    }

    private void OnTriggerEnter(Collider other)
    {
        Effect();
    }


    private void Effect()
    {
        if (_particleEffect)
        {
            _particleEffect.Play();
        }
        if (_audio)
        {
            _audio.Play();
        }
        if (_flashRenderer)
        {
            StartCoroutine(FlashRenderer(_flashRenderer));
        }
    }
    
    private IEnumerator FlashRenderer(MeshRenderer renderer)
    {
        renderer.material.EnableKeyword("_EMISSION");
        renderer.material.SetColor("_EmissionColor", Color.white);
        yield return new WaitForSeconds(.1f);
        renderer.material.SetColor("_EmissionColor", Color.clear);

    }
}
