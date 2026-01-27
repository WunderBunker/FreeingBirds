using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(BirdOverride))]
public class BirdOverrideEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        BirdOverride BO = (BirdOverride)target;
        if (GUILayout.Button("Set Values From Scene"))
            BO.InitDefaultValues();
    }
}
#endif


[CreateAssetMenu(fileName = "BirdOverride", menuName = "Scriptable Objects/BirdOverride")]
public class BirdOverride : ScriptableObject
{
    public bool IsDefault;
    public int NbNodesPerChunk;
    [Range(0, 40)] public int MaxDistanceBtwNodes;
    [Range(0, 40)] public int MinDistanceBtwNodes;
    [Range(0, 180)] public int MaxAngleBtwNodes;
    [Range(0, 180)] public int MinAngleBtwNodes;
    [Range(0, 75)] public float DistanceToWalls;

    public int NbPointParVecteur;
    public int LargeurMurs;
    public float TubeDistanceLimitFromWall;
    public float TubeSpacing;
    public int[] ValuesForTube = { 0, 0 };
    public int[] ValuesForMovingTube = { 0, 0 };
    public int[] ValuesForFlyingTube = { 0, 0 };
    public int[] ValuesForFlyingTubeSharpnel = { 0, 0 };
    public int[] ValuesForDollarBill = { 0, 0 };
    public float HorizonAvancementForMaxSpawn;
    public float PlayerSizeCoeff = 0;

    public float DecalagePlayerCam;
    public float CamDistanceForMaxSpeed;
    public float CamOrthoSize;

    public void OverrideProperties(bool pSlowCamSizeSwitxh = false)
    {
        MapOveride();
        SpawnsOverride();
        CamAndPlayerOverride(pSlowCamSizeSwitxh);
    }

    public void MapOveride()
    {
        PathScript vPathScript;
        vPathScript = GameObject.FindGameObjectWithTag("Path").GetComponent<PathScript>();
        if (IsDefault || NbNodesPerChunk != 0)
            vPathScript._nbNodesPerChunk = NbNodesPerChunk;
        if (IsDefault || MaxDistanceBtwNodes != 0)
            vPathScript._maxDistanceBtwNodes = MaxDistanceBtwNodes;
        if (IsDefault || MinDistanceBtwNodes != 0)
            vPathScript._minDistanceBtwNodes = MinDistanceBtwNodes;
        if (IsDefault || MaxAngleBtwNodes != 0)
            vPathScript._maxAngleBtwNodes = MaxAngleBtwNodes > 1 ? MaxAngleBtwNodes : 0; //1 degré est arrondi à 0 pour différencier du par défaut
        if (IsDefault || MinAngleBtwNodes != 0)
            vPathScript._minAngleBtwNodes = MinAngleBtwNodes > 1 ? MinAngleBtwNodes : 0; //1 degré est arrondi à 0 pour différencier du par défaut

        ChunkScript vChunkScript;
        vChunkScript = GameObject.FindGameObjectWithTag("Map").GetComponent<ChunkScript>();
        if (IsDefault || DistanceToWalls != 0)
            vChunkScript.DistanceToWalls = DistanceToWalls;
        if (IsDefault || NbPointParVecteur != 0)
            vChunkScript.NbPointParVecteur = NbPointParVecteur;
        if (IsDefault || LargeurMurs != 0)
            vChunkScript.LargeurMurs = LargeurMurs;

    }

    public void SpawnsOverride()
    {
        SpawnManager vSpawnManager;
        vSpawnManager = GameObject.FindGameObjectWithTag("SpawnManager").GetComponent<SpawnManager>();
        if (IsDefault || TubeDistanceLimitFromWall != 0)
            vSpawnManager._tubeDistanceLimitFromWall = TubeDistanceLimitFromWall;
        if (IsDefault || TubeSpacing != 0)
            vSpawnManager._tubeSpacing = TubeSpacing;
        if (IsDefault || ValuesForTube[0] != 0 || ValuesForTube[1] != 0)
            vSpawnManager._valuesForTube = ValuesForTube;
        if (IsDefault || ValuesForMovingTube[0] != 0 || ValuesForMovingTube[1] != 0)
            vSpawnManager._valuesForMovingTube = ValuesForMovingTube;
        if (IsDefault || ValuesForFlyingTube[0] != 0 || ValuesForFlyingTube[1] != 0)
            vSpawnManager._valuesForFlyingTube = ValuesForFlyingTube;
        if (IsDefault || ValuesForFlyingTubeSharpnel[0] != 0 || ValuesForFlyingTubeSharpnel[1] != 0)
            vSpawnManager._valuesForFlyingTubeSharpnel = ValuesForFlyingTubeSharpnel;
        if (IsDefault || ValuesForDollarBill[0] != 0 || ValuesForDollarBill[1] != 0)
            vSpawnManager._valuesForDollarBill = ValuesForDollarBill;
        if (IsDefault || HorizonAvancementForMaxSpawn != 0)
            vSpawnManager._horizonAvancementForMaxSpawn = HorizonAvancementForMaxSpawn;
        if (IsDefault || PlayerSizeCoeff != 0)
            vSpawnManager._playerSizeCoeff = PlayerSizeCoeff;
    }

    public void CamAndPlayerOverride(bool pSlowCamSizeSwitch = false)
    {
        CamFollowPath vCamFollowPath;
        vCamFollowPath = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CamFollowPath>();
        if (IsDefault || CamDistanceForMaxSpeed != 0)
            vCamFollowPath._camDistanceForMaxSpeedOrtho = CamDistanceForMaxSpeed;
        if (IsDefault || DecalagePlayerCam != 0)
            vCamFollowPath._decalagePlayerCam = DecalagePlayerCam;
        if (IsDefault || CamOrthoSize != 0)
        {
            if (!pSlowCamSizeSwitch)
                GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>().orthographicSize = CamOrthoSize;
            else
                GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CamFollowPath>().SlowChangeOrthoSize(CamOrthoSize, 10);
        }
    }

    public void InitDefaultValues()
    {
        PathScript vPathScript;
        vPathScript = GameObject.FindGameObjectWithTag("Path").GetComponent<PathScript>();
        NbNodesPerChunk = vPathScript._nbNodesPerChunk;
        MaxDistanceBtwNodes = vPathScript._maxDistanceBtwNodes;
        MinDistanceBtwNodes = vPathScript._minDistanceBtwNodes;
        MaxAngleBtwNodes = vPathScript._maxAngleBtwNodes;
        MinAngleBtwNodes = vPathScript._minAngleBtwNodes;

        ChunkScript vChunkScript;
        vChunkScript = GameObject.FindGameObjectWithTag("Map").GetComponent<ChunkScript>();
        DistanceToWalls = vChunkScript.DistanceToWalls;
        NbPointParVecteur = vChunkScript.NbPointParVecteur;
        LargeurMurs = vChunkScript.LargeurMurs;

        SpawnManager vSpawnManager;
        vSpawnManager = GameObject.FindGameObjectWithTag("SpawnManager").GetComponent<SpawnManager>();
        TubeDistanceLimitFromWall = vSpawnManager._tubeDistanceLimitFromWall;
        TubeSpacing = vSpawnManager._tubeSpacing;
        ValuesForTube = vSpawnManager._valuesForTube;

        ValuesForMovingTube = vSpawnManager._valuesForMovingTube;
        ValuesForFlyingTube = vSpawnManager._valuesForFlyingTube;
        ValuesForFlyingTubeSharpnel = vSpawnManager._valuesForFlyingTubeSharpnel;
        ValuesForDollarBill = vSpawnManager._valuesForDollarBill;
        HorizonAvancementForMaxSpawn = vSpawnManager._horizonAvancementForMaxSpawn;
        PlayerSizeCoeff = vSpawnManager._playerSizeCoeff;

        CamFollowPath vCamFollowPath;
        vCamFollowPath = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CamFollowPath>();
        CamDistanceForMaxSpeed = vCamFollowPath._camDistanceForMaxSpeedOrtho;
        DecalagePlayerCam = vCamFollowPath._decalagePlayerCam;
        CamOrthoSize = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>().orthographicSize;

#if UNITY_EDITOR
        EditorUtility.SetDirty(this); // Marque l’objet comme modifié
        PrefabUtility.RecordPrefabInstancePropertyModifications(this); // Pour forcer la sauvegarde sur le prefab
#endif
    }
}
