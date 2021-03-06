﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowBoulder : MonoBehaviour {
    public AudioSource impactAudio;
    Vector3 startLoc;
    Summoner summoner;
    float duration = 1f;
    public SpriteRenderer spriteR;

    private void Awake() {
        summoner = FindObjectOfType<Summoner>();
    }

    public IEnumerator Attack(Vector3 location, Summon summon) {
        yield return StartCoroutine(MoveRoutine(transform.position, location));
        Color color = spriteR.color;
        color.a = 0;
        spriteR.color = color;
        yield return StartCoroutine(summon.TakeDamage());
        Destroy(gameObject);
    }

    public IEnumerator Attack(Vector3 location, Summoner summoner) {
        yield return StartCoroutine(MoveRoutine(transform.position, location));
        impactAudio.Play();
        Color color = spriteR.color;
        color.a = 0;
        spriteR.color = color;
        yield return StartCoroutine(summoner.TakeDamage(2));
        Destroy(gameObject);
    }

    IEnumerator MoveRoutine(Vector3 start, Vector3 destination) {
        for (float t = 0; t < duration; t += Time.deltaTime) {
            transform.position = Vector3.Lerp(start, destination, Mathf.Min(1, t / duration));
            yield return null;
        }
    }
}
