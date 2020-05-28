﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthElemental : Boss {
    public GameObject boulderPrefab;
    public GameObject boulderSpawner;
    public GameObject pebbleStormCardPrefab;
    public GameObject blockingCrystalPrefab;
    Summoner summoner;
    GameManager gameManager;
    BoardManager boardManager;
    CardManager cardManager;
    BlockingCrystal blockingCrystal;
    bool attackRoutineRunning = false;
    GameObject blockingCrystalRunTimeReference;
    bool doneSpawningBlockingCrystal = false;
    bool spawnedBlockingCrystal = false;

    public override void Awake() {
        base.Awake();
        summoner = FindObjectOfType<Summoner>();
        gameManager = FindObjectOfType<GameManager>();
        cardManager = FindObjectOfType<CardManager>();
        boardManager = FindObjectOfType<BoardManager>();
    }

    public override void Start() {
        base.Start();
    }

    public override IEnumerator RunTurnRoutine() {
        if (GetIsProtected()) {
            SetIsProtected(false);
        }

        if (spawnedBlockingCrystal) {
            BlockingCrystal blockingCrystal = FindObjectOfType<BlockingCrystal>().GetComponent<BlockingCrystal>();
            yield return StartCoroutine(blockingCrystal.Break());
        }
        yield return StartCoroutine(Attack());
    }

    public override IEnumerator TakeDamage(int damage) {
        if (GetIsProtected()) {
            yield break;
        } else {
            yield return StartCoroutine(base.TakeDamage(damage));
        }
    }

    public void OnAttackAnimationEnd() {
        attackRoutineRunning = false;
    }

    public void OnBlockingCrystalSpawnAnimationEnd() {
        doneSpawningBlockingCrystal = true;
    }
    // rock throw => throw 2 rocks at 2 different lanes, hitting the first summon
    // lane block
    // block a tile for a turn

    protected override IEnumerator Attack() {
        int randomAttack = Random.Range(0, 5);
        ExecuteAttack(3);
        yield break;
    }

    void ExecuteAttack(int randomAttack) {
        attackRoutineRunning = true;
        if (randomAttack == 0) {
            StartCoroutine(PebbleStorm());
        } else if (randomAttack == 1) {
            StartCoroutine(BoulderDrop());
        } else if (randomAttack == 2) {
            RockThrow();
        } else if (randomAttack == 3) {
            StartCoroutine(CrystalBlock());
        } else if (randomAttack == 4) {
            StartCoroutine(Crystalize());
        }
    }

    IEnumerator PebbleStorm() {
        animator.SetTrigger("Attack1");
        cardManager.AddToDeck(Instantiate(pebbleStormCardPrefab).GetComponent<Card>());
        cardManager.AddToDeck(Instantiate(pebbleStormCardPrefab).GetComponent<Card>());
        yield return new WaitUntil(() => DoneActions());
        yield return StartCoroutine(cardManager.DrawToHand());
    }

    IEnumerator BoulderDrop() {
        animator.SetTrigger("Attack1");
        gameManager.SetWaitForCompletion(true);
        Instantiate(boulderPrefab, boulderSpawner.transform);
        yield return new WaitUntil(() => DoneBoulderDrop());
        yield return new WaitUntil(() => DoneActions());
        summoner.TakeDamage();
    }

    void RockThrow() {
        animator.SetTrigger("Attack1");
    }

    IEnumerator CrystalBlock() {
        animator.SetTrigger("Attack2");
        yield return new WaitUntil(() => DoneActions());
        List<Tile> validTiles = boardManager.GetSummonableTiles();
        int randomIndex = Random.Range(0, validTiles.Count);
        Tile tileToBlock = validTiles[randomIndex];
        spawnedBlockingCrystal = true;
        blockingCrystalRunTimeReference = Instantiate(blockingCrystalPrefab, tileToBlock.transform);
        BlockingCrystal blockingCrystal = blockingCrystalRunTimeReference.GetComponent<BlockingCrystal>();
        yield return new WaitUntil(() => !blockingCrystal.GetIsSpawning());
    }

    IEnumerator Crystalize() {
        animator.SetTrigger("Attack3");
        yield return new WaitUntil(() => DoneActions());
        SetIsProtected(true);
    }

    bool DoneActions() {
        return attackRoutineRunning ? false : true;
    }

    bool DoneBoulderDrop() {
        return !gameManager.GetWaitForCompletion();
    }

    bool DoneSpawningBlockingCrystal() {
        return doneSpawningBlockingCrystal;
    }

    bool GetIsProtected() {
        return animator.GetBool("IsProtected");
    }

    void SetIsProtected(bool value) {
        animator.SetBool("IsProtected", value);
    }
}
