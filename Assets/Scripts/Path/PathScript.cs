using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = System.Random;

//Objet chargé de générer un chemin procédural (constitués de vecteurs, ou nodes), afin d'y faire circuler le player, 
//et de déclencher les génarations de chunks de terrain autour de celui-ci
public class PathScript : MonoBehaviour
{
    public List<Vector2> NodeList = new List<Vector2>();

    private Transform _playerShell;
    private PlayerControl _playerControl;

    private ChunkScript _chunksGenerator;

    private PartieManager _partieManager;

    /*Nb de noeuds (éléments du chemin) encapsulés par un chunk */
    [SerializeField] public int _nbNodesPerChunk;
    /*Distance min et max entre deux noeuds du chemin*/
    [SerializeField][Range(2, 40)] public int _maxDistanceBtwNodes;
    [SerializeField][Range(2, 40)] public int _minDistanceBtwNodes;
    /*Angle max entre deux noeuds du chemin*/
    [SerializeField][Range(0, 180)] public int _maxAngleBtwNodes;
    /*Distance min entre deux noeuds du chemin*/
    [SerializeField][Range(0, 180)] public int _minAngleBtwNodes;

    protected int _cumulAngleGenNodes2 = 0;
    protected int _angleSignGenNodes2 = 1;

    int _retardPlayer;

    // Start is called before the first frame update
    void Awake()
    {
        _playerShell = GameObject.FindGameObjectWithTag("PlayerShell").transform;
        _chunksGenerator = GameObject.FindGameObjectWithTag("Map").GetComponent<ChunkScript>();
        _partieManager = PartieManager.Instance.GetComponent<PartieManager>();
    }

    public void StartPath()
    {
        _playerControl = _playerShell.GetComponent<PlayerShellscript>()._playerControl;

        NodeList.Clear();

        /*On génère 3 premiers chunks d'avance pour positionner le player au milieu du premier, il aura ainsi toujours un 1,5 chunk de retard sur leur génération*/
        /*Generation des noeuds du chemin*/
        GenNodes(3 * _nbNodesPerChunk);
        /*Génération du chunk autour du cheminn*/
        _chunksGenerator.CreateChunk(0, _nbNodesPerChunk, false);
        _chunksGenerator.CreateChunk(_nbNodesPerChunk, _nbNodesPerChunk, false);
        _chunksGenerator.CreateChunk(2 * _nbNodesPerChunk, _nbNodesPerChunk, true);

        _retardPlayer = Mathf.RoundToInt(_nbNodesPerChunk * 1.5f);

        /*Initialisation de la position du player au milieu du premier chunk*/
        _playerControl.InitiatePlayerOnPath(NodeList.Count - 1 - _retardPlayer);
    }

    // Update is called once per frame
    void Update()
    {

        /*On génère un nouveau chunk en fonction de la position du player sur le chemin
         Celui-ci a démarré avec un certain retard sur leur génération, ce test correspond donc à attendre qu'il arrive au dernier chunk moins ce retard*/
        if (_partieManager._partieState == PartieState.PartieStarted && _playerControl._currentPathIndex > NodeList.Count - 1 - _retardPlayer)
        {
            SuppNodes(_nbNodesPerChunk);
            GenNodes(_nbNodesPerChunk);
            _playerControl.RemapIndex(_nbNodesPerChunk);
            _chunksGenerator.RemapOnPathIndex(_nbNodesPerChunk);
            //On génère le chunk associé au derniers noeuds
            _chunksGenerator.CreateChunk(NodeList.Count - _nbNodesPerChunk, _nbNodesPerChunk, true);
        }
    }

    //Génération de vecteurs (neuds) constituant le chemin procédural, que l'on stocke dans nodelist
    void GenNodes(int pShift)
    {
        switch (SaveManager.SafeSave.SelectedBirdId)
        {
            case "Bird2":
                GenNodes2(pShift);
                break;
            case "Bird4":
                GenNodes4(pShift);
                break;
            default:
                GenNodes1(pShift);
                break;
        }
    }

    //Génération de vecteurs (neuds) constituant le chemin procédural, que l'on stocke dans nodelist
    void GenNodes1(int pShift)
    {
        Vector2 vStep;
        Vector2 vNewNode;
        Random vRandomAngle = new Random();
        Random vRandomMagnitude = new Random();

        Vector2 vLastNodeDirection;

        for (int i = 0; i < pShift; i++)
        {
            if (NodeList.Count == 0) NodeList.Add(_playerShell.position);
            else
            {
                if (NodeList.Count >= 2)
                {
                    vLastNodeDirection = (NodeList[NodeList.Count - 1] - NodeList[NodeList.Count - 2]).normalized;
                    vStep = RotateVector2d(vLastNodeDirection, vRandomAngle.Next(-_minAngleBtwNodes, _maxAngleBtwNodes)) * vRandomMagnitude.Next(_minDistanceBtwNodes, _maxDistanceBtwNodes);
                    bool lIntersect = NodeList.Count > 3;
                    int lInfinityCounter = 0;
                    while (lIntersect)
                    {
                        lInfinityCounter++;
                        if (lInfinityCounter > 100)
                        {
                            Debug.LogError("Inifinity loop in GenNodes1");
                            break;
                        }
                        for (int j = 0; j < NodeList.Count - 3; j++)
                        {
                            lIntersect = Tools.SegmentsIntersect(NodeList[NodeList.Count - 1], NodeList[NodeList.Count - 1] + vStep, NodeList[j], NodeList[j + 1]);
                            if (lIntersect)
                            {
                                vStep = RotateVector2d(vLastNodeDirection, vRandomAngle.Next(-90, 90)) * vRandomMagnitude.Next(_minDistanceBtwNodes, _maxDistanceBtwNodes);
                                break;
                            }
                        }
                    }
                }
                else vStep = Vector2.right * vRandomMagnitude.Next(_minDistanceBtwNodes, _maxDistanceBtwNodes);

                vNewNode = NodeList[NodeList.Count - 1] + vStep;
                NodeList.Add(vNewNode);
            }
        }
    }

    //Génération de vecteurs (neuds) constituant le chemin procédural, que l'on stocke dans nodelist
    void GenNodes2(int pShift)
    {
        Vector2 vStep;
        Vector2 vNewNode;

        Vector2 vLastNodeDirection;

        for (int i = 0; i < pShift; i++)
        {
            if (_cumulAngleGenNodes2 >= 360)
            {
                _angleSignGenNodes2 = -_angleSignGenNodes2;
                _cumulAngleGenNodes2 = 0;
            }
            if (NodeList.Count == 0) NodeList.Add(_playerShell.position);
            else
            {
                if (NodeList.Count >= 2)
                {
                    vLastNodeDirection = (NodeList[NodeList.Count - 1] - NodeList[NodeList.Count - 2]).normalized;
                    vStep = RotateVector2d(vLastNodeDirection, _angleSignGenNodes2 * _maxAngleBtwNodes) * _maxDistanceBtwNodes;
                }
                else vStep = Vector2.right * _maxDistanceBtwNodes;

                _cumulAngleGenNodes2 += _maxAngleBtwNodes;

                vNewNode = NodeList[NodeList.Count - 1] + vStep;
                NodeList.Add(vNewNode);
            }
        }
    }

    void GenNodes4(int pShift)
    {
        Vector2 vStep;
        Vector2 vNewNode;
        Random vRandomMagnitude = new Random();

        for (int i = 0; i < pShift; i++)
        {
            if (NodeList.Count == 0) NodeList.Add(_playerShell.position);
            else
            {
                vStep = Vector2.right * vRandomMagnitude.Next(_minDistanceBtwNodes, _maxDistanceBtwNodes);
                vNewNode = NodeList[NodeList.Count - 1] + vStep;
                NodeList.Add(vNewNode);
            }
        }
    }


    //Suppression d'une partie des noeuds
    void SuppNodes(int pShift)
    {
        if (NodeList.Count == 0) return;

        for (int i = 0; i < pShift; i++) NodeList.RemoveAt(0);
    }

    static Vector2 RotateVector2d(Vector2 pVector, float pDegrees)
    {
        float vNewX;
        float vNewY;

        float vRadFromDegrees;
        vRadFromDegrees = Mathf.PI / 180 * pDegrees;

        vNewX = pVector.x * Mathf.Cos(vRadFromDegrees) - pVector.y * Mathf.Sin(vRadFromDegrees);
        vNewY = pVector.x * Mathf.Sin(vRadFromDegrees) + pVector.y * Mathf.Cos(vRadFromDegrees);
        return new Vector2(vNewX, vNewY);
    }

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {

        for (int i = 0; i < NodeList.Count; i++)
        {
            if (i < NodeList.Count - 1)
            {
                Gizmos.color = UnityEngine.Color.red;
                Gizmos.DrawLine(NodeList[i], NodeList[i + 1]);
            }
            Gizmos.color = UnityEngine.Color.blue;
            Gizmos.DrawSphere(NodeList[i], 0.25f);

        }
    }
#endif
}
