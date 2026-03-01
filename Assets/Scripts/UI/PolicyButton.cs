using UnityEngine;

public class PolicyButton : MonoBehaviour
{
    [SerializeField] string _policyURL = "https://doc-hosting.flycricket.io/freeing-birds-privacy-policy/716da884-fb64-4bfe-9b35-15873e69e7b5/privacy";
    public void OpenPrivacyPolicy()
    {
        Application.OpenURL(_policyURL);
    }
}
