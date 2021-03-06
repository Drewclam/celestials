﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarchEffect : SpellEffect {
    Summon summon;

    private void Awake() {
        summon = GetComponentInParent<Summon>();
    }

    public override IEnumerator Activate() {
        yield return StartCoroutine(base.Activate());
        summon.Attack();
        yield return new WaitUntil(() => summon.DoneAttacking());
        summon.Walk();
        yield return new WaitUntil(() => summon.DoneMoving());
        gameObject.SetActive(false);
        base.Deactivate();
    }
}
