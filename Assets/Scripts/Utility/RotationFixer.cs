using UnityEngine;

public class RotationFixer : MonoBehaviour
{

    [SerializeField] Vector3 _angles;

    // Update is called once per frame
    void Update()
    {
        transform.eulerAngles = _angles;
    }
}
