using UnityEngine;

namespace AmmoRacked2.Runtime.Player
{
    [SelectionBase, DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public class Tank : MonoBehaviour
    {
        public TankSettings config;
        public Transform turret0;
        public Transform turret1;
        
        private Transform model;
        private Vector2 modelLean;
        private Vector2 smoothedLean;

        public float Throttle { get; set; }
        public float Turning { get; set; }
        public Vector2 TurretInput { get; set; }
        
        public Rigidbody Body { get; private set; }
        
        private void Awake()
        {
            Body = GetComponent<Rigidbody>();
            Body.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            Body.maxAngularVelocity = float.MaxValue;

            model = transform.Find("Model");
        }

        private void FixedUpdate()
        {
            ApplyThrottle();
            ApplyTurning();
            ApplyTangentFriction();
            TurnTurret();

            smoothedLean = Vector2.Lerp(modelLean, smoothedLean, config.leanSmoothing);
        }

        private void TurnTurret()
        {
            
        }

        private void LateUpdate()
        {
            var lean = smoothedLean * config.leanScale;
            model.localRotation = Quaternion.Euler(lean.y, 0.0f, lean.x);
        }

        private void ApplyTangentFriction()
        {
            var delta = Vector3.Dot(transform.right, Body.velocity) * config.tangentialFriction;
            modelLean.x = delta / Time.deltaTime;
            
            Body.velocity -= transform.right * delta;
        }

        private void ApplyThrottle()
        {
            var input = Mathf.Clamp(Throttle, -1.0f, 1.0f);

            var normal = transform.forward;
            var target = input * config.moveSpeed;
            var current = Vector3.Dot(normal, Body.velocity);
            
            var force = (target - current) * 2.0f / config.moveAccelerationTime;
            modelLean.y = force;
            Body.AddForce(normal * force, ForceMode.Acceleration);
        }
        
        private void ApplyTurning()
        {
            var input = Mathf.Clamp(Turning, -1.0f, 1.0f);

            var normal = transform.up;
            var target = input * config.turnSpeedDegrees * Mathf.Deg2Rad;
            var current = Vector3.Dot(normal, Body.angularVelocity);
            
            var torque = (target - current) * 2.0f / config.turnAccelerationTime;
            Body.AddTorque(Vector3.up * torque, ForceMode.Acceleration);
        }
    }
}