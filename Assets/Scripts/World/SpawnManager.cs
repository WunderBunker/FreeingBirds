using Random = System.Random;
using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;
using System.Linq;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] public float _tubeDistanceLimitFromWall;
    [SerializeField] GameObject _meanTubePrefab;
    [SerializeField] GameObject _meanTubeMovingPrefab;
    [SerializeField] GameObject _meanTubeFlyinPrefab;
    [SerializeField] GameObject _meanTubeFlyingSharpnelPrefab;
    [SerializeField] GameObject _dollarBillPrefab;

    [SerializeField] public float _horizonAvancementForMaxSpawn;
    [SerializeField] float _maxDeltaBtwTubes;

    [SerializeField] public int[] _valuesForTube = { 30, 75 };
    [SerializeField] public int[] _valuesForMovingTube = { 1, 25 };
    [SerializeField] public int[] _valuesForFlyingTube = { 0, 0 };
    [SerializeField] public int[] _valuesForFlyingTubeSharpnel = { 0, 0 };
    [SerializeField] public int[] _valuesForDollarBill = { 0, 0 };

    [SerializeField] public float _tubeSpacing = 0;
    [SerializeField] public float _playerSizeCoeff = 1;

    Transform _meanTubesParent;
    Transform _itemsParent;
    private GameObject _playerShell;
    Vector2 _playerSize = new Vector2(1, 1);
    Vector2 _meanTubeSize = new Vector2(1, 10);

    string _currentBird;

    float _lastSizeTubeA = 0;
    List<Tools.OrientedRect> _listeTubeBoxB = new();
    float _lastSizeTubeB = 0;
    private float _avancement;

    void Awake()
    {
        _meanTubesParent = GameObject.Find("MeanTubes").transform;
        _itemsParent = GameObject.Find("Items").transform;
        PartieManager.Instance.GameDataReady += GetPlayerInfos;

        GameObject vMeanTube = Instantiate(_meanTubePrefab, transform.position, Quaternion.identity);
        _meanTubeSize = vMeanTube.GetComponent<BoxCollider2D>().size * vMeanTube.transform.localScale;
        Destroy(vMeanTube);
    }

    public void GetPlayerInfos()
    {
        _playerShell = GameObject.FindGameObjectWithTag("PlayerShell");
        _playerSize = _playerShell.GetComponentInChildren<PlayerControl>() != null ? _playerShell.GetComponentInChildren<PlayerControl>().PlayerSize : Vector2.zero;
        _playerSize *= _playerSizeCoeff;
        _currentBird = SaveManager.SafeSave.SelectedBirdId;
    }

    public void GererNewSpawnsOnSegment(Vector2[] pStartEndOnA, Vector2[] pStartEndOnB, float pDistancePathToWall)
    {
        if (_currentBird == "Bird4")
        {
            gameObject.GetComponent<DragonSpawnManager>().InitializeBasicData(pDistancePathToWall);
            return;
        }
        else if (_currentBird == "Bird5" && GetComponent<Bird5SpawnManager>().isActiveAndEnabled)
            GetComponent<Bird5SpawnManager>().InitializeBasicData(pDistancePathToWall);

        //Longueur du segment sur le mur A
        float vLgA = Vector2.Distance(pStartEndOnA[1], pStartEndOnA[0]);
        //Chaque élément représente une section du segment de la taille de la largeur du tube, valeur > 0 s'il faut spawn sur cette section
        int[] vTubeTypesOnA = new int[Mathf.RoundToInt(vLgA / (_meanTubeSize.x + _tubeSpacing))];
        //Idem que A
        float vLgB = Vector2.Distance(pStartEndOnB[1], pStartEndOnB[0]);
        int[] vTubeTypesOnB = new int[Mathf.RoundToInt(vLgB / (_meanTubeSize.x + _tubeSpacing))];

        //On prend comme référence le segment A pour délimiter les sections sur lesquelles faire spawn des items
        int[] vSpawnItems = new int[vTubeTypesOnA.Length];

        //On détermine ici de manière aléatoire quelles sections doivent spawner
        GetRandomTubes(ref vTubeTypesOnA);
        GetRandomTubes(ref vTubeTypesOnB);
        GetRandomItems(ref vSpawnItems);

        _tubeDistanceLimitFromWall = (_tubeDistanceLimitFromWall == 0) ? (pDistancePathToWall * 2) : _tubeDistanceLimitFromWall;

        //Très importants pour que les raycasts puissent fonctionner (détection des tubes en face)
        Physics2D.SyncTransforms();

        //Pour chacune d'elle on fait spawner un tube
        Vector2 vSection = (pStartEndOnA[1] - pStartEndOnA[0]).normalized * _meanTubeSize.x;
        Vector2 vSpacing = vSection.normalized * _tubeSpacing;
        for (int lCptSection = 0; lCptSection < vTubeTypesOnA.Length; lCptSection++)
            if (vTubeTypesOnA[lCptSection] > 0 || vSpawnItems[lCptSection] > 0)
            {
                Vector2 lStart = pStartEndOnA[0] + (vSection + vSpacing) * lCptSection + vSpacing;
                Vector2 lEnd = lStart + vSection;
                if (vTubeTypesOnA[lCptSection] > 0) SpawnMeanTubeOnWall(lStart, lEnd, _tubeDistanceLimitFromWall, "A", vTubeTypesOnA[lCptSection]);
                if (vSpawnItems[lCptSection] > 0) SpawnItemInSection(lStart, lEnd, pDistancePathToWall);
            }

        Physics2D.SyncTransforms();

        vSection = (pStartEndOnB[1] - pStartEndOnB[0]).normalized * _meanTubeSize.x;
        for (int lCptSection = 0; lCptSection < vTubeTypesOnB.Length; lCptSection++)
            if (vTubeTypesOnB[lCptSection] > 0)
            {
                Vector2 lStart = pStartEndOnB[0] + (vSection + vSpacing) * lCptSection + vSpacing;
                Vector2 lEnd = lStart + vSection;
                if (vTubeTypesOnB[lCptSection] > 0) SpawnMeanTubeOnWall(lStart, lEnd, _tubeDistanceLimitFromWall, "B", vTubeTypesOnB[lCptSection]);
            }

        if (_currentBird == "Bird5" && _playerShell.GetComponentInChildren<Human>() is { } lHumman && lHumman._mustSpawnSwitch)
            _playerShell.GetComponentInChildren<Human>().SpawnModeSwitch((pStartEndOnA[1] + pStartEndOnB[1]) / 2, (pStartEndOnA[1] - pStartEndOnB[1]).normalized);
    }

    private void GetRandomTubes(ref int[] pTubeTypes)
    {
        _avancement = PartieManager.Instance._avancement / _horizonAvancementForMaxSpawn;

        Random vRan = new Random();
        int vRandomValue;
        float vValueForTube = Mathf.Lerp(_valuesForTube[0], _valuesForTube[1], _avancement);
        float vValueFormMovingTube = Mathf.Lerp(_valuesForMovingTube[0], _valuesForMovingTube[1], _avancement);
        float vValueFormFlyingTube = Mathf.Lerp(_valuesForFlyingTube[0], _valuesForFlyingTube[1], _avancement); ;
        float vValueFormFlyingTubeSharpnel = Mathf.Lerp(_valuesForFlyingTubeSharpnel[0], _valuesForFlyingTubeSharpnel[1], _avancement); ;

        for (int lCptSection = 0; lCptSection < pTubeTypes.Length; lCptSection++)
        {
            vRandomValue = vRan.Next(1, 100);
            if (vRandomValue <= vValueFormFlyingTubeSharpnel) pTubeTypes[lCptSection] = 4;
            else if (vRandomValue <= vValueFormFlyingTube) pTubeTypes[lCptSection] = 3;
            else if (vRandomValue <= vValueFormMovingTube) pTubeTypes[lCptSection] = 2;
            else if (vRandomValue <= vValueForTube) pTubeTypes[lCptSection] = 1;
            else pTubeTypes[lCptSection] = 0;
        }
    }

    private void GetRandomItems(ref int[] pTypeItem)
    {
        float vAvancementProportionnel = PartieManager.Instance._avancement / _horizonAvancementForMaxSpawn;

        Random vRanItem = new Random();
        int vRandomValue;
        float vValueForDollarBill = Mathf.Lerp(_valuesForDollarBill[1], _valuesForDollarBill[0], vAvancementProportionnel);

        for (int lCptSectionInA = 0; lCptSectionInA < pTypeItem.Length; lCptSectionInA++)
        {
            vRandomValue = vRanItem.Next(1, 100);
            pTypeItem[lCptSectionInA] = vRandomValue <= vValueForDollarBill ? 1 : 0;
        }
    }

    //Spawn d'un tube sur un mur
    //pTypeOfTube - 1 : normal, 2: moving, 3: flying, 4: flying with sharpnel
    private void SpawnMeanTubeOnWall(Vector2 pWallSegmentStart, Vector2 pWallSegmentEnd, float pDistanceLimitFromWall, string pWallName, int pTypeOfTube)
    {
        float vLastSizeTube = pWallName == "A" ? _lastSizeTubeA : _lastSizeTubeB;
        Random vRanSize = new Random();
        Vector2 vDirection = (pWallSegmentEnd - pWallSegmentStart).normalized;
        Vector2 vPosition = pWallSegmentStart;
        float vMinSize = math.max(vLastSizeTube - _maxDeltaBtwTubes, pDistanceLimitFromWall / 10);
        float vMaxSize = GetTubeMaxSize(vLastSizeTube, pDistanceLimitFromWall, vPosition, vDirection, vMinSize, pWallName);

        //On détermine la longueur du tube de façon aléatoire entre un certain minimum et la taille max calculée précédemment (sous réserve que ça ne dépasse pas la taille totale du tuyau) 
        float vSize = vRanSize.Next(Mathf.FloorToInt(vMinSize), Mathf.FloorToInt(vMaxSize));

        //On maj la position en décalant la longueur souhaitée selon l'orthogonal au segment 
        //A : on décale négativement sur l'orthogonal B: on décale positivement sur l'orthogonal
        Vector2 vOrthoDirection = new Vector3(-vDirection.y, vDirection.x);
        vPosition += vOrthoDirection * vSize * (pWallName == "A" ? -1 : 1);

        //On instancie l'objet
        GameObject vMeanTube;
        switch (pTypeOfTube)
        {
            case 4:
                vMeanTube = Instantiate(_meanTubeFlyingSharpnelPrefab, transform.position, Quaternion.identity);
                vMeanTube.GetComponent<MeanTubeFlying>().InitSpeed(_avancement);
                break;
            case 3:
                vMeanTube = Instantiate(_meanTubeFlyinPrefab, transform.position, Quaternion.identity);
                vMeanTube.GetComponent<MeanTubeFlying>().InitSpeed(_avancement);
                break;
            case 2:
                vMeanTube = Instantiate(_meanTubeMovingPrefab, transform.position, Quaternion.identity);

                //Si la taille prévue est inférieure à la taille max alors on autorise à bouger jusqu'à elle
                if (vSize < vMaxSize) vMeanTube.GetComponent<MeanTubeMoving>()._maxMovingUp = vMaxSize - vSize;
                //Sinon on réduit la taille prévue de façon à ce que le mouvemnet autorisé puisse couvrir l'ampitude jusu'au maxsize
                else vSize = vMaxSize - vMeanTube.GetComponent<MeanTubeMoving>()._maxMovingUp;

                vMeanTube.GetComponent<MeanTubeMoving>().InitSpeed(_avancement);
                break;
            default:
                vMeanTube = Instantiate(_meanTubePrefab, transform.position, Quaternion.identity);
                break;
        }

        //On positionne l'objet
        vMeanTube.transform.SetParent(_meanTubesParent);
        vMeanTube.transform.position = vPosition;
        if (pTypeOfTube == 2)
        {
            vMeanTube.transform.position += (Vector3)vOrthoDirection * vMeanTube.GetComponent<MeanTubeMoving>()._maxMovingUp * (pWallName == "A" ? -1 : 1);
            vMeanTube.GetComponent<MeanTubeMoving>()._isMovingUp = false;
            vMeanTube.GetComponent<MeanTubeMoving>()._positionInit = vPosition;
        }

        //On oriente l'objet en focntion de la direction du segment
        // A : on ajoute 180 degrés, B : on n'ajoute rien
        float vAngle = Vector2.SignedAngle(Vector2.right, vDirection) + (pWallName == "A" ? 180 : 0);
        vMeanTube.transform.eulerAngles = new Vector3(vMeanTube.transform.eulerAngles.x, vMeanTube.transform.eulerAngles.y, vAngle);
        //nregistrement de la taille pour calcul des tailles futures
        //POur les flying tube on s'en fout
        if (pTypeOfTube == 3) return;
        else if (pWallName == "A") _lastSizeTubeA = pTypeOfTube == 2 ? vMaxSize : vSize;
        else _lastSizeTubeB = pTypeOfTube == 2 ? vMaxSize : vSize;
    }

    private void SpawnItemInSection(Vector2 pWallSegmentStart, Vector2 pWallSegmentEnd, float pDistanceLimitFromWall)
    {
        Random vRanDistance = new Random();
        Vector2 vDirection = (pWallSegmentEnd - pWallSegmentStart).normalized;
        Vector2 vPosition = pWallSegmentEnd;

        float vMinSize = pDistanceLimitFromWall / 3;
        float vMaxSize = pDistanceLimitFromWall * 2;

        float vDistance = vRanDistance.Next(Mathf.FloorToInt(vMinSize), Mathf.FloorToInt(vMaxSize));

        Vector2 vOrthoDirection = new Vector3(-vDirection.y, vDirection.x);
        vPosition -= vOrthoDirection * vDistance;

        //On instancie l'objet
        GameObject vDollarBill;

        vDollarBill = Instantiate(_dollarBillPrefab, transform.position, Quaternion.identity);
        //On positionne l'objet
        vDollarBill.transform.SetParent(_itemsParent);
        vDollarBill.transform.position = vPosition;
    }

    float GetTubeMaxSize(float pLastSize, float pDistanceLimitFromWall, Vector3 pPosition, Vector2 pDirection, float pMinSize, string pWallName)
    {
        float vMaxSize = math.min(pDistanceLimitFromWall - _playerSize.y, _meanTubeSize.y);
        if (pLastSize > 0) vMaxSize = math.min(vMaxSize, pLastSize + _maxDeltaBtwTubes);

        Vector2 vOrthoDirection = new Vector3(-pDirection.y, pDirection.x).normalized;
        if (pWallName == "A") vOrthoDirection *= -1;

        float vAngle = Vector2.SignedAngle(Vector2.right, pDirection) + (pWallName == "A" ? 180 : 0);
        Vector2 vCastBox = new Vector2(_meanTubeSize.x, 0.2f);
        int vLayerMask = 1 << _meanTubePrefab.layer;
        //On regarde si des tuyaux existent deja en face (en regardant 4 de chaque côté aussi)
        for (int lCptOnTubeSides = -6; lCptOnTubeSides <= 6; lCptOnTubeSides++)
        {
            Vector2 lOrigin = (Vector2)pPosition + vOrthoDirection * 0.2f + lCptOnTubeSides * pDirection.normalized * _meanTubeSize.x;
            //On utilise pour cela un boxCast
            RaycastHit2D lHit = Physics2D.BoxCast(lOrigin, vCastBox, vAngle, vOrthoDirection, pDistanceLimitFromWall * 1.1f, vLayerMask);
            if (lHit.collider) vMaxSize = Mathf.Min(lHit.distance - _playerSize.y, vMaxSize);
        }

        vMaxSize = Mathf.Clamp(vMaxSize, Mathf.FloorToInt(pMinSize), _meanTubeSize.y);
        return vMaxSize;
    }

    public void DestroyAllTubes()
    {
        if (_meanTubesParent != null)
            for (int lCptTube = 0; lCptTube < _meanTubesParent.childCount; lCptTube++)
                Destroy(_meanTubesParent.GetChild(lCptTube).gameObject);

        if (_itemsParent != null)
            for (int lCptItem = 0; lCptItem < _itemsParent.childCount; lCptItem++)
                Destroy(_itemsParent.GetChild(lCptItem).gameObject);
    }

}
