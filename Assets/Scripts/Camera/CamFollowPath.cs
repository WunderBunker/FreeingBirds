using UnityEngine;
using System.Collections;
using System;
using Unity.Mathematics;

public class CamFollowPath : MonoBehaviour
{
    //Décalage souhaité avec le joueur sur axe du chemin en pourcentage d'une demi-largeur d'écran (distance max)
    [SerializeField][Range(0, 1)] public float _prctDecalagePlayerCam;
    //Distance à partir de laquelle la caméra est à maxspeed pour rejoindre le joueur sur axe ortho
    [SerializeField] public float _camDistanceForMaxSpeed;
    [SerializeField] public float _camMaxSpeed;
    [SerializeField] float _camMaxAcceleration = 1;
    [SerializeField] public Transform RainCam;
    private float _worldHalfWidth;

    public bool DontSmoothSpeed;
    float _lastCurrentSpeedOrtho;

    Transform _playerShell;

    Vector3 _lastPLayerPosition;

    PartieManager _partieManager;
    float _screenRatio;

    Animator _tiltingAnim;

    Coroutine _slowChangingSize;
    private Vector2 vTargetPosition;

    //On utilise Awake au lieu de start afin d'initialiser les objets de la caméra avant le start du pathScript qui viedra l'affecter
    private void Awake()
    {
        _playerShell = GameObject.FindGameObjectWithTag("PlayerShell").transform;
        _partieManager = PartieManager.Instance.GetComponent<PartieManager>();

        _screenRatio = (float)Screen.height / Screen.width;

        _tiltingAnim = GameObject.FindGameObjectWithTag("MainCanvas").transform.Find("TiltSymbol").GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_partieManager._partieState == PartieState.InGame || _partieManager._partieState == PartieState.PartieStarted)
            CamMove();
    }

    public void InitCam(Vector3 pPosition)
    {
        transform.eulerAngles = Vector3.zero;
        RainCam.eulerAngles = Vector3.zero;
        _lastPLayerPosition = Vector3.zero;
        transform.position = pPosition + new Vector3(0, 0, transform.position.z - pPosition.z);

        _worldHalfWidth = GetComponent<Camera>().orthographicSize / _screenRatio;
    }

    //Déplace la caméra vers le prochain noeud
    private void CamMove()
    {
        //Récupération des infos sur le dernier mouvement effectué par le player
        Vector3 vDirectionPath = _playerShell.GetComponentInChildren<PlayerControl>()._directionOnPath;
        float vMagnitudeOnPath;
        if (_lastPLayerPosition != Vector3.zero)
            vMagnitudeOnPath = Vector3.Dot(_playerShell.position - _lastPLayerPosition, vDirectionPath);
        else vMagnitudeOnPath = _playerShell.GetComponentInChildren<PlayerControl>()._speed * Time.deltaTime;

        //Calcul du décalage souhaité avec le player en fonction du ratio de l'écran et de l'angle
        // (si player va vers le bas on ne veut pas un décalage à 100% de la largeur , car sinon on finit complettement hors cadre)
        //on prend transform.right pour bien tenir compte des rotations de la caméra (chameau)
        float vAngle = Vector2.Angle(transform.right, vDirectionPath) * Mathf.Deg2Rad;
        float vScreenRadius = Tools.EllipseRadius(_worldHalfWidth, _worldHalfWidth * _screenRatio, vAngle);
        float vDecalageForThisDirection = vScreenRadius * _prctDecalagePlayerCam;

        //Maj des positions selon les mouvements du player et de l'écart souhaité pour  la camera
        Vector3 vDeltaCam;
        switch (SaveManager.SafeSave.SelectedBirdId)
        {
            case "Bird4":
                vDeltaCam = CamMoveToPlayer(vDecalageForThisDirection, -vDirectionPath, vMagnitudeOnPath);
                break;
            default:
                vDeltaCam = CamMoveToPlayer(vDecalageForThisDirection, vDirectionPath, vMagnitudeOnPath);
                break;
        }

        transform.position += vDeltaCam;
        _lastPLayerPosition = _playerShell.position;
    }

    Vector3 CamMoveToPlayer(float pEcartFromPlayer, Vector2 pDirectionPath, float pMagnitudeOnPath)
    {
        float vDeltaTime = Time.deltaTime;
        float vCurrentMaxSpeed = DontSmoothSpeed ? _camMaxSpeed * 2 : _camMaxSpeed;

        // I - On calcul l'avancement à effectuer sur l'axe du chemin
        vTargetPosition = (Vector2)_playerShell.position + pDirectionPath * pEcartFromPlayer;
        float vDistanceToTargetOnPath = Vector3.Dot(vTargetPosition - (Vector2)transform.position, pDirectionPath);

        //Pour ce calcul :
        //  - On récupère le dernier mouvement effectué par le joueur
        Vector2 vDeltaCamOnPath = pMagnitudeOnPath * pDirectionPath;
        //vDeltaCamOnPath = vDistanceToTargetOnPath * pDirectionPath;
        float vDistCoef = math.abs(vDistanceToTargetOnPath) / _camDistanceForMaxSpeed;
        //  - On détermine une vitesse comprise entre 0 et la vitesse max en fonction de la distance configurée
        float vCurrentSpeed = Mathf.Lerp(0, vCurrentMaxSpeed, vDistCoef);
        //  - Si on a de l'avance sur la distance souhaitée au joueur on ralenti selon la grandeur de l'avance
        if (vDistanceToTargetOnPath < 0) vDeltaCamOnPath -= pDirectionPath * Time.deltaTime * Mathf.Lerp(0, vCurrentSpeed, vDistCoef);
        //  - Sinon on accélère
        else vDeltaCamOnPath += pDirectionPath * Time.deltaTime * Mathf.Lerp(0, vCurrentSpeed, vDistCoef);


        // II - On calcul l'avancement à effectuer sur l'axe orthogonal au chemin
        Vector2 vOrthoPath = new Vector2(-pDirectionPath.y, pDirectionPath.x);
        Vector2 vDistanceToPlayer = (Vector2)_playerShell.position - (Vector2)transform.position;
        float vSignOfAlignmentOnOrtho = Vector2.Angle(vDistanceToPlayer, vOrthoPath) > 90 ? -1 : 1;
        float vDistanceToPlayerProjectOrtho = Vector3.Project(vDistanceToPlayer, vOrthoPath).magnitude;

        //Pour ce calcul :
        //  - On détermine une vitesse comprise entre 0 et la vitesse max en fonction de la distance configurée
        vCurrentSpeed = Mathf.Lerp(0, vCurrentMaxSpeed, vDistanceToPlayerProjectOrtho / _camDistanceForMaxSpeed);
        //  - On clamp la vitesse de manière à ne pas dépasser une certaine accélération par rapport à la dernière vitesse
        vCurrentSpeed = Mathf.Clamp(vCurrentSpeed, _lastCurrentSpeedOrtho - _camMaxAcceleration * Time.deltaTime, _lastCurrentSpeedOrtho + _camMaxAcceleration * Time.deltaTime);
        _lastCurrentSpeedOrtho = vCurrentSpeed;
        //  - On calcul le delta
        Vector2 vDeltaCamOnOrtho = vCurrentSpeed * vSignOfAlignmentOnOrtho * vOrthoPath.normalized * vDeltaTime;

        return vDeltaCamOnPath + vDeltaCamOnOrtho;
    }

    public IEnumerator TiltCamera(float pTiltValue)
    {
        _tiltingAnim.SetTrigger("Tilt");

        //On tourne la camera à une certaine vitesse
        yield return new TiltingTransforms(pTiltValue, 20, new Transform[] { transform });
        yield return new TiltingTransforms(pTiltValue, -20, new Transform[] { RainCam.Find("RainDropsParticles") });
    }

    public IEnumerator Tremour()
    {
        yield return null;
        float vAmplitude = SaveManager.SafeSave.SettingsSave.MotionSickness ? 0.1f : 1;
        float vTime = 0.07f;
        transform.position += new Vector3(vAmplitude, -vAmplitude, 0f);
        yield return new WaitForSeconds(vTime);
        transform.position += new Vector3(vAmplitude, vAmplitude, 0f);
        yield return new WaitForSeconds(vTime);
        transform.position += new Vector3(-vAmplitude, 0f, 0f);
        yield return new WaitForSeconds(vTime);
        transform.position += new Vector3(-vAmplitude, -vAmplitude, 0f);
        yield return new WaitForSeconds(vTime);
        transform.position += new Vector3(-vAmplitude, vAmplitude, 0f);
        yield return new WaitForSeconds(vTime);
        transform.position += new Vector3(vAmplitude, 0, 0f);
        yield return new WaitForSeconds(vTime);
    }

    public void SlowChangeOrthoSize(float pSize, float pSpeed)
    {
        if (_slowChangingSize != null) StopCoroutine(_slowChangingSize);
        _slowChangingSize = StartCoroutine(SlowChangingOrthoSize(pSize, pSpeed));
    }

    public IEnumerator SlowChangingOrthoSize(float pSize, float pSpeed)
    {
        Camera vCam = GetComponent<Camera>();

        while (vCam.orthographicSize < pSize)
        {
            vCam.orthographicSize += Time.deltaTime * pSpeed;
            if (vCam.orthographicSize >= pSize) yield break;

            yield return null;
        }
        while (vCam.orthographicSize > pSize)
        {
            vCam.orthographicSize -= Time.deltaTime * pSpeed;
            if (vCam.orthographicSize <= pSize) yield break;

            yield return null;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(vTargetPosition, 1);
    }
#endif
}
