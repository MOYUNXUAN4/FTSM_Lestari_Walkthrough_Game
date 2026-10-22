using UnityEngine;

public sealed class FTSMSpin : MonoBehaviour
{
    [SerializeField] private Vector3 axis = Vector3.up;
    [SerializeField] private float speed = 180f;

    private bool spinning = true;

    private void Update()
    {
        if (spinning)
            transform.Rotate(axis, speed * Time.deltaTime, Space.Self);
    }

    public void SetSpinning(bool shouldSpin)
    {
        spinning = shouldSpin;
    }

    public void StopSpinning()
    {
        spinning = false;
    }
}
