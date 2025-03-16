using UnityEngine;
using UnityEngine.Pool;
namespace TDS.Zombie
{
    public class ZombieRoot : MonoBehaviour
    {
        [SerializeField] private ZombieMoveController _moveController;
        public IObjectPool<GameObject> Pool { get; private set; }

        public void Initialize(IObjectPool<GameObject> pool)
        {
            Pool = pool;
        }

        public void Reset()
        {
            _moveController.ResetState();
        }
    }
}
