using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class AchievementNotif : MonoBehaviour
{
    [SerializeField] float _animTime = 2;
    [SerializeField] float _amplitude = 100;
    [SerializeField] AnimationCurve _curve = AnimationCurve.EaseInOut(0, 0, 1, 0);
    [SerializeField] AudioClip _notifSound;

    RectTransform _rectTransform;
    Vector3 _initPosition;
    ParticleSystem _particles;
    bool _mustAnim;
    float _startAnimTime;
    bool _animIsDown;
    bool _mustSendNotif;
    TextMeshProUGUI _textMesh;

    List<Achievement> _notifsList = new();

    bool _canPlayEffects;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _textMesh = GetComponentInChildren<TextMeshProUGUI>();
        _initPosition = _rectTransform.anchoredPosition;
        _particles = GetComponentInChildren<ParticleSystem>();
    }

    void Update()
    {
        if (_mustAnim)
        {
            float vAvancement = _curve.Evaluate((Time.time - _startAnimTime) / (_animTime / 2));
            if (_animIsDown) vAvancement = 1 - vAvancement;

            float _YPos = math.lerp(_initPosition.y, _initPosition.y + _amplitude, vAvancement);
            _rectTransform.anchoredPosition = new Vector3(_rectTransform.anchoredPosition.x, _YPos);

            if (vAvancement >= 0.8f && _canPlayEffects)
            {
                _particles.Play();
                AudioManager.Instance.PlaySound(_notifSound, 1);
                _canPlayEffects = false;
            }

            if ((Time.time - _startAnimTime) >= _animTime / 2)
            {
                if (!_animIsDown)
                {
                    _animIsDown = true;
                    _startAnimTime = Time.time;
                }
                else
                {
                    _rectTransform.anchoredPosition = _initPosition;
                    _mustAnim = false;
                    _animIsDown = false;
                }
            }
        }
        else if (_mustSendNotif) InitNotif();
    }

    public void SendAchievNotif(Achievement pAchievement)
    {
        _notifsList.Add(pAchievement);
        _mustSendNotif = true;
    }

    void InitNotif()
    {
        if (_notifsList.Count == 0)
        {
            _mustSendNotif = false;
            return;
        }

        _rectTransform.anchoredPosition = _initPosition;
        _mustAnim = true;
        _startAnimTime = Time.time;

        _notifsList[0].LocalText.StringChanged += (string pText)=>{ _textMesh.text = pText; };

        if (_notifsList[0].LocalArg.Length > 0)
        {
            _notifsList[0].LocalText.Arguments = new object[_notifsList[0].LocalArg.Length];

            foreach (var lArg in _notifsList[0].LocalArg)
            {
                if (lArg.LocalizedString.TableReference != null)
                {
                    lArg.LocalizedString.StringChanged += (string pText) =>
                    {
                        _notifsList[0].LocalText.Arguments[lArg.Index] = pText;
                        _notifsList[0].LocalText.RefreshString();
                    };
                    lArg.LocalizedString.RefreshString();
                }
                else _notifsList[0].LocalText.Arguments[lArg.Index] = lArg.String;
            }
        }




        _notifsList.RemoveAt(0);
        _mustSendNotif = _notifsList.Count > 0;

        _canPlayEffects = true;
    }

}
