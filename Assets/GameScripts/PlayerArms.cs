using UnityEngine;

// First-person arm bob. Attach to a child of Main Camera.
// Expects children named "LeftArm" and "RightArm".
public class PlayerArms : MonoBehaviour
{
    public float bobSpeed    = 6f;
    public float bobAmount   = 0.032f;
    public float returnSpeed = 8f;

    Transform _leftArm, _rightArm;
    Vector3   _lBase, _rBase;
    float     _timer;

    void Start()
    {
        _leftArm  = transform.Find("LeftArm");
        _rightArm = transform.Find("RightArm");
        if (_leftArm)  _lBase = _leftArm.localPosition;
        if (_rightArm) _rBase = _rightArm.localPosition;
    }

    void Update()
    {
        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");
        bool moving = Mathf.Abs(v) > 0.05f || Mathf.Abs(h) > 0.05f;

        if (moving)
        {
            _timer += Time.deltaTime * bobSpeed;
            float vert  =  Mathf.Sin(_timer)        * bobAmount;
            float horiz =  Mathf.Cos(_timer * 0.5f) * bobAmount * 0.45f;

            if (_leftArm)  _leftArm.localPosition  = _lBase + new Vector3( horiz,  vert,         0f);
            if (_rightArm) _rightArm.localPosition = _rBase + new Vector3(-horiz, -vert * 0.55f, 0f);
        }
        else
        {
            _timer = Mathf.Lerp(_timer, 0f, Time.deltaTime * returnSpeed);
            float idle = Mathf.Sin(Time.time * 1.4f) * 0.006f;
            if (_leftArm)
                _leftArm.localPosition  = Vector3.Lerp(_leftArm.localPosition,  _lBase + Vector3.up * idle,  Time.deltaTime * returnSpeed);
            if (_rightArm)
                _rightArm.localPosition = Vector3.Lerp(_rightArm.localPosition, _rBase + Vector3.up * idle, Time.deltaTime * returnSpeed);
        }
    }
}
