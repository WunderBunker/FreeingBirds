using Random = System.Random;
using UnityEngine;
using Unity.Mathematics;

public class MeanTubeFlying : MonoBehaviour
{
    [SerializeField] int _maxSpeed = 20;
    [SerializeField] int _minSpeed = 5;
    [SerializeField] float _playerDistanceToStartFlying = 40;
    [SerializeField] bool _hasSharpnels;
    [SerializeField] GameObject _destructionParticles;
    [SerializeField] AudioClip _breakingSound;

    public float _speed { get; private set; }

    Transform _playerTransform;

    bool _isFlying = false;
    float _startFlyingTime;

    Random _ranDistance = new Random();

    // Start is called before the first frame update
    void Awake()
    {
        _playerTransform = GameObject.FindGameObjectWithTag("PlayerShell").transform;

        _playerDistanceToStartFlying = _ranDistance.Next(Mathf.RoundToInt(_playerDistanceToStartFlying / 2), Mathf.RoundToInt(_playerDistanceToStartFlying * 2));
    }

    void Update()
    {
        if (!_isFlying && Vector2.Distance(_playerTransform.position, transform.position) <= _playerDistanceToStartFlying)
        {
            _isFlying = true;
            _startFlyingTime = Time.time;
        }
        if (_isFlying) transform.position += Quaternion.AngleAxis(transform.localEulerAngles.z, new Vector3(0, 0, 1)) * Vector3.up
            * _speed * Time.deltaTime;
    }

    public void InitSpeed(float pAvancement)
    {
        _speed = math.lerp(_minSpeed, _maxSpeed, pAvancement);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerShell")) collision.GetComponent<PlayerShellscript>().Die();
        else if (_isFlying && (collision.CompareTag("PolyA") || collision.CompareTag("PolyB")) && (Time.time - _startFlyingTime > 1))
        {
            if (_hasSharpnels)
            {
                ActivateSharpnels();
                return;
            }
            Instantiate(_destructionParticles, transform.position, Quaternion.identity, transform.parent);
            //To-do : revoir où ets le listener et peut être le mettre sur le player et adapter les volumes
            AudioManager.Instance.PlaySound(_breakingSound, 0.2f, transform.position);
            Destroy(gameObject);
        }
    }

    void ActivateSharpnels()
    {
        Instantiate(_destructionParticles, transform.position, Quaternion.identity, transform.parent);
        GetComponent<BoxCollider2D>().enabled = false;
        GetComponent<SpriteRenderer>().enabled = false;
        for (int lCptChildren = 0; lCptChildren < transform.childCount; lCptChildren++)
            transform.GetChild(lCptChildren).gameObject.SetActive(true);
        this.enabled = false;
    }
}
