using Unity.Mathematics;
using UnityEngine;

public class MeanTubeMoving : MonoBehaviour
{
    [SerializeField] Vector2 _speedMinMax = new Vector2(15, 30);
    public float _speed { get; private set; }
    public float _maxMovingUp;

    public Vector3 _positionInit;

    public bool _isMovingUp = true;
    private float _tubeSize;

    void Awake()
    {
        _tubeSize = GetComponent<BoxCollider2D>().size.y * transform.localScale.y;
    }

    void Update()
    {
        if (_isMovingUp)
        {
            if (Vector2.Distance(transform.position, _positionInit) >= _maxMovingUp) _isMovingUp = false;
            else transform.position += Quaternion.AngleAxis(transform.localEulerAngles.z, new Vector3(0, 0, 1)) * Vector3.up * _speed * Time.deltaTime;
        }
        else
        {
            if (Vector2.Distance(transform.position, _positionInit) <= 0.2f) _isMovingUp = true;
            else transform.position -= Quaternion.AngleAxis(transform.localEulerAngles.z, new Vector3(0, 0, 1)) * Vector3.up * _speed * Time.deltaTime;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerShell")) collision.GetComponent<PlayerShellscript>().Die();
    }

    public void InitSpeed(float pAvancement)
    {
        _speed = math.lerp(_speedMinMax.x, _speedMinMax.y, pAvancement);
        float vCoef = _maxMovingUp / _tubeSize;
        _speed = math.lerp(0, _speed, vCoef);
    }

}
