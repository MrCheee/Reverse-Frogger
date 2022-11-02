using UnityEngine;

public class VaultRotator : MonoBehaviour
{
    Vector3 _rotateDirection;
    float _rotateAmount;
    float _speed = 90f;
    float _currentRotated = 0f;

    private void Update()
    {
        if (_currentRotated < _rotateAmount)
        {
            float delta = Mathf.Min(_speed * Time.deltaTime, _rotateAmount - _currentRotated);
            _currentRotated += delta;
            Rotate(_rotateDirection, delta);
        }
        else
        {
            _currentRotated = 0;
            _rotateAmount = 0;
        }
    }

    private void Rotate(Vector3 rotateAxis, float rotateAmt)
    {
        transform.RotateAround(transform.position, rotateAxis, rotateAmt);
    }

    public void GiveRotateCommand(Vector3 rotateDirection, float rotateAmt)
    {
        _rotateDirection = rotateDirection;
        _rotateAmount = rotateAmt;
    }
}