﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectIndicator : MonoBehaviour {
    public SpriteRenderer spriteR;
    Vector3 origPos;

    private void Start() {
        origPos = transform.position;
    }

    public bool GetIsTargeted() {
        return spriteR.enabled;
    }

    public void SetIndicator(bool value) {
        Vector3 newPos = GetPositionRelativeToSummon();
        transform.position = newPos;
        spriteR.enabled = value;
    }

    public void RefreshPosition() {
        transform.position = GetPositionRelativeToSummon();
    }

    Vector3 GetPositionRelativeToSummon() {
        Summon summon = transform.parent.GetComponentInChildren<Summon>();
        if (summon) {
            SpriteRenderer summonSpriteR = summon.GetComponent<SpriteRenderer>();
            //float summonHeight = summonSpriteR.bounds.max.y - summonSpriteR.bounds.min.y;
            Vector3 indicatorPosition = summon.transform.Find("Head Effect").transform.position;
            return new Vector3(origPos.x, indicatorPosition.y + 0.2f);
        }
        return origPos;
    }
}
