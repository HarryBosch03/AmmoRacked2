
using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AmmoRacked2.Runtime.Utility
{
    [SelectionBase, DisallowMultipleComponent]
    public class SinkWithDelay : MonoBehaviour
    {
        public float initialDelay = 5.0f;
        public float duration = 3.0f;
        public float shakeAmplitude = 0.04f;
        public Vector3 positionOffset = Vector3.down;
        public Vector3 rotationOffset = new(5.0f, 0.0f, 15.0f);

        private void OnEnable()
        {
            StartCoroutine(Sink());
        }

        private IEnumerator Sink()
        {
            yield return new WaitForSeconds(initialDelay);
            
            var startPosition = transform.localPosition;
            var startRotation = transform.localRotation;
            var endPosition = startPosition + positionOffset;
            var endRotation = Quaternion.Euler(rotationOffset) * startRotation;
            
            var t = 0.0f;
            while (t < 1.0f)
            {
                var position = Vector3.Lerp(startPosition, endPosition, t);
                var rotation = Quaternion.Slerp(startRotation, endRotation, t);

                var random = Random.insideUnitCircle;
                position += new Vector3(random.x, 0.0f, random.y) * shakeAmplitude / 50.0f;
                
                transform.localPosition = position;
                transform.localRotation = rotation;

                t += Time.deltaTime / duration;
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}