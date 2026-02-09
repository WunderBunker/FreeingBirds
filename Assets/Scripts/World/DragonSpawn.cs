using System;
using UnityEngine;

public class DragonSpawn : MonoBehaviour
{
    [NonSerialized] Vector2 _minMaxSpeed = new Vector2(10, 25);
    [SerializeField] float _fallSpeed = 20;
    [SerializeField] float _flappMaxSpeed = 40;
    [SerializeField] float _flappDrag = 0.7f;
    [NonSerialized] public Vector2 Direction;
    [SerializeField] float _lifetime = 5;
    [SerializeField] GameObject _explosionParticle;

    public float CoefAvancement;
    public float _distanceToWallUp;
    public float _distanceToWallDown;

    float _speed;

    float _flappPeriodInit;
    float _flappPeriod;
    float _flappAmplitude;
    bool _isFlapping;
    float _flappStartTime;

    Vector3 _lastPosition;

    System.Random _ranPeriode = new System.Random();
    Vector2 _orthoDirect;


    // Start is called before the first frame update
    void Start()
    {
        _flappStartTime = Time.time;
        _lastPosition = transform.position;
        _flappAmplitude = GetAmplitudeFlapp();
        _flappPeriodInit = GetDureeFlapp();
        if (Direction.magnitude <= 0.1f) Destroy(gameObject);
        _orthoDirect = new Vector2(-Direction.y, Direction.x);

        transform.eulerAngles = new Vector3(0, 0, Vector2.SignedAngle(Vector2.right, Direction));

        _speed = Mathf.Lerp(_minMaxSpeed[0], _minMaxSpeed[1], CoefAvancement);
    }

    // Update is called once per frame
    void Update()
    {
        _lifetime -= Time.deltaTime;
        if (_lifetime <= 0) Destroy(gameObject);

        Move();

        if ((Time.time - _flappStartTime >= _flappPeriod && _distanceToWallUp > _flappAmplitude + 2) || _distanceToWallDown < 3)
            Flapp();

        float vDeltaOrtho = Vector2.Dot((Vector2)transform.position - (Vector2)_lastPosition, _orthoDirect.normalized);
        _distanceToWallUp -= vDeltaOrtho;
        _distanceToWallDown += vDeltaOrtho;

        _lastPosition = transform.position;
    }

    void Move()
    {
        Vector2 vDeltaPosition = Vector3.zero;

        vDeltaPosition += _speed * Direction * Time.deltaTime;

        //On applique la vitesse de chutte dans la direction orthogonale à l'orientation 
        vDeltaPosition -= _orthoDirect * _fallSpeed * Time.deltaTime;

        //Si on est en train de flapper on applique la force
        if (_isFlapping)
        {
            //On calcul la vitesse pour cett frame en fonction du temps et du drag
            float vTrueSpeed = Mathf.Lerp(_flappMaxSpeed, 0, (Time.time - _flappStartTime) / _flappDrag);
            vDeltaPosition += _orthoDirect * vTrueSpeed * Time.deltaTime;
            if (vTrueSpeed <= 0f) _isFlapping = false;
        }

        transform.position += (Vector3)vDeltaPosition;
    }

    void Flapp()
    {
        _isFlapping = true;
        _flappStartTime = Time.time;
        int vMinFlappPeriode = Mathf.RoundToInt(_flappPeriodInit * 10 / 2);
        int vMaxFlappPeriode = Mathf.RoundToInt(_flappPeriodInit * 10 * 2);
        _flappPeriod = (float)_ranPeriode.Next(vMinFlappPeriode, vMaxFlappPeriode) / 10;
    }


    float GetAmplitudeFlapp()
    {
        return FonctionFlappTheorique(_flappDrag * (1 - _fallSpeed / _flappMaxSpeed));
    }

    float FonctionFlappTheorique(float t)
    {
        return _flappMaxSpeed * (t - t * t / (2 * _flappDrag)) - _fallSpeed * t;
    }

    float GetDureeFlapp()
    {
        return 2 * _flappDrag * (_flappMaxSpeed - _fallSpeed) / _flappMaxSpeed;
    }


    public void DestroyDragon()
    {
        Instantiate(_explosionParticle, transform.parent)
            .transform.position = transform.position;
        Destroy(gameObject);
    }
}
