using AmmoRacked2.Runtime.Health;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AmmoRacked2.Runtime.Player
{
    [SelectionBase, DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public class Tank : MonoBehaviour, IDamageable
    {
        public TankSettings config;
        public GameObject deadTankPrefab;
        public Transform turret;
        public Transform muzzle;

        private int currentHealth;
        
        private Transform model;
        private Vector2 modelLean;
        private Vector2 smoothedLean;
        private float lastFireTime;

        public float LeftThrottle { get; set; }
        public float RightThrottle { get; set; }
        public float TurretInput { get; set; }
        public bool Shoot { get; set; }
        
        public Rigidbody Body { get; private set; }

        public static event System.Action<Tank, DamageArgs, GameObject, Vector3, Vector3> DamageEvent;
        public static event System.Action<Tank, DamageArgs, GameObject, Vector3, Vector3> DeathEvent;
        
        private void Awake()
        {
            Body = GetComponent<Rigidbody>();
            Body.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            Body.maxAngularVelocity = float.MaxValue;

            model = transform.Find("Model");

            foreach (var t in GetComponentsInChildren<Transform>())
            {
                t.gameObject.layer = 8;
            }
        }

        private void OnEnable()
        {
            currentHealth = config.maxHealth;
        }

        private void FixedUpdate()
        {
            ApplyThrottle();
            ApplyTangentFriction();
            TryShoot();

            smoothedLean = Vector2.Lerp(modelLean, smoothedLean, config.leanSmoothing);
            Shoot = false;
        }

        private void TryShoot()
        {
            if (!Shoot) return;
            if (Time.time - lastFireTime < config.fireDelay) return;

            var fwd = muzzle.forward;
            fwd.y = 0.0f;
            fwd.Normalize();
            
            config.projectilePrefab.Spawn(gameObject, muzzle.position, fwd * config.muzzleSpeed, config.damage);
            
            lastFireTime = Time.time;
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
            ApplyThrottle(1);
            ApplyThrottle(-1);
        }
        
        private void ApplyThrottle(int sign)
        {
            var input = Mathf.Clamp(sign == 1 ? RightThrottle : LeftThrottle, -1.0f, 1.0f);

            var normal = transform.forward;
            var target = input * config.moveSpeed;

            var point = transform.position + transform.right * config.turnSpeed * sign;
            var velocity = Body.GetPointVelocity(point);
            var current = Vector3.Dot(normal, velocity);
            
            var force = (target - current) * 2.0f / config.moveAccelerationTime;
            modelLean.y = force;
            Body.AddForceAtPosition(normal * force, point, ForceMode.Acceleration);
        }
        
        public void Damage(DamageArgs args, GameObject invoker, Vector3 point, Vector3 direction)
        {
            DamageEvent?.Invoke(this, args, invoker, point, direction);
            
            currentHealth -= args.damage;
            Body.AddForce(direction.normalized * args.knockback, ForceMode.Impulse);

            if (currentHealth <= 0)
            {
                Die(args, invoker, point, direction);
            }
        }

        private void Die(DamageArgs args, GameObject invoker, Vector3 point, Vector3 direction)
        {
            DeathEvent?.Invoke(this, args, invoker, point, direction);
            
            if (deadTankPrefab)
            {
                var instance = Instantiate(deadTankPrefab, transform.position, transform.rotation);
                var body = instance.GetComponent<Rigidbody>();
                if (body)
                {
                    body.velocity = this.Body.velocity;
                    body.angularVelocity = this.Body.angularVelocity;
                }
            }
            
            gameObject.SetActive(false);
        }
    }
}