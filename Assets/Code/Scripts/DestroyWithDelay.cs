using UnityEngine;

namespace AmmoRacked2.Runtime
{
    public sealed class DestroyWithDelay : MonoBehaviour
    {
        public float delay;
        
        private void Start()
        {
            Destroy(gameObject, delay);
        }
    }
}