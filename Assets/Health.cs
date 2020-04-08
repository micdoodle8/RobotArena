using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Health : NetworkBehaviour
{
    [SyncVar]
    private float health;
    public float startHealth = 100.0F;
    private GameObject healthBar;
    private GameObject button;

    void Start() {
        health = startHealth;
        healthBar = transform.Find("HealthBar").gameObject;
        button = transform.Find("PlayerButton").gameObject;
    }

    void Update() {
        if (button.active && healthBar.active) {
            healthBar.SetActive(false);
        } else if (!button.active && !healthBar.active) {
            healthBar.SetActive(true);
        }
        Transform greenBar = healthBar.transform.Find("GreenBar");
        greenBar.localScale = new Vector3(health / startHealth, greenBar.localScale.y, greenBar.localScale.z);
    }

    public void DoDamage(float damage) {
        if (hasAuthority) {
            health -= damage;
            if (health <= 0.0F) {
                health = 0.0F;
                OnDeath();
                Destroy(gameObject);
            }
        }
    }

    private void OnDeath() {
        Object[] connectedPlayers = GameObject.FindObjectsOfType(typeof(ConnectedPlayerManager));
        foreach (Object o in connectedPlayers) {
            ((ConnectedPlayerManager) o).OnRobotDeath(gameObject);
        }
    }
}
