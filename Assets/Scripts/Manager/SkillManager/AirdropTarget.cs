using UnityEngine;

public class AirdropTarget : MonoBehaviour
{
    [SerializeField] GameObject LandingSprite;
    [SerializeField] AudioSource LandingSound;

    public void Trigger(string vehType, Vector3 position, float laneDirection)
    {
        if (vehType == "Truck")
        {
            LandingSprite.transform.localPosition = new Vector3(2.2f * laneDirection, 5, -0.5f);
            LandingSprite.transform.localScale = new Vector3(8, 3, 5);
        }
        else if (vehType == "Bus")
        {
            LandingSprite.transform.localPosition = new Vector3(4.5f * laneDirection, 5, -0.5f);
            LandingSprite.transform.localScale = new Vector3(11, 3.5f, 5);
        } else
        {
            LandingSprite.transform.localPosition = new Vector3(0, 5, 0);
            LandingSprite.transform.localScale = new Vector3(3, 3, 5);
        }

        transform.position = position;
        LandingSound.Play();
        LandingSprite.GetComponent<Animator>().Play("LandingTarget", -1, 0f);
    }
}