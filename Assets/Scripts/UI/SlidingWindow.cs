using UnityEngine;

public class SlidingWindow : MonoBehaviour
{
    protected bool opened;
    protected Vector3 closedPosition;
    protected Vector3 openedPosition;
    protected float moveSpeed;

    private void Start()
    {
        opened = false;
        closedPosition = new Vector3(2139, 514, 0); // 219, -26, 0
        openedPosition = new Vector3(1702, 514, 0);
        moveSpeed = 700f;
    }

    private void Update()
    {
        if (opened)
        {
            if (!ReachedDestination(openedPosition))
            {
                transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);
            }
        }
        else
        {
            if (!ReachedDestination(closedPosition))
            {
                transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);
            }
        }
    }

    bool ReachedDestination(Vector3 target)
    {
        return Vector3.Distance(transform.position, target) <= 1f;
    }

    public void OpenLogWindow()
    {
        opened = true;
    }

    public void CloseLogWindow()
    {
        opened = false;
    }
}