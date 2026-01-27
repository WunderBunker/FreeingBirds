using UnityEngine;

public class HumanModeSwitch : MonoBehaviour
{
    [SerializeField] GameObject _explosionParticles;

    void OnTriggerEnter2D(Collider2D pCollider)
    {
        if (pCollider.CompareTag("Player"))
        {
            Instantiate(_explosionParticles, transform.position, transform.rotation);
            pCollider.GetComponent<Human>().SwitchCamToNextMode();
            Destroy(gameObject);
        }
    }
}
