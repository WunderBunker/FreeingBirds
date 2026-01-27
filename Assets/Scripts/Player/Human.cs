using System;
using UnityEngine;

public class Human : MonoBehaviour
{
    [SerializeField] float _switchModePace = 100;
    [SerializeField] BirdOverride _defaultOverridableValues;
    [SerializeField] BirdList _birds;
    [SerializeField] GameObject _modeSwitchPrefab;

    public bool _mustSpawnSwitch { get; private set; }

    float _lastSwitchAvancement;

    enum Mode { Bird, Camel, Lion, Dragon }

    Mode _mapCurrentMode;
    Mode _camCurrentMode;
    private PlayerControl vHumanControl;

    void Awake()
    {
        vHumanControl = GetComponent<PlayerControl>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _lastSwitchAvancement = 0;

        _mapCurrentMode = Mode.Dragon;
        _camCurrentMode = Mode.Dragon;
        SwitchMapToNextMode();
        SwitchCamToNextMode();
    }

    // Update is called once per frame
    void Update()
    {
        if (PartieManager.Instance._avancement - _lastSwitchAvancement >= _switchModePace)
        {
            _lastSwitchAvancement = PartieManager.Instance._avancement;
            _mustSpawnSwitch = true;
        }
    }

    public void SpawnModeSwitch(Vector3 pPosition, Vector2 pUpDirection)
    {
        _mustSpawnSwitch = false;
        SwitchMapToNextMode();

        GameObject vSwitch = Instantiate(_modeSwitchPrefab, pPosition, Quaternion.identity);
        float vAngle = Vector2.SignedAngle(Vector2.up, pUpDirection);
        vSwitch.transform.eulerAngles = new Vector3(vSwitch.transform.rotation.x, vSwitch.transform.rotation.y, vAngle);
    }

    void SwitchMapToNextMode()
    {
        Action<int> aSwitchMode = (int pModeIndex) =>
        {
            _defaultOverridableValues.MapOveride();
            _defaultOverridableValues.SpawnsOverride();
            _birds?.List[pModeIndex].BirdOverride.MapOveride();
            _birds?.List[pModeIndex != 3 ? pModeIndex : 4].BirdOverride.SpawnsOverride();
        };

        _mapCurrentMode = (_mapCurrentMode != Mode.Dragon) ? (_mapCurrentMode + 1) : Mode.Bird;
        //vho _mapCurrentMode = Mode.Dragon;

        switch (_mapCurrentMode)
        {
            case Mode.Bird:
                aSwitchMode(0);
                break;
            case Mode.Camel:
                aSwitchMode(1);
                break;
            case Mode.Lion:
                aSwitchMode(2);
                break;
            case Mode.Dragon:
                aSwitchMode(3);
                break;
            default:
                break;
        }
    }

    public void SwitchCamToNextMode()
    {
        Action<int> aSwitchMode = (int pModeIndex) =>
        {
            _defaultOverridableValues.CamAndPlayerOverride(true);
            _birds?.List[pModeIndex != 3 ? pModeIndex : 4].BirdOverride.CamAndPlayerOverride(true);

            ApplyModeBirdControl(_birds.List[pModeIndex != 3 ? pModeIndex : 4].BirdPrefab);

            PartieManager.Instance.ChangeDecor(_birds.List[pModeIndex]);

            GameObject.FindGameObjectWithTag("SpawnManager").GetComponent<Bird5SpawnManager>().enabled = pModeIndex == 3;
        };

        _camCurrentMode = (_camCurrentMode != Mode.Dragon) ? (_camCurrentMode + 1) : Mode.Bird;
        //vho _mapCurrentMode = Mode.Dragon;
        switch (_camCurrentMode)
        {
            case Mode.Bird:
                aSwitchMode(0);
                break;
            case Mode.Camel:
                aSwitchMode(1);
                break;
            case Mode.Lion:
                aSwitchMode(2);
                break;
            case Mode.Dragon:
                aSwitchMode(3);
                break;
            default:
                break;
        }
    }

    void ApplyModeBirdControl(GameObject pBirdPrefab)
    {
        vHumanControl.ApplyOtherControlParameter(pBirdPrefab.GetComponent<PlayerControl>());

        var vTempBird = Instantiate(pBirdPrefab, transform.parent);
        vHumanControl.GetComponent<ParticleSystem>().Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        Tools.CopyComponent(vTempBird.GetComponent<ParticleSystem>(), gameObject, null);
        Destroy(gameObject.transform.Find("FlapTrail").gameObject);
        Instantiate(vTempBird.transform.Find("FlapTrail").gameObject, transform).name = "FlapTrail";
        Destroy(vTempBird);

        GetComponent<SpriteRenderer>().sprite = pBirdPrefab.GetComponent<SpriteRenderer>().sprite;
        GetComponent<Animator>().runtimeAnimatorController = pBirdPrefab.GetComponent<Animator>().runtimeAnimatorController;
        GetComponent<BoxCollider2D>().size = pBirdPrefab.GetComponent<BoxCollider2D>().size;
        GetComponent<BoxCollider2D>().offset = pBirdPrefab.GetComponent<BoxCollider2D>().offset;
        transform.localScale = pBirdPrefab.transform.localScale;
        vHumanControl.PlayerSize = GetComponent<BoxCollider2D>().size * transform.localScale;
    }
}
