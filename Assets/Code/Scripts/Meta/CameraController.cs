using UnityEngine;

namespace AmmoRacked2.Runtime.Meta
{
    public class CameraController : MonoBehaviour
    {
        public Vector3 eulerAngles;
        public float height;
        public float expand;
        public float minSize;
        
        [Range(0.0f, 1.0f)]
        public float smoothing;

        private Vector2 min;
        private Vector2 max;

        private Vector2 smoothedMin;
        private Vector2 smoothedMax;

        private Camera mainCamera;

        private void Awake()
        {
            mainCamera = Camera.main;
        }

        private void FixedUpdate()
        {
            var orientation = Quaternion.Euler(eulerAngles);
            
            var right = orientation * Vector3.right;
            var up = orientation * Vector3.up;
            var forward = orientation * Vector3.forward;
            
            if (GameController.players.Count > 0)
            {
                min.x = float.MaxValue;
                min.y = float.MaxValue;

                max.x = float.MinValue;
                max.y = float.MinValue;

                foreach (var player in GameController.players)
                {
                    var worldPosition = player.tank.Body.position;
                    var cameraPosition = new Vector2(Vector3.Dot(right, worldPosition), Vector3.Dot(up, worldPosition));
                    
                    min.x = Mathf.Min(min.x, cameraPosition.x);
                    min.y = Mathf.Min(min.y, cameraPosition.y);
                    
                    max.x = Mathf.Max(max.x, cameraPosition.x);
                    max.y = Mathf.Max(max.y, cameraPosition.y);
                }

                min -= Vector2.one * expand;
                max += Vector2.one * expand;

                smoothedMin = Vector2.Lerp(min, smoothedMin, smoothing);
                smoothedMax = Vector2.Lerp(max, smoothedMax, smoothing);
            }
            
            var center = (max + min) * 0.5f;
            var size = (max - min);
            var orthoSize = Mathf.Max(size.x * mainCamera.aspect * 0.5f, size.y * 0.5f, minSize);

            var position = right * center.x + up * center.y + forward;
            position += (forward / forward.y) * (height - position.y);

            mainCamera.transform.position = position;
            mainCamera.transform.rotation = orientation;
            mainCamera.orthographicSize = orthoSize;
        }

        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying)
            {
                var orientation = Quaternion.Euler(eulerAngles);
                var trs = Matrix4x4.TRS(orientation * Vector3.forward * -height, orientation, Vector3.one);

                Gizmos.matrix = trs;
                var ortho = minSize;
                var depth = 5.0f;
                Gizmos.DrawWireCube(Vector3.forward * depth * 0.5f, new Vector3(ortho * 16.0f / 9.0f, ortho, depth));
            }
        }
    }
}