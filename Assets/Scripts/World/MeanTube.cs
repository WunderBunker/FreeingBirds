using UnityEngine;

public class MeanTube : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerShell")) collision.GetComponent<PlayerShellscript>().Die();
    }
}
