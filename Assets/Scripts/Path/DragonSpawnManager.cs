using UnityEngine;
using Random = System.Random;

public class DragonSpawnManager : MonoBehaviour
{
    [SerializeField] Vector2 _minMaxSpawnPeriod = new Vector2(1.5f, 0.5f);
    [SerializeField] Vector2 _minsMaxSpeed = new Vector2(15, 30);
    [SerializeField] int[] _valuesForBirds = { 40, 100 };
    [SerializeField] AnimationCurve _avancementCurve;

    [SerializeField] GameObject _bird1Spawn;

    float _horizonAvancementForMaxSpeed;
    float _avancementCoeff;

    Vector3 _birdsDirection;
    Vector3 _basicBirdPosition;
    float _distancePathToWall;

    float _spawnTimer;

    Transform _birdsParent;


    // Start is called before the first frame update
    void Awake()
    {
        //on met les spawns dans meantubes pour qu'il soient correctement supprimés à la fin
        _birdsParent = GameObject.Find("MeanTubes").transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (PartieManager.Instance._partieState != PartieState.PartieStarted) return;

        _spawnTimer -= Time.deltaTime;

        //Si notre position par rapport au mur le permet et que la c'est le bon moment alors on spaw un bird
        if (_spawnTimer <= 0)
        {
            _avancementCoeff = _avancementCurve.Evaluate(PartieManager.Instance._avancement / _horizonAvancementForMaxSpeed);
            
            SpawnRandomBird();
            _spawnTimer = Mathf.Lerp(_minMaxSpawnPeriod[0], _minMaxSpawnPeriod[1], _avancementCoeff);
        }
    }

    public void InitializeBasicData(float pDistanceToWall)
    {
        _distancePathToWall = pDistanceToWall;

        GameObject vPlayerShell = GameObject.FindGameObjectWithTag("PlayerShell");
        Camera vCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

        _birdsDirection = vPlayerShell.GetComponentInChildren<PlayerControl>()._directionOnPath;
        _basicBirdPosition = vCamera.ViewportToWorldPoint(new Vector3(-0.5f, 0.5f, vCamera.nearClipPlane + 1));

        _horizonAvancementForMaxSpeed = GetComponent<SpawnManager>()._horizonAvancementForMaxSpawn;
    }

    private void SpawnRandomBird()
    {
        int vRandomValue;
        float vValueForBird1 = Mathf.Lerp(_valuesForBirds[0], _valuesForBirds[1], _avancementCoeff);
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

            vNewBird.GetComponent<Bird1Spawn>()._speed = Mathf.Lerp(_minsMaxSpeed[0], _minsMaxSpeed[1], _avancementCoeff);
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
