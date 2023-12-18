using UnityEngine;

namespace AmmoRacked2.Runtime.Player
{
    public class Turret : MonoBehaviour
    {
        private Tank tank;

        private Vector2 currentRotation;

        public TankSettings Config => tank.config;
        
        private void Awake()
        {
            tank = GetComponentInParent<Tank>();
        }

        private void FixedUpdate()
        {
            currentRotation += tank.TurretInput * Config.turnSensitivity * Time.deltaTime;
        }

        private void LateUpdate()
        {
            var turret0 = tank.turret0;
            var turret1 = tank.turret1;
            
            turret0.rotation = Quaternion.Euler(0.0f, currentRotation.x, 0.0f);
            turret1.rotation = Quaternion.Euler(currentRotation.y, currentRotation.x, 0.0f);
        }
    }
}