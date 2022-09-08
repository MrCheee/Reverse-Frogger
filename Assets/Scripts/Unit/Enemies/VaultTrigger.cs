using UnityEngine;

public class VaultTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        gameObject.GetComponentInParent<Vaulter>().VaultHit();
        Destroy(gameObject);
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}