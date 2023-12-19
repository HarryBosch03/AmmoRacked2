
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
        [Range(0.0f, 1.0f)]
        public float knockbackScale = 0.1f;
        public bool dieOnContact;

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
                foreach (var rb in instance.GetComponentsInChildren<Rigidbody>())
                {
                    rb.AddForce(direction.normalized * args.knockback * knockbackScale, ForceMode.VelocityChange);
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
            
            var damageArgs = new DamageArgs
            {
                damage = health,
                knockback = collision.relativeVelocity.magnitude,
            };
            Damage(damageArgs, collision.rigidbody.gameObject, transform.position, collision.relativeVelocity.normalized);
        }
    }
}