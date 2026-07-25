using UnityEngine;

// i give up
public class Billboard : MonoBehaviour
{
    private Camera _cam;

    private void Start()
    {
        _cam = Camera.main;
    }

    private void LateUpdate()
    {
        Vector3 offset = _cam.transform.position - transform.position;
        transform.rotation = Quaternion.LookRotation(offset, Vector3.up);
    }
}
