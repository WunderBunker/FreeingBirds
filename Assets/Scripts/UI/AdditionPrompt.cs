using System.Collections;
using TMPro;
using UnityEngine;

public class AdditionPrompt : MonoBehaviour
{
    [SerializeField] float _fadePace = 0.05f;
    [SerializeField] Color _colorGain = Color.yellow;
    [SerializeField] Color _colorLose = Color.red;

    ParticleSystem _particles;
    TextMeshProUGUI _textMesh;
    bool _isFading;

    // Start is called before the first frame update
    void Awake()
    {
        _textMesh = GetComponent<TextMeshProUGUI>();
        _particles = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_isFading) Fade();
    }


    public IEnumerator AddPoint(int pNumber)
    {
        _textMesh.text = "+" + pNumber.ToString();
        _textMesh.material.SetColor("_OutlineColor", _colorGain);
        _textMesh.material.SetColor("_GlowColor", _colorGain);
        _textMesh.alpha = 1;

        ParticleSystem.MainModule vMain = _particles.main;
        vMain.startColor = _colorGain;
        _particles.Play();

        yield return new WaitForSeconds(0.5f);
        _isFading = true;
    }

    public IEnumerator RemovePoint(int pNumber)
    {
        _textMesh.text = "-" + pNumber.ToString();
        _textMesh.material.SetColor("_OutlineColor", _colorLose);
        _textMesh.material.SetColor("_GlowColor", _colorLose);
        _textMesh.alpha = 1;

        ParticleSystem.MainModule vMain = _particles.main;
        vMain.startColor = _colorLose;
        _particles.Play();

        yield return new WaitForSeconds(0.5f);
        _isFading = true;
    }

    void Fade()
    {
        if (_textMesh.alpha > 0) _textMesh.alpha -= _fadePace;
        else _isFading = false;
    }
}
