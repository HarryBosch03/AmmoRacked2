using System;
using AmmoRacked2.Runtime.Health;
using UnityEngine;

namespace AmmoRacked2.Runtime
{
    public class Projectile : MonoBehaviour
    {
        public float baseRadius;
        
        private Vector3 position, velocity, force;
        private DamageArgs damage;

        private GameObject owner;
        private Transform hitFX;
        private Transform detach;

        public Projectile Spawn(GameObject owner, Vector3 position, Vector3 velocity, DamageArgs damage)
        {
            var instance = Instantiate(this, position, Quaternion.LookRotation(velocity, Vector3.up));
            instance.owner = owner;
            instance.damage = damage;
            instance.position = position;
            instance.velocity = velocity;
            return instance;
        }

        private void Awake()
        {
            hitFX = transform.Find("HitFX");
            if (hitFX) hitFX.gameObject.SetActive(false);

            detach = transform.Find("Detach");
        }

        private void FixedUpdate()
        {
            Collide();
            Iterate();
        }

        private void LateUpdate()
        {
            transform.position = position + velocity * (Time.time - Time.fixedTime);
            transform.rotation = Quaternion.LookRotation(velocity, Vector3.up);
        }

        private void Collide()
        {
            var start = position;

            if (Physics.SphereCast(start, baseRadius, velocity, out var hit, velocity.magnitude * Time.deltaTime * 1.1f))
            {
                var damageable = hit.collider.GetComponentInParent<IDamageable>();
                if (damageable != null)
                {
                    damageable.Damage(damage, owner, hit.point, velocity.normalized);
                }
                
                Destroy(hit.point, Vector3.Reflect(velocity, hit.normal));
            }
        }

        private void Destroy(Vector3 position, Vector3 direction)
        {
            if (hitFX)
            {
                hitFX.gameObject.SetActive(true);
                hitFX.SetParent(null);
                hitFX.position = position;
                hitFX.rotation = Quaternion.LookRotation(direction, Vector3.up);
            }

            if (detach)
            {
                detach.SetParent(null);
            }
            
            Destroy(gameObject);
        }

        private void Iterate()
        {
            position += velocity * Time.deltaTime;
            velocity += force * Time.deltaTime;
            
            force = Physics.gravity;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position, baseRadius);
        }
    }
}