using UnityEngine;

namespace Utilities
{
    public class Rotate_Image : MonoBehaviour
    {
        [SerializeField] private float _rotationSpeed = 0.5f;

        private void Update()
        {
            transform.rotation *= Quaternion.AngleAxis(
                -_rotationSpeed,
                Vector3.forward
            );
        }
    }
}