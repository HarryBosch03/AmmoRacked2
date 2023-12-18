using UnityEngine;

namespace AmmoRacked2.Runtime.Health
{
    public interface IDamageable
    {
        void Damage(DamageArgs args, GameObject invoker, Vector3 point, Vector3 direction);
    }
}