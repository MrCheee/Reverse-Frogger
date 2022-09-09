using UnityEngine;

public class BodyCollideTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        gameObject.GetComponentInParent<Enemy>().ChildTriggeredEnter(other);
    }
}