
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
                if (propSwap)
                {
                    var instance = Instantiate(propSwap, transform.position, transform.rotation);
                    foreach (var rb in instance.GetComponentsInChildren<Rigidbody>())
                    {
                        rb.AddForce(direction.normalized * args.knockback, ForceMode.Impulse);
                    }
                    
                    Destroy(gameObject);
                }
                else if (sink) sink.enabled = true;
                else Destroy(gameObject);
                destroyed = true;
            }
        }
    }
}