using Unity.Mathematics;
using UnityEngine;

public class PlayerShellscript : MonoBehaviour
{
    [SerializeField] GameObject _explosionParticles;
    [SerializeField] public AudioClip _deathNoises;
    [SerializeField] BirdOverride _defaultOverridableValues;

    public PlayerControl _playerControl { get; private set; }
    public GameObject _player { get; private set; }

    public void InstanciatePlayer(Bird pBird)
    {
        _player = GameObject.FindGameObjectWithTag("Player");
        if (_player != null) DestroyImmediate(_player);

        _player = Instantiate(pBird.BirdPrefab, transform.position, quaternion.identity, transform);
        _player.transform.localPosition = Vector3.zero;
        _player.transform.localEulerAngles = Vector3.zero;
        _playerControl = _player.GetComponent<PlayerControl>();

        //On ne maj les propriété de l'oiseau que si on est pas en mode humain (qui gère ça lui-même)
        if (SaveManager.SafeSave.SelectedBirdId != "Bird5")
        {
            _defaultOverridableValues.OverrideProperties();
            pBird.BirdOverride.OverrideProperties();
        }
    }

    public void Die()
    {
        if (PartieManager.Instance.ModeDebug.Contains(DebugModes.Invincible)) return;

        AudioClip vDeathSound = _deathNoises;
        AudioManager.Instance.PlaySound(vDeathSound, 0.7f, null, true);

        ParticleSystem vParticle = Instantiate(_explosionParticles, _player.transform.parent.position, Quaternion.identity, _player.transform.parent).GetComponent<ParticleSystem>();

        StartCoroutine(PartieManager.Instance.KillPlayer(Mathf.Max(vParticle.main.duration + vParticle.main.startLifetime.constantMax, vDeathSound.length)));

        Destroy(_player);
    }

    public void DieByCage()
    {
        if (PartieManager.Instance.ModeDebug.Contains(DebugModes.Invincible)) return;

        AudioClip vDeathSound = _deathNoises;
        AudioManager.Instance.PlaySound(vDeathSound, 0.7f, null, true);

        StartCoroutine(PartieManager.Instance.KillPlayer(vDeathSound.length));
    }
}
