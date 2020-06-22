﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EarthElemental : Boss {
    enum Moves {
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
    Summoner summoner;
    GameManager gameManager;
    BoardManager boardManager;
    CardManager cardManager;
    BlockingCrystal blockingCrystal;
    bool attackRoutineRunning = false;
    GameObject blockingCrystalRunTimeReference;
    bool doneSpawningBlockingCrystal = false;
    bool spawnedBlockingCrystal = false;
    List<Moves> attackQueue = new List<Moves>();
    List<Tile> tilesToAttack = new List<Tile>();
    EarthElementalSkillIndicators skillIndicators;

    public override void Awake() {
        base.Awake();
        summoner = FindObjectOfType<Summoner>();
        gameManager = FindObjectOfType<GameManager>();
        cardManager = FindObjectOfType<CardManager>();
        boardManager = FindObjectOfType<BoardManager>();
        skillIndicators = GetComponent<EarthElementalSkillIndicators>();
    }

    public override void Start() {
        base.Start();
        QueueAttack();
        QueueAttack();
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
        QueueAttack();
        yield return new WaitUntil(() => DoneActions());
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
        int randomAttack = Random.Range(0, 5);
        if (attackQueue.Count > 0) {
            yield return ExecuteAttack(attackQueue.First());
            attackQueue.RemoveAt(0);
        } else {
            yield return ExecuteAttack((Moves)randomAttack);
        }

        skillIndicators.ClearIndicator();
    }

    IEnumerator ExecuteAttack(Moves randomAttack) {
        attackRoutineRunning = true;
        if (randomAttack == Moves.PEBBLESTORM) {
            yield return StartCoroutine(PebbleStorm());
        } else if (randomAttack == Moves.BOULDERDROP) {
            yield return StartCoroutine(BoulderDrop());
        } else if (randomAttack == Moves.ROCKTHROW) {
            yield return StartCoroutine(RockThrow());
        } else if (randomAttack == Moves.CRYSTALBLOCK) {
            yield return StartCoroutine(CrystalBlock());
        } else if (randomAttack == Moves.CRYSTALIZE) {
            yield return StartCoroutine(Crystalize());
        }
    }

    void QueueAttack() {
        foreach (Tile tile in tilesToAttack) {
            tile.HideAttackIndicator();
        }
        int randomAttack = Random.Range(0, 5);
        Moves nextAttack = /*(Moves)randomAttack*/ Moves.ROCKTHROW;
        attackQueue.Add(nextAttack);
        if (nextAttack == Moves.PEBBLESTORM) {
            skillIndicators.SetPebbleStorm();
        } else if (nextAttack == Moves.BOULDERDROP) {
            skillIndicators.SetBoulderDrop();
        } else if (nextAttack == Moves.ROCKTHROW) {
            QueueBoulderThrowTargets();
            skillIndicators.SetBoulderThrow();
            for (int i = 0; i < 2; i++) {
                tilesToAttack[i].ShowAttackIndicator();
            }
        } else if (nextAttack == Moves.CRYSTALBLOCK) {
            skillIndicators.SetBlockingCrystal();
        } else if (nextAttack == Moves.CRYSTALIZE) {
            skillIndicators.SetCrystalize();
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
        Instantiate(boulderDropPrefab, boulderSpawner.transform);
        yield return new WaitUntil(() => DoneBoulderDrop());
        yield return new WaitUntil(() => DoneActions());
        summoner.TakeDamage(3);
        yield break;
    }

    void QueueBoulderThrowTargets() {
        int numberOfTargets = 2;
        List<Tile> randomList = boardManager.GetFirstSummonInRows().OrderBy(tile => Random.Range(0f, 1f)).ToList();
        for (int i = 0; i < numberOfTargets; i++) {
            tilesToAttack.Add(randomList[i]);
            //randomList[i].ShowAttackIndicator();
        }
    }

    IEnumerator RockThrow() {
        animator.SetTrigger("Attack1");
        yield return new WaitUntil(() => DoneActions());
        List<Tile> targets = new List<Tile>();
        targets.Add(TakeFromTileQueue());
        targets.Add(TakeFromTileQueue());
        foreach (Tile tile in targets) {
            GameObject boulder = Instantiate(boulderThrowPrefab, boulderThrowSpawner.transform);
            Summon summon = tile.GetSummon();
            if (summon) {
                yield return StartCoroutine(boulder.GetComponent<ThrowBoulder>().Attack(summon.transform.position, summon));
                Debug.Log("done attacking summon");
            } else {
                yield return StartCoroutine(boulder.GetComponent<ThrowBoulder>().Attack(summoner.transform.position, summoner));
            }
        }
    }

    Tile TakeFromTileQueue() {
        Tile tile = tilesToAttack.First();
        if (!tile) {
            Debug.LogError("Tile Queue was empty");
        }
        tilesToAttack.Remove(tile);
        return tile;
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
