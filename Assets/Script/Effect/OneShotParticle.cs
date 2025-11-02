using UnityEngine;

public class OneShotParticle : MonoBehaviour
{
    void Start()
    {
        var ps = GetComponent<ParticleSystem>();
        Destroy(gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
    }
}