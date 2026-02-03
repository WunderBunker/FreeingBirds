using UnityEngine;

public class DragonPointsManager : MonoBehaviour
{
    [SerializeField] GameObject _bigCagePrefab;
    GameObject _bigCage;

    // Start is called before the first frame update
    void Start()
    {
        Camera vCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        Transform vCameraTransform = vCamera.transform;
        
        _bigCage = Instantiate(_bigCagePrefab, vCameraTransform.localPosition, Quaternion.identity, vCameraTransform);
        _bigCage.transform.position = vCamera.ViewportToWorldPoint(new Vector3(1, 0.3f, vCamera.nearClipPlane + 1));
    }

    void Update()
    {
        if (_bigCage != null && _bigCage.transform.parent.name == "Main Camera" && PartieManager.Instance._partieState == PartieState.PartieStarted)
            _bigCage.transform.SetParent(GameObject.FindGameObjectWithTag("Map").transform);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("BirdSpawn"))
        {
            PartieManager.Instance.AddPonctualAvancement(collision.gameObject.GetComponent<Bird1Spawn>()._advancementToGain);
            collision.gameObject.GetComponent<Bird1Spawn>().GetCaptured();
        }
    }

    void OnDestroy()
    {

        if (PartieManager.Instance._partieState != PartieState.PlayerIsDying && _bigCage != null)
        {
            Destroy(_bigCage);
        }
    }
}
