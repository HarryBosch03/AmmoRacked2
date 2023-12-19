using UnityEngine;

namespace AmmoRacked2.Runtime.Player
{
    public class Turret : MonoBehaviour
    {
        private Tank tank;

        private float currentRotation;
        
        public TankSettings Config => tank.config;
        
        private void Awake()
        {
            tank = GetComponentInParent<Tank>();
        }

        private void Update()
        {
            if (Config.lockTurret || !tank.turret)
            {
                currentRotation = 0.0f;
                return;
            }
            
            var current = tank.turret.up;
            var target = tank.AimPosition - tank.transform.position;
            
            if (target.magnitude < 0.1f) return;
            
            current.Normalize();
            target.Normalize();

            var a0 = Mathf.Atan2(current.x, current.z);
            var a1 = Mathf.Atan2(target.x, target.z);
            var da = Mathf.DeltaAngle(a0 * Mathf.Rad2Deg, a1 * Mathf.Rad2Deg);
            
            currentRotation += Mathf.Clamp(da * Config.turretTurnAcceleration, -1.0f, 1.0f) * Config.turretTurnSpeed * Time.deltaTime;
        }

        private void LateUpdate()
        {
            var turret = tank.turret;
            if (turret) turret.localRotation = Quaternion.Euler(0.0f, 0.0f, currentRotation);
        }
    }
}