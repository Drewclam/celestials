﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pebble : CardEffect {
    CardManager cardManager;

    private void Awake() {
        cardManager = FindObjectOfType<CardManager>();
    }

    public override IEnumerator Apply() {
        cardManager.TrashCard(GetComponent<Card>());
        yield break;
    }
}
