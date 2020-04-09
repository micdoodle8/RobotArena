using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Health : NetworkBehaviour
{
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
    }

    public void DoDamage(float damage) {
        if (!isServer) {
            return;
        }

        health -= damage;
        RpcHealthChanged(health);
        if (health <= 0.0F) {
            health = 0.0F;
            Destroy(gameObject);
            OnDeath();
        }
    }

    // [Command]
    // private void CmdUpdateHealth(float health) {
    //     this.health = health;
    //     RpcUpdateHealth(health);
    // }

    [ClientRpc]
    void RpcHealthChanged(float newHealth) {
        health = newHealth;
        if (health <= 0.0F) {
            Destroy(gameObject);
            OnDeath();
        } else {
            Transform greenBar = healthBar.transform.Find("GreenBar");
            greenBar.localScale = new Vector3(health / startHealth, greenBar.localScale.y, greenBar.localScale.z);
        }
    }

    // private void RpcUpdateHealth(float health) {
    //     if (!this.hasAuthority) {
    //         this.health = health;
    //     }
    // }

    private void OnDeath() {
        Object[] connectedPlayers = GameObject.FindObjectsOfType(typeof(ConnectedPlayerManager));
        foreach (Object o in connectedPlayers) {
            ((ConnectedPlayerManager) o).OnRobotDeath(gameObject);
        }
    }

    public float GetHealth() {
        return health;
    }
}
