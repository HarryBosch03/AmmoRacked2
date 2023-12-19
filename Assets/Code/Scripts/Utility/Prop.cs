
using System;
using AmmoRacked2.Runtime.Health;
using UnityEngine;

namespace AmmoRacked2.Runtime.Utility
{
    [SelectionBase, DisallowMultipleComponent]
    public class Prop : MonoBehaviour, IDamageable
    {
        public int health;
        public bool destroyed;
        public GameObject propSwap;
        
        public SinkWithDelay sink;

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
                    Instantiate(propSwap, transform.position, transform.rotation);
                    Destroy(gameObject);
                }
                else if (sink) sink.enabled = true;
                destroyed = true;
            }
        }
    }
}