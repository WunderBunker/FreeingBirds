using UnityEngine;

public class Rain : MonoBehaviour
{
    [SerializeField] AudioClip gRainSound;
    [SerializeField] bool gIsImpactingGround;
    [SerializeField] LayerMask gPolyA;
    [SerializeField] LayerMask gPolyB;

    Transform gPlayerTransform;
    private int _rainSoundToken;

    // Start is called before the first frame update
    void Start()
    {
        var vParticleShape = GetComponent<ParticleSystem>().shape;
        vParticleShape.radius = GameObject.Find("Main Camera").GetComponent<Camera>().orthographicSize * 3.5f;

        PartieManager.Instance.GetComponent<PartieManager>().GetInGame += StartRain;

        gPlayerTransform = GameObject.FindGameObjectWithTag("PlayerShell").transform;
    }

    void StartRain()
    {
        gameObject.GetComponent<ParticleSystem>().Play();
        _rainSoundToken = AudioManager.Instance.PlayKeepSound(gRainSound, 0.2f, null, null, true);
    }
    void StopRain()
    {
        gameObject.GetComponent<ParticleSystem>().Stop();
        if (_rainSoundToken > 0) AudioManager.Instance.StopKeepSound(_rainSoundToken);
    }

    void Update()
    {
        if (gIsImpactingGround)
        {
            PlayerControl vPlayerControl = gPlayerTransform.GetComponentInChildren<PlayerControl>();
            if (vPlayerControl == null) return;

            Vector3 vDirectionPath = vPlayerControl._directionOnPath;

            float vPathAngle = Mathf.Abs(Vector2.SignedAngle(Vector2.right, vDirectionPath));

            ParticleSystem.CollisionModule vCollisionModule = gameObject.GetComponent<ParticleSystem>().collision;
            if (vPathAngle < 90) vCollisionModule.collidesWith = gPolyB;
            else vCollisionModule.collidesWith = gPolyA;
        }
    }

    void OnDestroy()
    {
        StopRain();
        if (PartieManager.Instance) PartieManager.Instance.GetInGame -= StartRain;
    }
}
