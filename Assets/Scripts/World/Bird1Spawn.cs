using System;
using UnityEngine;
using System.Collections;
using Random = System.Random;

public class Bird1Spawn : MonoBehaviour
{
    [NonSerialized] public float _speed = 20;
    [SerializeField] float _fallSpeed = 20;
    [SerializeField] float _flappMaxSpeed = 40;
    [SerializeField] float _flappDrag = 0.7f;
    [SerializeField] public int _advancementToGain = 100;
    [NonSerialized] public Vector2 _direction;
    [SerializeField] GameObject _explosionPaticles;
    [SerializeField] AudioClip _deathSound;

    public float _distanceToWallUp;
    public float _distanceToWallDown;

    float _flappPeriodInit;
    float _flappPeriod;
    float _flappAmplitude;
    bool _isFlapping;
    float _flappStartTime;

    Vector3 _lastPosition;

    Random _ranPeriode = new Random();


    // Start is called before the first frame update
    void Start()
    {
        _flappStartTime = Time.time;
        _lastPosition = transform.position;
        _flappAmplitude = GetAmplitudeFlapp();
        _flappPeriodInit = GetDureeFlapp();
    }

    // Update is called once per frame
    void Update()
    {
        Move();

        if ((Time.time - _flappStartTime >= _flappPeriod && _distanceToWallUp > _flappAmplitude + 2) || _distanceToWallDown < 3)
            Flapp();

        _distanceToWallUp -= (transform.position - _lastPosition).y;
        _distanceToWallDown += (transform.position - _lastPosition).y;

        _lastPosition = transform.position;
    }

    void Move()
    {
        Vector3 vDeltaPosition = Vector3.zero;

        vDeltaPosition += _speed * (Vector3)_direction * Time.deltaTime;

        //On applique la vitesse de chutte dans la direction orthogonale à l'orientation 
        vDeltaPosition -= new Vector3(-_direction.y, _direction.x, 0) * _fallSpeed * Time.deltaTime;

        //Si on est en train de flapper on applique la force
        if (_isFlapping)
        {
            //On calcul la vitesse pour cett frame en fonction du temps et du drag
            float vTrueSpeed = Mathf.Lerp(_flappMaxSpeed, 0, (Time.time - _flappStartTime) / _flappDrag);
            vDeltaPosition += Quaternion.AngleAxis(transform.localEulerAngles.z + 90, new Vector3(0, 0, 1)) * Vector3.right * vTrueSpeed * Time.deltaTime;
            if (vTrueSpeed <= 0f) _isFlapping = false;
        }

        transform.position += vDeltaPosition;
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

    public void GetCaptured()
    {
        Instantiate(_explosionPaticles, transform.position, Quaternion.identity, transform.parent).GetComponent<ParticleSystem>();
        AudioManager.Instance.PlaySound(_deathSound, 1, transform.position);
        Destroy(gameObject);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("BigCage"))
        {
            collision.gameObject.GetComponent<BigCage>().LoosePoints(1);
            StartCoroutine(DieAfterAWhile());
        }
    }


    protected IEnumerator DieAfterAWhile()
    {
        yield return new WaitForSeconds(2);
        Destroy(gameObject);
    }

}
