using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
public class EnemyAnimatorDriver : MonoBehaviour
{
    Animator _anim;
    NavMeshAgent _agent;

    static readonly int _hashSpeed = Animator.StringToHash("Speed");

    void Start()
    {
        _anim  = GetComponent<Animator>();
        _agent = GetComponentInParent<NavMeshAgent>();
    }

    void Update()
    {
        if (_anim == null || _agent == null) return;
        float vel = _agent.velocity.magnitude;
        _anim.SetFloat(_hashSpeed, vel);
        // Scale animation playback speed so enemy doesn't glide when stopped
        _anim.speed = vel > 0.1f ? 1f : 0f;
    }
}
