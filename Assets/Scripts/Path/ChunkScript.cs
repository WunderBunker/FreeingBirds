using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;


#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(ChunkScript))]
public class ChunkScriptEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        ChunkScript CH = (ChunkScript)target;
        if (GUILayout.Button("Save Meshs"))
            CH.SaveMesh();
    }
}
#endif


public class ChunkScript : MonoBehaviour
{
    [SerializeField][Range(0, 75)] public float DistanceToWalls;

    /*Nombre de points de collider qu'on souhaite récupérer des matrice de présence pour chaque vecteur constituant le chemin*/
    [SerializeField] public int NbPointParVecteur = 2;
    /*Distances des points de collider "out path" par rapport au début du mur (largeur du mur)*/
    [SerializeField] public int LargeurMurs = 80;

    //Objet chargé de la gestion des spawns
    [SerializeField] SpawnManager _spawnManager;

    //Objet chargé de générer un chemin procédural (constitués de vecteurs, ou nodes) 
    //et de déclencher les génarations de chunks de terrain autour de celui-ci avec le présent objet
    PathScript _pathManager;

    //liste des nouveaux noeuds pour le nouveau chunk (quasi identique à pathmanager.nodelist)
    List<Vector2> _newPointList = new List<Vector2>();
    //Liste des vecteurs formés par les new points
    List<Vector2> _newVectorList = new List<Vector2>();

    public List<PolyPoint> _pointListA = new List<PolyPoint>();
    List<PolyPoint> _pointListB = new List<PolyPoint>();
    List<PolyPoint> _lastPointListA = new List<PolyPoint>();
    List<PolyPoint> _lastPointListB = new List<PolyPoint>();

    //Collider et Mesh pour la zone A
    private PolygonCollider2D _polygonA;
    private MeshFilter _meshFilterA;
    private Mesh _meshA;

    //Collider et Mesh pour la zone B
    private PolygonCollider2D _polygonB;
    private MeshFilter _meshFilterB;
    private Mesh _meshB;

    //True si collider et mesh des zones déjà instanciés
    private bool _firstChunk = true;
    private bool _canSpawn = false;

    [Serializable]
    public class PolyPoint
    {
        public int IndexOnPath;
        public Vector2 PointInPath;
        public Vector2 PointOnPath;
        public Vector2 PointOutPath;
        public Vector2 PrevPointInPath;
    }

    void Awake()
    {
        GameObject vPolygonA;
        GameObject vPolygonB;

        _pathManager = GameObject.FindGameObjectWithTag("Path").GetComponent<PathScript>();
        _spawnManager = GameObject.FindGameObjectWithTag("SpawnManager").GetComponent<SpawnManager>();

        //Initailisation des composants zone A
        vPolygonA = GameObject.FindGameObjectWithTag("PolyA");
        _polygonA = vPolygonA.GetComponent<PolygonCollider2D>();
        _meshFilterA = vPolygonA.GetComponent<MeshFilter>();
        _meshA = new Mesh();
        _meshFilterA.mesh = _meshA;

        //Initailisation des composants zone B
        vPolygonB = GameObject.FindGameObjectWithTag("PolyB");
        _polygonB = vPolygonB.GetComponent<PolygonCollider2D>();
        _meshFilterB = vPolygonB.GetComponent<MeshFilter>();
        _meshB = new Mesh();
        _meshFilterB.mesh = _meshB;
    }


    //Méthode principale qui gère la génération d'un nouveau chunk à partir d'un indice dans la liste des noeuds et d'un nombre de noeud à prendre
    public void CreateChunk(int pIndexFirstNode, int pNumberOfNodes, bool pCanSpawn = false)
    {
        _canSpawn = pCanSpawn;

        //Si premier Chunk on initialise le spawnManager
        if (_firstChunk) _spawnManager.GetPlayerInfos();

        //On initialise les différentes listes/array de points à partir du path manager
        InitialiserChunkNodesList(pIndexFirstNode, pNumberOfNodes);

        //On génère les points à ajouter aux colliders des deux zones
        GenererCollidersNewPoints(pIndexFirstNode);

        GeneratePolygonCollider("A");
        GeneratePolygonCollider("B");

        _firstChunk = false;
    }

    //On inititialise la liste des nouveaux noeuds pour les quelles on veut créer un chunk, 
    //on les récupère de la liste du path manager à partir d'un indice donné sous la forme d'une liste des noeuds directement ainsi que des vecteurs qu'ils forment
    private void InitialiserChunkNodesList(int pIndexFirstNode, int pNumberOfNodes)
    {
        _newVectorList.Clear();
        _newPointList.Clear();

        for (int lCptNodes = pIndexFirstNode; lCptNodes < pIndexFirstNode + pNumberOfNodes; lCptNodes++)
        {
            //Pour le premier point d'un nouveau chunk, on fait le lien avec les noeuds de celui d'avant
            if (lCptNodes == pIndexFirstNode && pIndexFirstNode > 0)
            {
                _newVectorList.Add(_pathManager.NodeList[pIndexFirstNode] - _pathManager.NodeList[pIndexFirstNode - 1]);
                _newPointList.Add(_pathManager.NodeList[pIndexFirstNode - 1]);
            }
            else if (lCptNodes > pIndexFirstNode) _newVectorList.Add(_pathManager.NodeList[lCptNodes] - _pathManager.NodeList[lCptNodes - 1]);

            _newPointList.Add(_pathManager.NodeList[lCptNodes]);
        }
    }

    void GenererCollidersNewPoints(int pFirstIndexOnPath)
    {
        Vector2 vNewVector;

        /*Longueur d'une division du nouveau vecteur en cours de traitement, par rapport au _nbPointParVecteur donc */
        float vLgSection;
        /*Position sur le vecteur en cours correspondant à l'itération par segment en cours*/
        Vector2 vTempPoint;

        bool vAddOutPath;

        int vLastChunkLastIndexA = 0;
        int vLastChunkLastIndexB = 0;

        List<PolyPoint> vPointsToRemoveA = new();
        List<PolyPoint> vPointsToRemoveB = new();

        _pointListA.Clear();
        _pointListB.Clear();

        //On conserve les anciens polyPoints qui sont toujours sur le path et on supprime les autres
        foreach (PolyPoint lPoly in _lastPointListA)
            if (lPoly.IndexOnPath < 0) vPointsToRemoveA.Add(lPoly);
            else
            {
                _pointListA.Add(lPoly);
                vLastChunkLastIndexA++;
            }
        foreach (PolyPoint lPoly in _lastPointListB)
            if (lPoly.IndexOnPath < 0) vPointsToRemoveB.Add(lPoly);
            else
            {
                _pointListB.Add(lPoly);
                vLastChunkLastIndexB++;
            }

        //Tant qu'il n'y a pas eu de remap (premiers chunks) rien ne sera supprimé, donc on check pour transmettre l'info au spawnmanager après
        bool vPointsAreRemoved = (vPointsToRemoveA.Count > 0) || (vPointsToRemoveB.Count > 0);

        //On supprime les anciens points des chunks maintenant en dehors du path
        foreach (PolyPoint lPoly in vPointsToRemoveA)
            _lastPointListA.Remove(lPoly);
        foreach (PolyPoint lPoly in vPointsToRemoveB)
            _lastPointListB.Remove(lPoly);
        

        //On prévient le spawnmanager qu'il entame un nouveau chunk
        _spawnManager.StartNewObjectsChunk(vPointsAreRemoved);

        /*On parcours chaque Nouveau vecteur du chemin pour le chunk en cours */
        for (int lCptNewVector = 0; lCptNewVector < _newVectorList.Count(); lCptNewVector++)
        {
            vNewVector = _newVectorList[lCptNewVector];
            vLgSection = vNewVector.magnitude / NbPointParVecteur;
            vAddOutPath = false;

            Vector2[] vFirstLastInPathGeneratedA = new Vector2[2] { Vector2.zero, Vector2.zero };
            Vector2[] vFirstLastInPathGeneratedB = new Vector2[2] { Vector2.zero, Vector2.zero };

            /*On parcours le vecteur section par section selon le nombre de points qu'on souhaite récupérer pour un vecteur*/
            for (int lCptPoint = 0; lCptPoint < NbPointParVecteur; lCptPoint++)
            {
                /*Position sur le vecteur correspondant à cette itération de section, on va regarder à partir de ce point là quelle éléments des matrices de présences récupérer*/
                vTempPoint = _newPointList[lCptNewVector] + vNewVector.normalized * vLgSection * lCptPoint;
                //On fait en sorte d'avoir un outpath par vecteur, centré au milieu
                vAddOutPath = !vAddOutPath && (lCptPoint >= NbPointParVecteur / 2);

                CreatePolyPoints(vTempPoint, vNewVector, vAddOutPath, ref vFirstLastInPathGeneratedA, ref vFirstLastInPathGeneratedB, pFirstIndexOnPath + lCptNewVector);
            }

            if (_canSpawn) _spawnManager.GererNewSpawnsOnSegment(vFirstLastInPathGeneratedA, vFirstLastInPathGeneratedB, DistanceToWalls);
        }

        //On ajoute les nouveaux éléments dans les lastpointinlist pour le prochain chunck
        for (int lCpt = vLastChunkLastIndexA; lCpt < _pointListA.Count; lCpt++)
            _lastPointListA.Add(_pointListA[lCpt]);

        for (int lCpt = vLastChunkLastIndexB; lCpt < _pointListB.Count; lCpt++)
            _lastPointListB.Add(_pointListB[lCpt]);


    }

    void CreatePolyPoints(Vector2 pVectorPoint, Vector2 pVector, bool pAddPointOutPath,
        ref Vector2[] pFirstLastInPathGeneratedA, ref Vector2[] pFirstLastInPathGeneratedB, int pIndexOnPath)
    {
        pVector = pVector.normalized;
        Vector2 vOrthoVector = new Vector2(-pVector.y, pVector.x);

        Action<List<PolyPoint>, Vector2[], string> aTreatPolyPoint = (List<PolyPoint> pPPList, Vector2[] pFirstLastInPathGenerated, string pPolyName) =>
        {
            PolyPoint vCurrentNewPoint = new()
            {
                IndexOnPath = pIndexOnPath,
                PointInPath = Vector2.zero,
                PointOnPath = Vector2.zero,
                PointOutPath = Vector2.zero,
                PrevPointInPath = Vector2.zero
            };

            Vector2 vPotentialPointInPath = pVectorPoint + vOrthoVector * DistanceToWalls * (pPolyName == "A" ? 1 : (-1));
            Vector2 vPotentialPointOutPath = vPotentialPointInPath + vOrthoVector * LargeurMurs * (pPolyName == "A" ? 1 : (-1));

            bool vCanAddPoint = true;
            //On controle que le point inPath que l'on veut ajouter ne croise pas un segment chemin/inpath existant 
            for (int lIndicePoint = 0; lIndicePoint < pPPList.Count(); lIndicePoint++)
                if (Tools.SegmentsIntersect(pPPList[lIndicePoint].PointOnPath, pPPList[lIndicePoint].PointInPath, vPotentialPointInPath, pVectorPoint))
                {
                    vCanAddPoint = false;
                    break;
                }

            //On ajoute le nouveau point inPath si controles ok
            if (vCanAddPoint)
            {
                vCurrentNewPoint.PointInPath = vPotentialPointInPath;
                vCurrentNewPoint.PointOnPath = pVectorPoint;

                Vector2 vPreviousPointInPath = pPPList.Count() > 0 ? pPPList[pPPList.Count() - 1].PointInPath : Vector2.zero;
                vCurrentNewPoint.PrevPointInPath = vPreviousPointInPath;

                //On maj le First/last point du segment inPath du vecteur en cours pour le spawnManager 
                if (pFirstLastInPathGenerated[0] == Vector2.zero) pFirstLastInPathGenerated[0] = vPotentialPointInPath;
                pFirstLastInPathGenerated[1] = vPotentialPointInPath;

                // On contrôle que le segment inPath/PreviousInPath ainsi créé ne croise pas un couple inpath/outpath existant
                if (vPreviousPointInPath != Vector2.zero)
                    for (int lIndicePoint = 0; lIndicePoint < pPPList.Count() - 1; lIndicePoint++)
                        if (Tools.SegmentsIntersect(vCurrentNewPoint.PointInPath, vCurrentNewPoint.PrevPointInPath + (vCurrentNewPoint.PointInPath - vCurrentNewPoint.PrevPointInPath) / 1000,
                         pPPList[lIndicePoint].PointOutPath, pPPList[lIndicePoint].PointInPath))
                            //Si oui on supprime l'ancien outpath
                            pPPList[lIndicePoint].PointOutPath = Vector2.zero;
            }
            //On essaie d'ajouter un Outpath si demandé
            if (pAddPointOutPath && vCurrentNewPoint.PointInPath != Vector2.zero)
            {
                //On récupère le point outpath précédent    
                Vector2 vPreviousPointOutPath = pPPList.Count() > 0 ? GetpreviousOutPath(pPPList.Count() - 1, pPolyName) : Vector2.zero;

                vCanAddPoint = true;
                for (int lIndicePoint = pPPList.Count() - 1; lIndicePoint >= 0; lIndicePoint--)
                {
                    //On controle que le segment chemin/outPath que l'on veut ajouter ne croise pas un segment chemin/outpath existant 
                    if (pPPList[lIndicePoint].PointOutPath != Vector2.zero)
                        if (Tools.SegmentsIntersect(pPPList[lIndicePoint].PointOnPath, pPPList[lIndicePoint].PointOutPath, vCurrentNewPoint.PointOnPath, vPotentialPointOutPath))
                        {
                            //Si c'est le cas alors on fusionne l'ancien outpath et le nouveau à leur milieu
                            Vector2 lMiddlePoint = (pPPList[lIndicePoint].PointOutPath + vPotentialPointOutPath) / 2;
                            vPotentialPointOutPath = lMiddlePoint;
                            pPPList[lIndicePoint].PointOutPath = Vector2.zero;
                            continue;
                        }

                    // inpath/outpath v/s inPath/PreviousInPath existant 
                    if (pPPList[lIndicePoint].PrevPointInPath != Vector2.zero)
                        if (Tools.SegmentsIntersect(vCurrentNewPoint.PointInPath, vPotentialPointOutPath, pPPList[lIndicePoint].PointInPath, pPPList[lIndicePoint].PrevPointInPath))
                        {
                            vCanAddPoint = false;
                            break;
                        }

                    // outpath/previousOutpath v/s outPath/previousOutPath existant 
                    if (lIndicePoint > 0 && vPreviousPointOutPath != Vector2.zero
                        && pPPList[lIndicePoint].PointOutPath != Vector2.zero && pPPList[lIndicePoint].PointOutPath != vPreviousPointOutPath)
                    {
                        Vector2 vOldPreviousOutPath = GetpreviousOutPath(lIndicePoint - 1, pPolyName);
                        if (vOldPreviousOutPath != Vector2.zero)
                            if (Tools.SegmentsIntersect(vPotentialPointOutPath, vPreviousPointOutPath, pPPList[lIndicePoint].PointOutPath, vOldPreviousOutPath))
                                //Si oui on supprime l'ancien outpath (et on ne break pas pour avoir le controle sur le point suivant qui peut donc former un nouveau segment avec le oldPreviouspath)
                                pPPList[lIndicePoint].PointOutPath = Vector2.zero;
                    }
                }

                //On ajoute le nouveau point outPath si controles ok
                if (vCanAddPoint)
                    vCurrentNewPoint.PointOutPath = vPotentialPointOutPath;
            }

            if (vCurrentNewPoint.PointInPath != Vector2.zero) pPPList.Add(vCurrentNewPoint);
        };

        aTreatPolyPoint.Invoke(_pointListA, pFirstLastInPathGeneratedA, "A");
        aTreatPolyPoint.Invoke(_pointListB, pFirstLastInPathGeneratedB, "B");
    }

    Vector2 GetpreviousOutPath(int pCurrentOutpathIndex, string pColliderName)
    {
        Vector2 vPreviousPointOutPath = Vector2.zero;
        List<PolyPoint> vPPList = pColliderName == "A" ? _pointListA : _pointListB;

        for (int lIndicePoint = pCurrentOutpathIndex; lIndicePoint >= 0; lIndicePoint--)
            if (vPPList[lIndicePoint].PointOutPath != Vector2.zero)
            {
                vPreviousPointOutPath = vPPList[lIndicePoint].PointOutPath;
                break;
            }

        return vPreviousPointOutPath;
    }

    void GeneratePolygonCollider(string pPolyName)
    {
        List<Vector2> vChainOfNewPoints = new List<Vector2>();

        List<PolyPoint> vPPList = pPolyName == "A" ? _pointListA : _pointListB;
        PolygonCollider2D vPoly = pPolyName == "A" ? _polygonA : _polygonB;
        var vMesh = pPolyName == "A" ? _meshA : _meshB;

        bool vNoOutpath = true;
        //On récupère tous les points du polygon dans un même Array et dans l'ordre
        for (int lCptPoint = 0; lCptPoint < vPPList.Count; lCptPoint++)
            vChainOfNewPoints.Add(vPPList[lCptPoint].PointInPath);
        for (int lCptPoint = vPPList.Count - 1; lCptPoint >= 0; lCptPoint--)
            if (vPPList[lCptPoint].PointOutPath != Vector2.zero)
            {
                vChainOfNewPoints.Add(vPPList[lCptPoint].PointOutPath);
                vNoOutpath = false;
            }

        if (vNoOutpath)
        {
            return;
        }

        if (_firstChunk) vPoly.pathCount = 1;

        //On remplace les points actuels du polygon par les nouveaux
        vPoly.SetPath(0, vChainOfNewPoints.ToArray());


        //On  récupère les points et les UV directements à partir des points du polygon
        Vector2[] vMeshUV = new Vector2[vPoly.points.Count()];

        for (int lCptPoints = 0; lCptPoints < vPoly.points.Count(); lCptPoints++)
            vMeshUV[lCptPoints] = vPoly.points[lCptPoints] + vPoly.offset;

        //On récupère les triangles en triangulant les UV
        int[] vMeshTriangles = TriangulatorBis.Triangulate(ref vMeshUV);

        Vector3[] vMeshVertices = new Vector3[vMeshUV.Length];
        for (int lCptUV = 0; lCptUV < vMeshUV.Length; lCptUV++) vMeshVertices[lCptUV] = vMeshUV[lCptUV];

        //On maj les données dans la mesh
        vMesh.triangles = null;
        vMesh.vertices = vMeshVertices;
        vMesh.uv = vMeshUV;
        vMesh.triangles = vMeshTriangles;
        vMesh.RecalculateBounds();
    }

    public void DeleteAllhunks()
    {
        _polygonA.points = null;
        _polygonA.SetPath(0, new Vector2[0]);
        _polygonB.points = null;
        _polygonB.SetPath(0, new Vector2[0]);
        _newPointList.Clear();
        _newVectorList.Clear();
        _pointListA.Clear();
        _pointListB.Clear();
        _lastPointListA.Clear();
        _lastPointListB.Clear();
        _meshA.triangles = null;
        _meshB.triangles = null;
        _meshA.vertices = null;
        _meshB.vertices = null;
        _meshA.uv = null;
        _meshB.uv = null;
        _firstChunk = true;
        _canSpawn = false;
        _spawnManager.DestroyAllTubes();
    }

    public void RemapOnPathIndex(int pShift)
    {
        for (int i = 0; i < _lastPointListA.Count; i++)
        {
            PolyPoint lPoly = _lastPointListA[i];
            lPoly.IndexOnPath -= pShift;
            _lastPointListA[i] = lPoly;
        }
        for (int i = 0; i < _lastPointListB.Count; i++)
        {
            PolyPoint lPoly = _lastPointListB[i];
            lPoly.IndexOnPath -= pShift;
            _lastPointListB[i] = lPoly;
        }
    }

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {

        for (int i = 0; i < _newPointList.Count - 1; i++)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(_newPointList[i], _newPointList[i] + _newVectorList[i]);
        }

        Action<List<PolyPoint>, string> vDrawPoly = (List<PolyPoint> pPoly, string pPolyName) =>
        {
            GUIStyle style = new GUIStyle();
            style.fontStyle = FontStyle.Bold;
            style.fontSize = 16;

            PolygonCollider2D vPoly2D = (pPolyName == "A") ? _polygonA : _polygonB;
            if (vPoly2D == null || !vPoly2D.gameObject.activeSelf) return;

            if (pPoly != null)
                for (int lCpt = 0; lCpt < pPoly.Count(); lCpt++)
                {
                    if (lCpt % 3 == 0) Gizmos.color = Color.magenta;
                    else if (lCpt % 3 == 1) Gizmos.color = Color.yellow;
                    else if (lCpt % 3 == 2) Gizmos.color = Color.blue;

                    if (pPoly[lCpt].PointOutPath != Vector2.zero)
                    {
                        Gizmos.DrawLine(pPoly[lCpt].PointOnPath, pPoly[lCpt].PointOutPath);

                        style.normal.textColor = Gizmos.color;
                        Handles.Label(pPoly[lCpt].PointOnPath, pPoly[lCpt].IndexOnPath.ToString(), style);
                    }

                }

            /* var vMesh = pPolyName == "A" ? _meshA : _meshB;
            if (vMesh != null && vMesh.triangles is { } vTriangles)
                for (int lCpt = 0; lCpt < vTriangles.Length; lCpt++)
                {
                    Gizmos.color = Color.ghostWhite;

                    Gizmos.DrawLine(vMesh.uv[vTriangles[lCpt]], vMesh.uv[vTriangles[(lCpt + 1) % vTriangles.Length]]);
                } */
        };

        vDrawPoly.Invoke(_pointListA, "A");
        vDrawPoly.Invoke(_pointListB, "B");
    }

    public void SaveMesh()
    {
        AssetDatabase.CreateAsset(_meshA, "Assets/SavedMesh/meshA.asset");
        AssetDatabase.CreateAsset(_meshB, "Assets/SavedMesh/meshB.asset");
        AssetDatabase.SaveAssets();

    }
#endif
}