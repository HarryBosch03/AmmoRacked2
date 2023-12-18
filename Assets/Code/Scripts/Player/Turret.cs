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
            currentRotation += tank.TurretInput * Time.deltaTime;
        }

        private void LateUpdate()
        {
            var turret = tank.turret;
            
            turret.rotation = Quaternion.Euler(0.0f, currentRotation, 0.0f);
        }
    }
}