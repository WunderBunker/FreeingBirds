using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = System.Random;

public class PlayerControl : MonoBehaviour
{
    //Objet chargé de générer le chemin procédural à suivre par le player
    [SerializeField][Range(0, 50)] public float _speed;
    [SerializeField] float _fallSpeed;
    [SerializeField] float _flappMaxSpeed;
    [SerializeField] float _directionChangeSpeed = 2;

    [SerializeField] float _flappAttenuationTime;
    [SerializeField] float _flappTimeBfBoost = 0.2f;
    [SerializeField] float _boostMaxTime = 0.5f;
    [SerializeField] List<AudioClip> _flapNoisesList;
    [SerializeField] float _cameraTiltPace = 10f;

    [SerializeField] public float ScoreNeededToDoubleSpeed;

    public Vector2 PlayerSize;
    public FlapTrail FlapTrail;

    public Vector2 _directionOnPath { get; private set; } = Vector2.zero;

    //Prochain noeud du chemin où le player doit se rendre
    public Vector3 _nextPosition { get; private set; }
    //Indice du prochain noeud dans la liste du PathScript
    public int _currentPathIndex { get; private set; }

    PathScript _pathScript;

    private float _initZ;
    float _initFlappAttenuationDuration;
    private float _initSpeed;

    Vector3 _targetDirectionOnPath;
    protected Vector3 _lastPosition;

    Transform _playerShellTransform;

    PlayerShellscript _shell;

    bool _isFlapping;
    float _flappStartTime;
    float _attenuationStartTime;
    bool _isAskingForBoostingFlapp;
    bool _isBoostingFlapp;

    GameObject __camera;
    GameObject _camera
    {
        get
        {
            if (__camera == null) __camera = GameObject.FindGameObjectWithTag("MainCamera");
            return __camera;
        }
    }

    private void Awake()
    {
        _pathScript = GameObject.FindGameObjectWithTag("Path").GetComponent<PathScript>();

        _shell = transform.parent.GetComponent<PlayerShellscript>();
        _initFlappAttenuationDuration = _flappAttenuationTime;

        _playerShellTransform = transform.parent;
        _lastPosition = _playerShellTransform.position;
        _nextPosition = _lastPosition;
        _currentPathIndex = 0;

        PlayerSize = GetComponent<BoxCollider2D>().size * transform.localScale;

        FlapTrail = transform.Find("FlapTrail").GetComponent<FlapTrail>();
    }

    // Start is called before the first frame update
    void Start()
    {
        _playerShellTransform.position = _camera.transform.position;
        _playerShellTransform.position = new Vector3(_playerShellTransform.position.x, _playerShellTransform.position.y,
            _playerShellTransform.position.z + _camera.GetComponent<Camera>().nearClipPlane + 2); //on place le player devant la camera
        _initZ = _playerShellTransform.position.z;
        _initSpeed = SaveManager.SafeSave.SelectedBirdId != "Bird4" ? _speed : 0;
    }

    //Initialisation de la position du player sur le chemin, à la position d'un noeud du chemin dont on spécifie l'index 
    public void InitiatePlayerOnPath(int pFirstIndex)
    {
        _currentPathIndex = pFirstIndex + 1;

        _nextPosition = new Vector3(_pathScript.NodeList[pFirstIndex + 1].x, _pathScript.NodeList[pFirstIndex + 1].y, _initZ);
        _lastPosition = new Vector3(_pathScript.NodeList[pFirstIndex].x, _pathScript.NodeList[pFirstIndex].y, _initZ);
        _playerShellTransform.position = _lastPosition;

        _directionOnPath = (_nextPosition - _lastPosition).normalized;
        _targetDirectionOnPath = _directionOnPath;

        _camera.GetComponent<CamFollowPath>().InitCam(new Vector3(_lastPosition.x, _lastPosition.y, 0));
    }

    private void Update()
    {
        if (PartieManager.Instance._partieState != PartieState.PartieStarted) return;

        ActualizePath();
        UpdateSpeed();
        if (PartieManager.Instance.ModeDebug.Contains(DebugModes.MoveAlone))
            PlayerMoveDebug();
        else
            PlayerMove();
    }

    //On actualise (ou non) la direction que le player doit suivre, donc le prochain noeud du chemin vers lequelle elle doit aller
    void ActualizePath()
    {
        Vector3 vPathToFollow = _nextPosition - _lastPosition;
        float vDistanceRestante = (vPathToFollow != Vector3.zero) ? (vPathToFollow.magnitude - Vector3.Project(_playerShellTransform.position - _lastPosition, vPathToFollow).magnitude) : 0;
        if (vDistanceRestante < 1)
        {
            _lastPosition = _nextPosition;
            _currentPathIndex = _currentPathIndex + 1;
            _nextPosition = new Vector3(_pathScript.NodeList[_currentPathIndex].x, _pathScript.NodeList[_currentPathIndex].y, _initZ);

            _targetDirectionOnPath = (_nextPosition - _lastPosition).normalized;
        }

        _directionOnPath = Vector3.MoveTowards(_directionOnPath, _targetDirectionOnPath, _directionChangeSpeed * Time.deltaTime);
    }

    private void UpdateSpeed()
    {
        //TO-DO : remplacer par un lerp propre avec une vitesse max et min définies
        _speed = _initSpeed * (1 + Mathf.Pow(PartieManager.Instance._avancement / ScoreNeededToDoubleSpeed, 0.5f));
    }

    private void PlayerMove()
    {
        Vector2 vDeltaPosition = Vector3.zero;
        float vDeltaTime = Time.deltaTime;

        //On applique la vitesse de chutte dans la direction orthogonale à l'orientation 
        Vector2 vOrthoDirection = new Vector2(-_directionOnPath.y, _directionOnPath.x).normalized;
        vDeltaPosition -= vOrthoDirection * _fallSpeed * vDeltaTime;

        //Si on est en train de flapper on applique la force
        if (_isFlapping)
        {
            if (PartieManager.Instance.ModeDebug.Contains(DebugModes.PrintFlappInfo)) PrintFlappInfos();

            //On calcul la vitesse pour cette frame en fonction du temps, du boost et du drag
            if (_isAskingForBoostingFlapp && (Time.time - _flappStartTime >= _flappTimeBfBoost)) StartBoostingFlapp();
            if (_isBoostingFlapp && Time.time - _flappStartTime >= _boostMaxTime) StopBoostingFlapp();

            float vTrueSpeed = _isBoostingFlapp || (Time.time - _flappStartTime < _flappTimeBfBoost) ? _flappMaxSpeed
            : Mathf.Lerp(_flappMaxSpeed, 0, (Time.time - _attenuationStartTime) / _flappAttenuationTime);

            vDeltaPosition += vOrthoDirection * vTrueSpeed * vDeltaTime;
            if (vTrueSpeed <= 0f) { _isFlapping = false; _flappAttenuationTime = _initFlappAttenuationDuration; }
        }

        //Bird Specific 
        PlayerSave vSave = SaveManager.SafeSave;
        if (vSave.SelectedBirdId != "Bird4")
        {
            //On ajoute l'avancée sur le chemin
            vDeltaPosition += _directionOnPath * _speed * vDeltaTime;

            if (vSave.SelectedBirdId != "Bird3")
            {
                float vDeltaOnPath = Vector3.Project(vDeltaPosition, _directionOnPath).magnitude;
                PartieManager.Instance.AddToAvancement(vDeltaOnPath);
            }
        }
        _playerShellTransform.position += (Vector3)vDeltaPosition;
        //Maj de l'orientation du player
        _playerShellTransform.eulerAngles = new Vector3(0, 0, Vector2.SignedAngle(Vector2.right, _directionOnPath));
    }

    public void StartFlapp()
    {
        _isFlapping = true;
        _isAskingForBoostingFlapp = true;
        _flappStartTime = Time.time;
        _attenuationStartTime = _flappStartTime + _flappTimeBfBoost;
        _flappAttenuationTime = _initFlappAttenuationDuration;

        Random vRanNoise = new Random();
        AudioManager.Instance.PlaySound(_flapNoisesList[vRanNoise.Next(0, _flapNoisesList.Count - 1)], 1f, null, true);

        gameObject.GetComponent<ParticleSystem>().Play();
        gameObject.GetComponent<Animator>().SetTrigger("StartFlapping");

        FlapTrail.StartTrailing();

        _playerShellTransform.position += Quaternion.AngleAxis(_playerShellTransform.localEulerAngles.z + 90, new Vector3(0, 0, 1)) * Vector3.right * _flappMaxSpeed * Time.deltaTime;

        if (SaveManager.SafeSave.SelectedBirdId == "Bird2")
        {
            StartCoroutine(_camera.GetComponent<CamFollowPath>().TiltCamera(_cameraTiltPace));
        }
    }

    void StartBoostingFlapp()
    {
        _isBoostingFlapp = true;
        _isAskingForBoostingFlapp = false;
        gameObject.GetComponent<Animator>().speed = 0;
    }

    public void StopBoostingFlapp()
    {
        _isAskingForBoostingFlapp = false;
        if (_isBoostingFlapp)
        {
            _isBoostingFlapp = false;
            _attenuationStartTime = Time.time;
            gameObject.GetComponent<Animator>().speed = 1;
            gameObject.GetComponent<Animator>().SetTrigger("StartFalling");
        }
    }

    void PlayerMoveDebug()
    {
        Vector2 vDirectionOnPath = _directionOnPath;
        Vector3 vDeltaPosition = Vector3.zero;
        float vDeltaTime = Time.deltaTime;

        //On ajoute l'avancée sur le chemin
        if (SaveManager.SafeSave.SelectedBirdId != "Bird4") vDeltaPosition += (Vector3)vDirectionOnPath * _speed * vDeltaTime;

        _playerShellTransform.position += vDeltaPosition;

        float vDeltaOnPath = Vector3.Project(vDeltaPosition, _directionOnPath).magnitude;
        PartieManager.Instance.AddToAvancement(vDeltaOnPath);
    }

    private void PrintFlappInfos()
    {
        GameObject.FindGameObjectWithTag("DebugCanvas").transform.GetComponent<TextMeshProUGUI>().text
            = "IsAskingForBoostingFlapp : " + _isAskingForBoostingFlapp.ToString()
            + "\r\n" + "IsBoostingFlapp : " + _isBoostingFlapp.ToString()
            + "\r\n" + "Time.time - gFlappStartTime : " + (Time.time - _flappStartTime).ToString()
            + "\r\n" + "gAttenuationStartTime - gFlappStartTime : " + (_attenuationStartTime - _flappStartTime).ToString();
    }


    //Décale l'indice du noeud vers lequel le player se dirige d'une valeur pShift (permet de suivre les suppressions de noeuds)
    public void RemapIndex(int pIndexShift)
    {
        _currentPathIndex -= pIndexShift;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.CompareTag("PolyA") || collision.CompareTag("PolyB"))
        {
            StartCoroutine(_camera.GetComponent<CamFollowPath>().Tremour());
            _shell.Die();

        }
        else if (collision.CompareTag("MeanTube"))
        {
            StartCoroutine(_camera.GetComponent<CamFollowPath>().Tremour());
            if (gameObject.GetComponent<PlayerShield>() is { } vShield)
            {
                if (vShield._shieldsCount <= 0) _shell.Die();
                else vShield.LooseShield();

            }
            else _shell.Die();
        }
    }

    public void ApplyOtherControlParameter(PlayerControl pControl)
    {
        _speed = pControl._speed;
        _attenuationStartTime = pControl._attenuationStartTime;
        _fallSpeed = pControl._fallSpeed;
        _flappMaxSpeed = pControl._flappMaxSpeed;
        _flappAttenuationTime = pControl._flappAttenuationTime;
        _flappTimeBfBoost = pControl._flappTimeBfBoost;
        _boostMaxTime = pControl._boostMaxTime;
        _cameraTiltPace = pControl._cameraTiltPace;
    }
}
