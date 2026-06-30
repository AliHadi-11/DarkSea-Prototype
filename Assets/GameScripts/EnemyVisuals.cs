using UnityEngine;
using UnityEngine.AI;

// Procedural walk/idle animation for the low-poly enemy model.
// Attach to the "EnemyModel" child of the enemy root.
// Expects pivot child objects named: LArmPivot, RArmPivot, LLegPivot, RLegPivot, Head.
public class EnemyVisuals : MonoBehaviour
{
    Transform _lArmPivot, _rArmPivot, _lLegPivot, _rLegPivot, _head;
    NavMeshAgent _agent;
    float _walkCycle;
    float _baseHeadY;

    void Start()
    {
        _agent     = GetComponentInParent<NavMeshAgent>();
        _lArmPivot = transform.Find("LArmPivot");
        _rArmPivot = transform.Find("RArmPivot");
        _lLegPivot = transform.Find("LLegPivot");
        _rLegPivot = transform.Find("RLegPivot");
        _head      = transform.Find("Head");
        if (_head != null) _baseHeadY = _head.localPosition.y;
    }

    void Update()
    {
        float speed  = (_agent != null && _agent.isActiveAndEnabled) ? _agent.velocity.magnitude : 0f;
        bool  moving = speed > 0.2f;

        _walkCycle += Time.deltaTime * (moving ? speed * 2.8f : 1.2f);

        float swing = moving
            ? Mathf.Sin(_walkCycle) * 40f
            : Mathf.Sin(_walkCycle * 0.5f) * 5f; // idle sway

        if (_lArmPivot) _lArmPivot.localRotation = Quaternion.Euler( swing, 0f, 0f);
        if (_rArmPivot) _rArmPivot.localRotation = Quaternion.Euler(-swing, 0f, 0f);
        if (_lLegPivot) _lLegPivot.localRotation = Quaternion.Euler(-swing * (moving ? 0.85f : 0.3f), 0f, 0f);
        if (_rLegPivot) _rLegPivot.localRotation = Quaternion.Euler( swing * (moving ? 0.85f : 0.3f), 0f, 0f);

        if (_head != null)
        {
            float bob = moving ? Mathf.Abs(Mathf.Sin(_walkCycle)) * 0.05f : 0f;
            _head.localPosition = new Vector3(0f, _baseHeadY + bob, 0f);
        }
    }
}
