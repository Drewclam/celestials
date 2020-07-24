﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EarthElemental : Boss {
    public enum Moves {
        PEBBLESTORM,
        BOULDERDROP,
        ROCKTHROW,
        CRYSTALBLOCK,
        CRYSTALIZE
    }
    public GameObject boulderDropPrefab;
    public GameObject boulderThrowPrefab;
    public GameObject boulderSpawner;
    public GameObject boulderThrowSpawner;
    public GameObject pebbleStormCardPrefab;
    public GameObject blockingCrystalPrefab;
    public AttackQueueManager attackQueueManager;
    public ThrowBoulderSkill rockThrow;
    public PebbleStormSkill pebbleStorm;
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
            spawnedBlockingCrystal = false;
        }
        yield return StartCoroutine(Attack());
        yield return new WaitUntil(() => DoneActions());
        QueueAttack();
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

    protected override IEnumerator Attack() {
        yield return StartCoroutine(ExecuteNextCommand());
    }

    public override void Initialize() {
        QueueAttack();
        QueueAttack();
    }

    void QueueAttack() {
        Moves randomAttack = Moves.PEBBLESTORM;

        if (randomAttack == Moves.PEBBLESTORM) {
            pebbleStorm.QueueSkill();
        } else if (randomAttack == Moves.BOULDERDROP) {
        } else if (randomAttack == Moves.ROCKTHROW) {
            rockThrow.QueueSkill();
        } else if (randomAttack == Moves.CRYSTALBLOCK) {
        } else if (randomAttack == Moves.CRYSTALIZE) {
        }
    }

    IEnumerator ExecuteNextCommand() {
        yield return StartCoroutine(attackQueueManager.ProcessNextAttack());
    }

    IEnumerator BoulderDrop() {
        animator.SetTrigger("Attack1");
        gameManager.SetWaitForCompletion(true);
        Instantiate(boulderDropPrefab, boulderSpawner.transform);
        yield return new WaitUntil(() => DoneBoulderDrop());
        yield return new WaitUntil(() => DoneActions());
        summoner.TakeDamage(3);
        yield break;
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
