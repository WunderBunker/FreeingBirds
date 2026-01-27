using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class BigCage : MonoBehaviour
{
    [SerializeField] int _lifePoints = 5;
    [SerializeField] GameObject _explosionParticles;
    [SerializeField] List<AudioClip> _hitNoises;
    Vector2 _crackOffset;

    bool _isUnvulnerable;
    Vector2 _tiling;

    void Awake()
    {
        _crackOffset = gameObject.GetComponent<SpriteRenderer>().material.GetTextureOffset("_BreachTex");
        _tiling = gameObject.GetComponent<SpriteRenderer>().material.GetTextureScale("_BreachTex");
    }


    public void ChangeCrackTiling(Vector2 pOffset)
    {
        gameObject.GetComponent<SpriteRenderer>().material.SetTextureOffset("_BreachTex", pOffset);
        _crackOffset = pOffset;
    }

    public void LoosePoints(int pValue)
    {
        if (_isUnvulnerable) return;

        Random vRanNoise = new Random();
        AudioManager.Instance.PlaySound(_hitNoises[vRanNoise.Next(0, _hitNoises.Count - 1)], 1f);

        _lifePoints -= pValue;
        StartCoroutine(Tremour());
        ChangeCrackTiling(_crackOffset + _tiling);

        if (_lifePoints <= 0 && GameObject.FindGameObjectWithTag("Player") != null)
            DestroyCage();
    }

    void DestroyCage()
    {
        Instantiate(_explosionParticles, transform.position, Quaternion.identity, transform.parent).GetComponent<ParticleSystem>();
        GameObject.FindGameObjectWithTag("PlayerShell").GetComponent<PlayerShellscript>().DieByCage();
        Destroy(gameObject);
    }

    protected IEnumerator Tremour()
    {
        _isUnvulnerable = true;
        yield return null;
        float vAmplitude = 0.7f;
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
        _isUnvulnerable = false;
    }


}
