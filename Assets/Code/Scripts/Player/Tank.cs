using System;
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

        [Space]
        public Renderer[] coloredRenderers = new Renderer[0];

        private int currentHealth;
        
        private Transform model;
        private Vector2 modelLean;
        private Vector2 smoothedLean;
        private float lastFireTime;

        public float Throttle { get; set; }
        public float Turning { get; set; }
        public Vector3 AimPosition { get; set; }
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
            ApplyTurning();
            ApplyTangentFriction();
            TryShoot();

            smoothedLean = Vector2.Lerp(modelLean, smoothedLean, config.leanSmoothing);
        }

        private void TryShoot()
        {
            if (!Shoot) return;
            if (Time.time - lastFireTime < config.fireDelay) return;

            var fwd = muzzle.forward;
            fwd.y = 0.0f;
            fwd.Normalize();
            
            config.projectilePrefab.Spawn(gameObject, muzzle.position, Body.velocity + fwd * config.muzzleSpeed, config.damage, config.projectileGravity);
            
            Body.AddForce(-muzzle.forward * config.recoilForce, ForceMode.Impulse);
            
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
                    body.velocity = Body.velocity + direction * args.knockback;
                    body.angularVelocity = Body.angularVelocity;
                }
            }
            
            gameObject.SetActive(false);
        }

        public void SetColor(Color primaryColor)
        {
            const string key0 = "_HighColor";
            const string key1 = "_LowColor";
            const float scaleDown = 0.4f;

            var propertyBlock = new MaterialPropertyBlock();
            propertyBlock.SetColor(key0, primaryColor);
            
            Color.RGBToHSV(primaryColor, out var h, out _, out var v);
            var secondaryColor = Color.HSVToRGB(h, 1.0f, v * scaleDown);
            propertyBlock.SetColor(key1, secondaryColor);

            foreach (var r in coloredRenderers)
            {
                r.SetPropertyBlock(propertyBlock);
            }
        }
    }
}