using UnityEngine;

public class SlidingWindow : MonoBehaviour
{
    protected bool opened;
    protected bool move;
    protected Vector3 closedPosition;
    protected Vector3 openedPosition;
    protected float moveSpeed;

    private void Start()
    {
        opened = false;
        closedPosition = new Vector3(2135, 724, 0); // 219, -26, 0
        openedPosition = new Vector3(1695, 724, 0);
        moveSpeed = 700f;
    }

    private void Update()
    {
        if (move)
        {
            if (opened)
            {
                if (ReachedDestination(openedPosition))
                {
                    move = false;
                }
                else
                {
                    transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);
                    if (transform.position.x < openedPosition.x)
                    {
                        transform.position = openedPosition;
                    }
                }
            }
            else
            {
                if (ReachedDestination(closedPosition))
                {
                    move = false;
                }
                else
                {
                    transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);
                    if (transform.position.x > closedPosition.x)
                    {
                        transform.position = closedPosition;
                    }
                }
            }
        }
    }

    bool ReachedDestination(Vector3 target)
    {
        return Vector3.Distance(transform.position, target) <= 0.5f;
    }

    public void ToggleWindow()
    {
        move = true;
        if (opened)
        {
            opened = false;
        }
        else
        {
            opened = true;
        }
    }

    public void CloseWindow()
    {
        move = true;
        opened = false;
    }
}