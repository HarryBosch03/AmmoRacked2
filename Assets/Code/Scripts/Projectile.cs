
using System;
using AmmoRacked2.Runtime.Health;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AmmoRacked2.Runtime
{
    public class Projectile : MonoBehaviour
    {
        private Vector3 position, velocity, force;
        private DamageArgs damage;

        private GameObject owner;

        public Projectile Spawn(GameObject owner, Vector3 position, Vector3 velocity, DamageArgs damage)
        {
            var instance = Instantiate(this);
            instance.owner = owner;
            instance.damage = damage;
            instance.position = position;
            instance.velocity = velocity;
            return instance;
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
            var end = start + velocity * Time.deltaTime * 1.1f;

            if (Physics.Linecast(start, end, out var hit))
            {
                var damageable = hit.collider.GetComponentInParent<IDamageable>();
                if (damageable != null)
                {
                    damageable.Damage(damage, owner, hit.point, velocity.normalized);
                }

                Destroy();
            }
        }

        private void Destroy()
        {
            Destroy(gameObject);
        }

        private void Iterate()
        {
            position += velocity * Time.deltaTime;
            velocity += force * Time.deltaTime;
            
            force = Physics.gravity;
        }
    }
}