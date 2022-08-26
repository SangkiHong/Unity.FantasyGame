using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SK.Player;
using SK.FSM;

namespace SK.Manager
{
    public class SceneManager : MonoBehaviour
    {
        [SerializeField] private Player.PlayerController player;

        private Dictionary<int, Enemy> enemies = new Dictionary<int, Enemy>();

        public void AddEnemy(Enemy enemy)
        {
            enemies.Add(enemy.gameObject.GetInstanceID(), enemy);
        }

        public void RemoveEnemy(int instanceID)
        {
            if (enemies.ContainsKey(instanceID))
                enemies.Remove(instanceID);
        }

        private void FixedUpdate()
        {
            player.FixedTick();

            if (enemies.Count > 0)
                foreach (KeyValuePair<int, Enemy> enemy in enemies)
                    enemy.Value.FixedTick();
        }

        private void Update()
        {
            player.Tick();

            if (enemies.Count > 0)
                foreach (KeyValuePair<int, Enemy> enemy in enemies)
                    enemy.Value.Tick();
        }
    }
}
