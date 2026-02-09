using System.Collections;
using System.Linq;
using UnityEngine;

public class HumanModeSwitch : MonoBehaviour
{
    [SerializeField] GameObject _explosionParticles;
    [SerializeField] float _explosionTime;

    float _explosionStartTime;
    float _initCoreW;
    float _initIntensity;
    float _initAlpha;

    bool _hasCollided;

    void Awake()
    {
        _initCoreW = transform.Find("Core").localScale.x;
        _initIntensity = transform.Find("Core").GetComponent<SpriteRenderer>().material.GetFloat("_Intensity");
        _initAlpha = transform.Find("Core").GetComponent<SpriteRenderer>().material.GetFloat("_Alpha");
    }

    void OnTriggerEnter2D(Collider2D pCollider)
    {
        if (_hasCollided) return;
        if (pCollider.CompareTag("Player"))
        {
            _hasCollided = true;
            Instantiate(_explosionParticles, transform.position, transform.rotation);
            pCollider.GetComponent<Human>().SwitchCamToNextMode();
            StartCoroutine(ExplosionAnim());
        }
    }

    IEnumerator ExplosionAnim()
    {
        var vTrailPart = transform.Find("TrailParticles").GetComponent<ParticleSystem>();
        var vSimplePart = transform.Find("SimpleParticles").GetComponent<ParticleSystem>();

        ParticleSystem.Particle[] vParticles = new ParticleSystem.Particle[vTrailPart.main.maxParticles];
        for (int i = 0; i < vParticles.Length; i++) vParticles[i].remainingLifetime = 0.2f;
        vTrailPart.SetParticles(vParticles);

        vParticles = new ParticleSystem.Particle[vSimplePart.main.maxParticles];
        for (int i = 0; i < vParticles.Length; i++) vParticles[i].remainingLifetime = 0.2f;
        vSimplePart.SetParticles(vParticles);

        vTrailPart.Stop();
        vSimplePart.Stop();

        GameObject.FindGameObjectWithTag("MainCanvas").transform.Find("FlashScreen").gameObject.SetActive(true);

        _explosionStartTime = Time.time;

        float vSpentTime = Time.time - _explosionStartTime;
        Transform vCore = transform.Find("Core");
        Material vMaterial = vCore.GetComponent<SpriteRenderer>().material;

        while (vSpentTime < _explosionTime)
        {

            float vCoef = vSpentTime / _explosionTime;

            Vector3 vScoreScale = vCore.localScale;
            vScoreScale.x = Mathf.Lerp(_initCoreW, _initCoreW * 10, vSpentTime / _explosionTime);
            transform.Find("Core").localScale = vScoreScale;

            vMaterial.SetFloat("_Intensity", Mathf.Lerp(_initIntensity, _initIntensity * 3, vCoef * 2));
            vMaterial.SetFloat("_Alpha", Mathf.Lerp(_initAlpha, 0, vCoef));

            vSpentTime = Time.time - _explosionStartTime;
            yield return null;
        }

        GameObject.FindGameObjectWithTag("MainCanvas").transform.Find("FlashScreen").gameObject.SetActive(false);

        Destroy(gameObject);
    }
}
