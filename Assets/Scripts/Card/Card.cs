﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Card : MonoBehaviour {
    public Entity entity;
    public Sprite basicFrame;
    public Sprite pixFrame;
    public Sprite bossFrame;
    public Image imageDisplay;
    public Image cardFrame;
    public TextMeshProUGUI nameDisplay;
    public TextMeshProUGUI manaDisplay;
    public TextMeshProUGUI attackDisplay;
    public TextMeshProUGUI descriptionDisplay;
    Vector3 startingScale;
    CardEffect effect;
    int manaCost;
    int attack;

    private void Awake() {
        effect = GetComponent<CardEffect>();
    }

    private void Start() {
        startingScale = transform.localScale;
        imageDisplay.sprite = entity.artwork;
        nameDisplay.text = entity.name;
        manaDisplay.text = entity.manaCost.ToString();
        manaCost = entity.manaCost;
        attackDisplay.text = entity.attack.ToString();
        attack = entity.attack;
        descriptionDisplay.text = entity.description;
        if (entity.cardBase == CardBase.Basic) {
            cardFrame.sprite = basicFrame;
        } else if (entity.cardBase == CardBase.Pix) {
            cardFrame.sprite = pixFrame;
        } else if (entity.cardBase == CardBase.Boss) {
            cardFrame.sprite = bossFrame;
        }
    }

    public IEnumerator ActivateEffect() {
        yield return StartCoroutine(effect?.Apply());
        Destroy(gameObject);
    }

    public void SummonAt(Tile tile) {
        if (tile.GetComponentInChildren<Summon>()) {
            Debug.LogWarning("Overwriting summon at column, row: " + tile.column + " " + tile.row);
        }
        Summon summon = Instantiate(entity.summonPrefab, tile.transform).GetComponent<Summon>();
        summon.SetAttack(attack);
        summon.UseHowl();
    }

    public new CardType GetType() {
        return entity.type;
    }

    public Entity GetEntity() {
        return entity;
    }

    public int GetManaCost() {
        return manaCost;
    }

    public void SetManaCost(int value) {
        manaCost = value;
        manaDisplay.text = value.ToString();
    }

    public void AddAttack(int value) {
        attack += value;
        attackDisplay.text = attack.ToString();
    }

    public void SetInteractable(bool value) {
        GetComponent<CanvasGroup>().blocksRaycasts = value;
    }

}
