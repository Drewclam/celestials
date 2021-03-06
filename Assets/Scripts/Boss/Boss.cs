﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : MonoBehaviour {
    public AudioSource hurtAudio;
    public GameObject projectileCollision;
    public Healthbar healthBar;
    public BossStatusEffectManager statusEffects;

    protected int health;
    protected SpriteRenderer spriteRenderer;
    protected Animator animator;
    protected Color color;
    protected bool skipTurn = false;

    public virtual void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    public virtual void Start() {
        health = 30;
        color = spriteRenderer.color;
    }

    public IEnumerator SetupBoss() {
        yield break;
    }

    public int getHealth() {
        return health;
    }

    public virtual IEnumerator RunTurnRoutine() {
        yield break;
    }

    public Transform GetTransformToHit() {
        return projectileCollision.transform;
    }

    public virtual void Initialize() {
    }

    public void Stasis() {
        skipTurn = true;
        statusEffects.EnableStasis();
    }

    public virtual IEnumerator TakeDamage(int damage) {
        hurtAudio.Play();
        health -= damage;
        healthBar.SetHealth(health);
        yield return StartCoroutine(FlashRed());
        if (health < 0) {
            Debug.Log("Implement win scneario");
        }
    }

    public void EnableStasis() {
        statusEffects.EnableStasis();
    }

    public void DisableStasis() {
        statusEffects.DisableStasis();
    }

    protected virtual IEnumerator Attack() {
        yield break;
    }

    IEnumerator FlashRed() {
        int duration = 2;
        for (int t = 0; t < duration; t++) {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.1f);
        }
        spriteRenderer.color = color;
        yield break;
    }
}
