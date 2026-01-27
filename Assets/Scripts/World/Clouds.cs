using UnityEngine;

public class Clouds : MonoBehaviour
{
    [SerializeField][Range(-1, 1)] float _parallax = 0.3f;
    [SerializeField][Range(0, 10)] float _speed = 8;

    PolygonCollider2D _pollygon;
    Transform _cameraTransform;
    Vector3 _lastCameraPosition;
    ColliderToMesh _colliderToMesh;


    // Start is called before the first frame update
    void Awake()
    {
        _cameraTransform = GameObject.FindGameObjectWithTag("MainCamera").transform;
        _pollygon = gameObject.GetComponent<PolygonCollider2D>();
        _colliderToMesh = gameObject.GetComponent<ColliderToMesh>();
        _lastCameraPosition = _cameraTransform.position;

        InitSize();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 vMouvementCam = _cameraTransform.position - _lastCameraPosition;
        Vector3 vDeplacementBasic = Vector3.right * _speed * Time.deltaTime;

        //On déploace l'obj par rapport aux mouvements de la caméra en fonction du parallax
        transform.position += vDeplacementBasic - vMouvementCam * _parallax;

        UpdateSize(vMouvementCam, vDeplacementBasic);
        _lastCameraPosition = _cameraTransform.position;
    }

    void UpdateSize(Vector3 pCamMove, Vector2 pDeplacementBasic)
    {
        Vector2[] vNewPoints = new Vector2[4];

        vNewPoints[0] = _pollygon.points[0] + (Vector2)pCamMove - pDeplacementBasic + (Vector2)pCamMove * _parallax;
        vNewPoints[1] = _pollygon.points[1] + (Vector2)pCamMove - pDeplacementBasic + (Vector2)pCamMove * _parallax;
        vNewPoints[2] = _pollygon.points[2] + (Vector2)pCamMove - pDeplacementBasic + (Vector2)pCamMove * _parallax;
        vNewPoints[3] = _pollygon.points[3] + (Vector2)pCamMove - pDeplacementBasic + (Vector2)pCamMove * _parallax;

        _pollygon.SetPath(0, vNewPoints);

        _colliderToMesh.MajMesh();
    }

    void InitSize()
    {
        Vector2[] vNewPoints = new Vector2[4];

        Vector2 vCamPos = (Vector2)_cameraTransform.position;
        vNewPoints[0] = _pollygon.points[0] + vCamPos;
        vNewPoints[1] = _pollygon.points[1] + vCamPos;
        vNewPoints[2] = _pollygon.points[2] + vCamPos;
        vNewPoints[3] = _pollygon.points[3] + vCamPos;

        _pollygon.SetPath(0, vNewPoints);

        _colliderToMesh.MajMesh();
    }

}
