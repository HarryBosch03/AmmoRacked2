using AmmoRacked2.Runtime.Health;
using UnityEngine;

namespace AmmoRacked2.Runtime.Player
{
    [SelectionBase, DisallowMultipleComponent]
    public class GenericController : MonoBehaviour
    {
        public Tank tank;

        [HideInInspector] public int index;
        [HideInInspector] public Camera mainCamera;

        public static event System.Action<GenericController, Tank, DamageArgs, GameObject, Vector3, Vector3> KillEvent;
        public static event System.Action<GenericController, Tank, DamageArgs, GameObject, Vector3, Vector3> DeathEvent;

        protected virtual void Awake()
        {
            tank = Instantiate(tank);
            tank.gameObject.SetActive(false);

            mainCamera = Camera.main;
        }

        protected virtual void OnEnable()
        {
            Tank.DeathEvent += OnTankDeath;
        }

        protected virtual void OnDisable()
        {
            Tank.DeathEvent -= OnTankDeath;
        }

        protected virtual void OnTankDeath(Tank tank, DamageArgs args, GameObject invoker, Vector3 point, Vector3 direction)
        {
            if (tank == this.tank)
            {
                DeathEvent?.Invoke(this, tank, args, invoker, point, direction);
            }

            if (invoker.gameObject == this.tank.gameObject)
            {
                KillEvent?.Invoke(this, tank, args, invoker, point, direction);
            }
        }

        protected virtual void OnDestroy()
        {
            if (tank) Destroy(tank.gameObject);
        }

        public virtual void SetIndex(int index)
        {
            this.index = index;
        }

        public void SpawnTank(Vector3 spawnPoint)
        {
            tank.gameObject.SetActive(true);
            tank.transform.position = spawnPoint;
        }
    }
}