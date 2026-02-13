using System.Data;
using UnityEngine;

public class ObjectDestroyer : MonoBehaviour
{
    [SerializeField][Range(0, 100)] float _rotationSpeed;

    PartieManager _partieManager;
    Transform _playerShell;
    Transform _cameraTransform;
    Camera _camera;
    Vector3 _refVelocityPosition;

    private void Awake()
    {
        _partieManager = PartieManager.Instance.GetComponent<PartieManager>();

        _playerShell = GameObject.FindGameObjectWithTag("PlayerShell").transform;

        GameObject vCameraObject;
        vCameraObject = GameObject.FindGameObjectWithTag("MainCamera");
        _cameraTransform = vCameraObject.transform;
        _camera = vCameraObject.GetComponent<Camera>();
    }

    public void SetSize()
    {
        float vSize = GameObject.FindGameObjectWithTag("Map").GetComponent<ChunkScript>().DistanceToWalls * 4;
        GetComponent<BoxCollider2D>().size = new Vector2(2, vSize);
        UpdatePosition();
    }

    private void Update()
    {
        if (_partieManager._partieState == PartieState.PartieStarted)
            UpdatePosition();
    }

    private void UpdatePosition()
    {
        Vector3 vDirectionOnPath;
        vDirectionOnPath = _playerShell.GetComponentInChildren<PlayerControl>()._directionOnPath;
        float vAngle = Vector2.SignedAngle(Vector2.right, vDirectionOnPath);


        //On met d'abord à jour l'orientation
        Vector3 vRotation = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, vAngle);
        Quaternion vRotationQ = Quaternion.Euler(vRotation.x, vRotation.y, vRotation.z);

        transform.rotation = Quaternion.Lerp(transform.rotation, vRotationQ, Time.deltaTime * _rotationSpeed);

        //Puis on détermine les paramètre du rectangle (viewport) de la caméra
        float vDistanceToCamera;

        float vTailleLongueur = (_camera.ViewportToWorldPoint(Vector3.right) - _camera.ViewportToWorldPoint(Vector3.zero)).magnitude;
        float vTailleLargeur = (_camera.ViewportToWorldPoint(Vector3.up) - _camera.ViewportToWorldPoint(Vector3.zero)).magnitude;


        //Et trigo trigo trigo : on obtient la distance entre le mur et la camera pour que celui-ci reste sur le rectangle du viewport
        float vAngleFirstCoinRectangle = Mathf.Asin(vTailleLargeur / vTailleLongueur);
        Vector2 vPositionFromCentre;
        //On passe l'angle en valeur absolue pour n'avoir à calculer que les cas positifs (seul la distance absolue au centre nous intéresse, la direction est récupérée à part)
        vAngle = Mathf.Abs(vAngle * Mathf.PI / 180);
        if (vAngle < Mathf.PI / 2)
        {
            if (vAngle < vAngleFirstCoinRectangle) vPositionFromCentre = new Vector2(vTailleLongueur / 2, Mathf.Tan(vAngle) * vTailleLongueur / 2);
            else vPositionFromCentre = new Vector2(vTailleLargeur / (2 * Mathf.Tan(vAngle)), vTailleLargeur / 2);
        }
        else if (vAngle == Mathf.PI / 2) vPositionFromCentre = new Vector2(0, vTailleLargeur / 2);
        else
        {
            if (vAngle < Mathf.PI - vAngleFirstCoinRectangle) vPositionFromCentre = new Vector2(-vTailleLargeur / (2 * Mathf.Tan(vAngle)), vTailleLargeur / 2);
            else vPositionFromCentre = new Vector2(-vTailleLongueur / 2, Mathf.Tan(vAngle) * vTailleLongueur / 2);
        }
        vDistanceToCamera = Vector2.Distance(Vector2.zero, vPositionFromCentre);

        //On met à jour la position
        Vector3 vTargetposition = _cameraTransform.position - vDirectionOnPath.normalized * vDistanceToCamera * 2.5f;
        vTargetposition.z = transform.position.z;
        transform.position = Vector3.SmoothDamp(transform.position, vTargetposition, ref _refVelocityPosition, 1 / _rotationSpeed);
    }


    private void OnTriggerEnter2D(Collider2D pCollider)
    {
        if (pCollider.CompareTag("MeanTube"))
            Destroy(pCollider.transform.gameObject);
    }

}
