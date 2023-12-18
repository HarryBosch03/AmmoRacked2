using System.Linq;
using AmmoRacked2.Runtime.Player;
using UnityEngine;

namespace AmmoRacked2.Runtime.AI
{
    public class TankAI : GenericController
    {
        private Tank target;

        private void FixedUpdate()
        {
            FindTarget();

            if (target)
            {
                CircleTarget();
            }
        }

        private void CircleTarget()
        {
            var vector = (target.transform.position - tank.transform.position).normalized;

            tank.Throttle = Vector3.Dot(vector, tank.transform.forward);
            tank.Turning = Vector3.Cross(vector, tank.transform.forward).y;
        }

        private void FindTarget()
        {
            if (target && !target.gameObject.activeInHierarchy) return;

            var targets = FindObjectsOfType<Tank>().Where(e => e != tank).ToArray();
            target = targets[Random.Range(0, targets.Length)];
        }
    }
}