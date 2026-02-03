using UnityEngine;
using Random = System.Random;

public class DragonSpawnManager : MonoBehaviour
{

    [SerializeField] float _spawnPeriod = 1.2f;

    [SerializeField] GameObject _bird1Spawn;

    [SerializeField] float _horizonAvancementForMaxSpeed = 5000;

    [SerializeField] int[] _valuesForBirds = { 40, 100 };

    [SerializeField] float _speed = 20;
    private float _birdsSpeedInit;
    private float _scoreNeededToDoubleSpeed;

    Vector3 _birdsDirection;

    public bool _canSpawn = false;
    float _lastSpawnTimer;

    Transform _birdsParent;

    Vector3 _basicBirdPosition;

    float _distancePathToWall;


    // Start is called before the first frame update
    void Awake()
    {
        //on met les spawns dans meantubes pour qu'il soient correctement supprimés à la fin
        _birdsParent = GameObject.Find("MeanTubes").transform;
        _birdsSpeedInit = _speed;
    }

    // Update is called once per frame
    void Update()
    {
        if (PartieManager.Instance._partieState != PartieState.PartieStarted) return;

        _lastSpawnTimer -= Time.deltaTime;

        //Si notre position par rapport au mur le permet et que la c'est le bon moment alors on spaw un bird
        if (_canSpawn && (_lastSpawnTimer <= 0))
        {
            SpawnRandomBird();
            _lastSpawnTimer = _spawnPeriod;
        }
    }

    public void InitializeBasicData(float pDistanceToWall)
    {
        _distancePathToWall = pDistanceToWall;

        GameObject vPlayerShell = GameObject.FindGameObjectWithTag("PlayerShell");
        Camera vCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

        _birdsDirection = vPlayerShell.GetComponentInChildren<PlayerControl>()._directionOnPath;

        _basicBirdPosition = vCamera.ViewportToWorldPoint(new Vector3(-0.5f, 0.5f, vCamera.nearClipPlane + 1));

        _scoreNeededToDoubleSpeed = vPlayerShell.GetComponentInChildren<PlayerControl>().ScoreNeededToDoubleSpeed;

        _canSpawn = true;
    }

    private void SpawnRandomBird()
    {
        float vAvancementProportionnel = PartieManager.Instance._avancement / _horizonAvancementForMaxSpeed;

        int vRandomValue;
        float vValueForBird1 = Mathf.Lerp(_valuesForBirds[0], _valuesForBirds[1], vAvancementProportionnel);
        vRandomValue = new Random().Next(1, 100);

        if (vRandomValue <= vValueForBird1)
        {
            //On initialise sa position 
            float vEcartFromPath = new Random().Next(-Mathf.RoundToInt(_distancePathToWall), Mathf.RoundToInt(_distancePathToWall));
            Vector3 vBirdPosition = _basicBirdPosition + Vector3.up * vEcartFromPath;

            //On instancie l'oiseau
            GameObject vNewBird = Instantiate(_bird1Spawn, vBirdPosition, Quaternion.identity);
            vNewBird.transform.SetParent(_birdsParent);

            //On initialise l'info sur les distances de l'oiseau par rapport aux murs
            vNewBird.GetComponent<Bird1Spawn>()._distanceToWallDown = _distancePathToWall + vEcartFromPath;
            vNewBird.GetComponent<Bird1Spawn>()._distanceToWallUp = _distancePathToWall - vEcartFromPath;

            //On lui donne sa vitesse, tirée de la formule appliquée à la caméra pour les autres modes de jeux
            if (PartieManager.Instance._avancement > 0)
                _speed = _birdsSpeedInit * (1 + Mathf.Pow(PartieManager.Instance._avancement / _scoreNeededToDoubleSpeed, 0.5f));

            vNewBird.GetComponent<Bird1Spawn>()._speed = _speed;
            vNewBird.GetComponent<Bird1Spawn>()._direction = _birdsDirection;
        }

    }

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        Gizmos.color = UnityEngine.Color.red;
        Gizmos.DrawSphere(_basicBirdPosition, 1);
    }

#endif
}
