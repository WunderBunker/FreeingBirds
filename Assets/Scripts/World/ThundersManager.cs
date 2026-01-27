using UnityEngine;
using Random = System.Random;

public class ThundersManager : MonoBehaviour
{
    [SerializeField] int[] _valuesForThunder = { 20, 80 };
    [SerializeField] float _horizonAvancementForMaxSpawn = 5000;
    [SerializeField] float _thunderTryPeriod = 3;
    [SerializeField][Range(-1, 1)] float _parallax = 0.2f;
    [SerializeField][Range(0, 10)] float _speed ;
    [SerializeField] AudioClip _thunderNoise;

    GameObject _camera;
    PartieManager _partieManager;
    Animator _animator;

    float _lastThunderTryTime;
    Vector3 _lastCameraPosition;


    // Start is called before the first frame update
    void Start()
    {
        _partieManager = PartieManager.Instance.GetComponent<PartieManager>();
        _camera = GameObject.FindGameObjectWithTag("MainCamera");
        _animator = gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(_partieManager._partieState != PartieState.InGame && _partieManager._partieState != PartieState.PartieStarted) return;

        //On voit pour déclencher ou non un éclair
        if (Time.time - _lastThunderTryTime >= _thunderTryPeriod)
        {
            PlayRandomThunder();
            _lastThunderTryTime = Time.time;
        }

        //On déplace l'obj par rapport aux mouvements de la caméra en fonction du parallax
        Vector3 vMouvementCam = _camera.transform.position - _lastCameraPosition;
        Vector3 vDeplacementBasic = Vector3.right * _speed * Time.deltaTime;
        
        transform.position += vDeplacementBasic - vMouvementCam * _parallax;
        
        _lastCameraPosition = _camera.transform.position;
    }

    void PlayRandomThunder()
    {
        float vAvancementProportionnel = _partieManager._avancement / _horizonAvancementForMaxSpawn;

        Random vRanMustHaveThunder = new Random();
        float vValueForThunder = Mathf.Lerp(_valuesForThunder[0], _valuesForThunder[1], vAvancementProportionnel);
        float vRandomValue = vRanMustHaveThunder.Next(1, 100);
        if (vRandomValue <= vValueForThunder)
        {
            transform.position = _camera.transform.position + new Vector3(0, _camera.GetComponent<Camera>().orthographicSize, _camera.GetComponent<Camera>().nearClipPlane) ;
            AudioManager.Instance.PlaySound(_thunderNoise, 1, transform.position);

            Random vRanWichThunder = new Random();
            vRandomValue = vRanWichThunder.Next(1, 3);
            switch (vRandomValue)
            {
                case 1:
                    _animator.SetTrigger("Thunder1");
                    break;
                case 2:
                    _animator.SetTrigger("Thunder2");
                    break;
                case 3:
                    _animator.SetTrigger("Thunder3");
                    break;
            }
        }

    }
}
