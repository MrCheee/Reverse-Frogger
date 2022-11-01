using UnityEngine;

public class RotateCommand : Command
{
    Vector3 _rotateDirection;
    float _rotateAmount;
    float _speed;
    float _currentRotated = 0f;

    public RotateCommand(Vector3 rotateDirection, float rotateAmt, float rotateSpeed)
    {
        _rotateDirection = rotateDirection;
        _rotateAmount = rotateAmt;
        _speed = rotateSpeed;
    }

    public override void Execute(Unit unit)
    {
        if (isFinished) return;
        if (_currentRotated < _rotateAmount)
        {
            float delta = Mathf.Min(_speed * Time.deltaTime, _rotateAmount - _currentRotated);

            _currentRotated += delta;
            unit.Rotate(_rotateDirection, delta);
        } 
        else
        {
            isFinished = true;
        }
    }
}