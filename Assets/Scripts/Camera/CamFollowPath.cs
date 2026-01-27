using UnityEngine;
using System.Collections;

public class CamFollowPath : MonoBehaviour
{
    [SerializeField] public float _decalagePlayerCam;

    //Bird2 - Distance à partir de laquelle la caméra est à maxspeed pour rejoindre le joueur
    [SerializeField] public float _camDistanceForMaxSpeedOrtho;
    [SerializeField] public float _camMaxSpeedOnOrtho;
    [SerializeField] public Transform RainCam;

    Transform _playerShell;

    Vector3 _lastPLayerPosition;

    PartieManager _partieManager;
    float _screenRatio;

    Animator _tiltingAnim;

    Coroutine _slowChangingSize;

    //On utilise Awake au lieu de start afin d'initialiser les objets de la caméra avant le start du pathScript qui viedra l'affecter
    private void Awake()
    {
        _playerShell = GameObject.FindGameObjectWithTag("PlayerShell").transform;
        _partieManager = PartieManager.Instance.GetComponent<PartieManager>();

        _screenRatio = Screen.height / Screen.width;

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
    }

    //Déplace la caméra vers le prochain noeud
    private void CamMove()
    {
        //Récupération des infos sur le dernier mouvement effectué par le player
        Vector3 vDirectionPath = _playerShell.GetComponentInChildren<PlayerControl>()._directionOnPath;
        float vMagnitudeOnPath;
        if (_lastPLayerPosition != Vector3.zero)
        {
            vMagnitudeOnPath = Vector3.Project(_playerShell.position - _lastPLayerPosition, vDirectionPath).magnitude;
            int vMagnitudeSign = Vector2.Angle(_playerShell.position - _lastPLayerPosition, vDirectionPath) > 90 ? -1 : 1;
            vMagnitudeOnPath = vMagnitudeOnPath * vMagnitudeSign;
        }
        else vMagnitudeOnPath = _playerShell.GetComponentInChildren<PlayerControl>()._speed * Time.deltaTime;

        //Calcul du décalage souhaité avec le player en fonction du ratio de l'écran 
        // (si player va vers le bas on ne veut pas un décalage à 100% sur l'axe du chemin par ex, car sinon on finit complettement hors cadre)
        float[] vDecalageForThisDirection = new float[2];
        float vAngle = Mathf.Abs(Vector2.SignedAngle(Vector2.right, vDirectionPath));
        vAngle = vAngle > 90 ? (180 - vAngle) : vAngle;

        //Decalage souhaité avec le player sur l'axe du chemin
        vDecalageForThisDirection[0] = Mathf.Lerp(1, _screenRatio, Mathf.InverseLerp(0, 90, vAngle)) * _decalagePlayerCam;
        //Decalage souhaité avec le player sur l'axe orthogonal au chemin
        vDecalageForThisDirection[1] = Mathf.Lerp(1, _screenRatio, Mathf.InverseLerp(90, 0, vAngle)) * _decalagePlayerCam;

        //Maj des positions selon les mouvements du player et de l'écart souhaité pour  la camera
        Vector3 vDeltaCam;
        switch (SaveManager.SafeSave.SelectedBirdId)
        {
            case "Bird4":
                vDeltaCam = CamMoveToPlayer(new float[] { -vDecalageForThisDirection[0], vDecalageForThisDirection[1] }, vDirectionPath, vMagnitudeOnPath);
                break;
            default:
                vDeltaCam = CamMoveToPlayer(vDecalageForThisDirection, vDirectionPath, vMagnitudeOnPath);
                break;
        }

        transform.position += vDeltaCam;
        _lastPLayerPosition = _playerShell.position;
    }

    Vector3 CamMoveToPlayer(float[] pEcartFromPlayer, Vector2 pDirectionPath, float pMagnitudeOnPath)
    {
        float vDeltaTime = Time.deltaTime;

        //On récupère les infos sur la distance actuelle avec le joueur
        Vector2 vDistanceToPlayer = (Vector2)_playerShell.position - (Vector2)transform.position;

        //On calcul l'avancement à effectuer sur l'axe du chemin
        Vector2 vDeltaCamOnPath;
        float vSignOfAlignmentOnPath = Vector2.Angle(vDistanceToPlayer, pDirectionPath) > 90 ? -1 : 1;
        float vDistanceToPlayerProjectPath = Vector3.Project(vDistanceToPlayer, pDirectionPath).magnitude * vSignOfAlignmentOnPath;

        //Pour ce calcul :
        //  - On récupère le dernier mouvement effectué par le joueur
        vDeltaCamOnPath = pMagnitudeOnPath * pDirectionPath;
        //  - On ajoute un écart selon le sens souhaité si le dernier mouvement du joueur va dans ce sens ou si la distance au joueur est inférieure l'écart souhaité dans l'absolue
        if (vSignOfAlignmentOnPath == Mathf.Sign(pEcartFromPlayer[0]) || (Mathf.Abs(vDistanceToPlayerProjectPath) < Mathf.Abs(pEcartFromPlayer[0])))
            vDeltaCamOnPath += Mathf.Sign(pEcartFromPlayer[0]) *
                //  - On détermine cet écart comme étant X * la distance parcourue par le joueur (de façon à X* plus vite ou X* plus lentement afin de créer l'écart)
                //  - Si il n'y a pas de dernier mouvement de player ou qu'on est en avance sur lui on prend une valeur arbitraire (10)   
                ((pMagnitudeOnPath > 0 ? pMagnitudeOnPath / 5 : Time.deltaTime * 10) * pDirectionPath);


        //On calcul l'avancement à effectuer sur l'axe orthogonal au chemin
        Vector2 vDeltaCamOnOrtho;
        Vector2 vOrthoPath = new Vector2(-pDirectionPath.y, pDirectionPath.x);
        float vSignOfAlignmentOnOrtho = Vector2.Angle(vDistanceToPlayer, vOrthoPath) > 90 ? -1 : 1;
        float vDistanceToPlayerProjectOrtho = Vector3.Project(vDistanceToPlayer + vDistanceToPlayer.normalized * pEcartFromPlayer[1], vOrthoPath).magnitude;
        //Pour ce calcul :
        //  - On incrémente la position  entre 0 et gMaxSpeed en focntion de la distance souhaitée 
        vDeltaCamOnOrtho = Mathf.Lerp(0, _camMaxSpeedOnOrtho, Mathf.InverseLerp(0, _camDistanceForMaxSpeedOrtho, vDistanceToPlayerProjectOrtho))
            * vSignOfAlignmentOnOrtho * vOrthoPath.normalized * vDeltaTime;

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
        float vAmplitude = 1;
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
}
