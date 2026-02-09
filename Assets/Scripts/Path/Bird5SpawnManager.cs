using System.Collections.Generic;
using UnityEngine;

public class Bird5SpawnManager : MonoBehaviour
{
    [SerializeField] float _spawnPeriod = 1.2f;

    [SerializeField] GameObject _dragonSpawn;

    [SerializeField] float _horizonAvancementForMaxSpeed = 5000;

    [SerializeField] int[] _valuesForDragon = { 40, 100 };

    [SerializeField] float _dragonsSpeed = 20;
    float _dragonsSpeedInit;
    PathScript _pathScript;
    float _scoreNeededToDoubleSpeed;
    PlayerControl _playerControl;
    Vector3 _dragonsDirection;

    public bool _canSpawn = false;
    float _lastSpawnTimer;

    Transform _dragonsParent;

    float _distancePathToWall;
    private Vector3 _startPosition;

    List<GameObject> _dragonList = new();


    // Start is called before the first frame update
    void Awake()
    {
        _dragonsParent = GameObject.Find("MeanTubes").transform;
        _dragonsSpeedInit = _dragonsSpeed;
        _pathScript = GameObject.FindGameObjectWithTag("Path").GetComponent<PathScript>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!_canSpawn || PartieManager.Instance._partieState != PartieState.PartieStarted) return;

        _lastSpawnTimer -= Time.deltaTime;

        //Si notre position par rapport au mur le permet et que la c'est le bon moment alors on spaw un bird
        if (_lastSpawnTimer <= 0)
        {
            SpawnRandomDragons();
            _lastSpawnTimer = _spawnPeriod;
        }
    }

    public void InitializeBasicData(float pDistanceToWall)
    {
        _canSpawn = true;
        _distancePathToWall = pDistanceToWall;

        _playerControl = GameObject.FindGameObjectWithTag("PlayerShell").GetComponentInChildren<PlayerControl>();

        _dragonsDirection = -_playerControl._directionOnPath;
        _scoreNeededToDoubleSpeed = _playerControl.ScoreNeededFromMaxSpeed;
    }

    private void SpawnRandomDragons()
    {
        float vAvancementProportionnel = PartieManager.Instance._avancement / _horizonAvancementForMaxSpeed;

        int vRandomValue;
        float vValueForBird1 = Mathf.Lerp(_valuesForDragon[0], _valuesForDragon[1], vAvancementProportionnel);
        vRandomValue = new System.Random().Next(1, 100);

        if (vRandomValue <= vValueForBird1)
        {
            //On initialise sa position 
            float vEcartFromPath = new System.Random().Next(-Mathf.RoundToInt(_distancePathToWall), Mathf.RoundToInt(_distancePathToWall));

            Camera vCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

            //Si le prochain noeud est dehors de la caméra alors le prend comme position de départ
            Vector2 vNextNodeFromCam = vCamera.WorldToViewportPoint(_playerControl._nextPosition);
            if (vNextNodeFromCam.x < 0f || vNextNodeFromCam.x > 1f || vNextNodeFromCam.y < 0f || vNextNodeFromCam.y > 1f || ((_playerControl._currentPathIndex + 1) >= _pathScript.NodeList.Count))
                _startPosition = _playerControl._nextPosition;
            //sinon on prend le suivant, en adaptant la direction
            else
            {
                _startPosition = _pathScript.NodeList[_playerControl._currentPathIndex + 1];
                _dragonsDirection = (_playerControl._nextPosition - _startPosition).normalized;
            }

            Vector3 vBirdPosition = _startPosition + new Vector3(-_dragonsDirection.y, _dragonsDirection.x) * vEcartFromPath;

            //On instancie l'oiseau
            GameObject vNewDragon = Instantiate(_dragonSpawn, vBirdPosition, Quaternion.identity);
            vNewDragon.transform.SetParent(_dragonsParent);

            //On initialise l'info sur les distances de l'oiseau par rapport aux murs
            vNewDragon.GetComponent<DragonSpawn>()._distanceToWallUp = _distancePathToWall - vEcartFromPath;
            vNewDragon.GetComponent<DragonSpawn>()._distanceToWallDown = _distancePathToWall + vEcartFromPath;

            //On lui donne sa vitesse, tirée de la formule appliquée à la caméra pour les autres modes de jeux
            if (PartieManager.Instance._avancement > 0)
                _dragonsSpeed = _dragonsSpeedInit * (1 + Mathf.Pow(PartieManager.Instance._avancement / _scoreNeededToDoubleSpeed, 0.5f));

            vNewDragon.GetComponent<DragonSpawn>().Direction = _dragonsDirection;
            vNewDragon.GetComponent<DragonSpawn>().CoefAvancement = vAvancementProportionnel;

            _dragonList.Add(vNewDragon);
        }
    }

    void OnDisable()
    {
        foreach (var lDragon in _dragonList)
            if (lDragon != null)
                lDragon.GetComponent<DragonSpawn>().DestroyDragon();
        _dragonList.Clear();
    }


#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(_startPosition, 1);
    }

#endif
}
