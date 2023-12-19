
using System;
using AmmoRacked2.Runtime.Health;
using UnityEngine;

namespace AmmoRacked2.Runtime.Utility
{
    [SelectionBase, DisallowMultipleComponent]
    public class Prop : MonoBehaviour, IDamageable
    {
        public int health;
        public GameObject propSwap;
        public float propMass = 0.02f;
        [Range(0.0f, 4.0f)]
        public float damageKnockbackScale = 0.1f;
        [Range(0.0f, 4.0f)]
        public float collisionKnockbackScale = 2.0f;
        public bool dieOnContact = true;

        private bool destroyed;
        private SinkWithDelay sink;

        private void Awake()
        {
            sink = GetComponent<SinkWithDelay>();
            if (sink) sink.enabled = false;
        }

        public void Damage(DamageArgs args, GameObject invoker, Vector3 point, Vector3 direction)
        {
            if (destroyed) return;
            
            health -= args.damage;

            if (health <= 0.0f)
            {
                Die(args, direction);
            }
        }

        private void Die(DamageArgs args, Vector3 direction)
        {
            if (propSwap)
            {
                var instance = Instantiate(propSwap, transform.position, transform.rotation);
                instance.transform.localScale = transform.localScale;
                
                foreach (var rb in instance.GetComponentsInChildren<Rigidbody>())
                {
                    rb.mass = propMass;
                    rb.AddForce(direction.normalized * args.knockback * damageKnockbackScale, ForceMode.VelocityChange);
                }

                Destroy(gameObject);
            }
            else if (sink) sink.enabled = true;
            else Destroy(gameObject);

            destroyed = true;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!dieOnContact) return;

            DieWithContact(collision.rigidbody, collision.relativeVelocity);
        }

        private void DieWithContact(Rigidbody damager, Vector3 force)
        {
            if (!damager) return;
            
            var damageArgs = new DamageArgs
            {
                damage = health,
                knockback = force.magnitude * collisionKnockbackScale,
            };
            Damage(damageArgs, damager.gameObject, transform.position, force.normalized);
        }

        private void OnTriggerEnter(Collider other)
        {
            var rb = other.GetComponentInParent<Rigidbody>();
            if (!rb) return;
            DieWithContact(rb, rb.velocity);
        }
    }
}