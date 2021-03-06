﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardManager : MonoBehaviour {
    public Summoner summoner;
    Tile[][] grid;
    GameObject tilePrefab;
    Board board;
    Tile[] tiles;
    UIManager uiManager;
    GameManager gameManager;
    CardManager cardManager;
    Player player;
    Hand hand;
    int cardOrder = 0;
    int stageLimit = 3;
    int rowLimit = 3;
    int attackCounter = 0;
    int attacksToWaitFor = 0;
    List<Tile> queue;

    private void Awake() {
        uiManager = FindObjectOfType<UIManager>();
        player = FindObjectOfType<Player>();
        board = GetComponent<Board>();
        tiles = GetComponentsInChildren<Tile>();
        gameManager = FindObjectOfType<GameManager>();
        cardManager = FindObjectOfType<CardManager>();
        hand = FindObjectOfType<Hand>();
        grid = new Tile[4][];
        grid[0] = new Tile[3];
        grid[1] = new Tile[3];
        grid[2] = new Tile[3];
        // boss cells
        grid[3] = new Tile[3];

        queue = new List<Tile>();
    }

    void Start() {
        int tileCounter = 0;
        for (int row = 0; row < rowLimit; row++) {
            for (int stage = 0; stage < stageLimit; stage++) {
                Tile tile = tiles[tileCounter];
                tile.column = stage;
                tile.row = row;
                grid[stage][row] = tile;
                tileCounter++;
            }
        }

        for (int row = 0; row < rowLimit; row++) {
            Tile tile = tiles[tileCounter];
            tile.column = 3;
            tile.row = row;
            grid[3][row] = tile;
            tileCounter++;
        }
    }

    public void ResetAllIndicators() {
        for (int stage = 0; stage < stageLimit; stage++) {
            for (int row = 0; row < rowLimit; row++) {
                grid[stage][row].SetAttackIndicator(false);
            }
        }
    }

    public void DetectSummonableSpace() {
        int stage = 0;
        for (int row = 0; row < rowLimit; row++) {
            Tile tile = grid[stage][row];
            if (tile.IsOccupied()) {
                tile.SetInvalidState();
            } else {
                tile.SetValidState();
            }
        }
    }

    public List<Tile> GetSummonableTiles() {
        List<Tile> tiles = new List<Tile>();
        for (int stage = 0; stage < stageLimit; stage++) {
            for (int row = 0; row < rowLimit; row++) {
                Tile tile = grid[stage][row];
                if (!tile.IsOccupied()) {
                    tiles.Add(tile);
                }
            }
        }

        return tiles;
    }

    public Tile GetTile(int column, int row) {
        if (grid.ElementAtOrDefault(column) != null) {
            if (grid[column].ElementAtOrDefault(row) != null) {
                return grid[column][row];
            }
        }
        return null;
    }

    public List<Tile[]> GetRandomRows(int maxNum) {
        Tile[][] gridClone = grid.Select(row => (Tile[])row.Clone()).ToArray();
        List<int> randomList = new List<int>();
        for (int i = 0; i < maxNum; i++) {
            int numToAdd = UnityEngine.Random.Range(0, maxNum + 1);
            while (randomList.Contains(numToAdd)) {
                numToAdd = UnityEngine.Random.Range(0, maxNum + 1);
            }
            randomList.Add(numToAdd);
        }

        List<Tile[]> tiles = new List<Tile[]>();
        foreach (int num in randomList) {
            Tile[] row = new Tile[rowLimit];
            for (int i = 0; i < stageLimit; i++) {
                row[i] = gridClone[i][num];
            }
            tiles.Add(row);
        }
        return tiles;
    }

    public Tile[] GetFirstSummonInRows() {
        // index is row
        List<Tile> tiles = new List<Tile>();
        // prepopulate with the first tiles
        tiles.Add(grid[0][0]);
        tiles.Add(grid[0][1]);
        tiles.Add(grid[0][2]);
        for (int stage = 0; stage < stageLimit; stage++) {
            for (int row = 0; row < rowLimit; row++) {
                Tile tile = grid[stage][row];
                if (tile && tile.IsOccupied()) {
                    tiles[row] = tile;
                }
            }
        }

        return tiles.ToArray();
    }

    public void SetNeutral() {
        foreach (Tile tile in tiles) {
            tile.SetNeutralState();
        }
    }

    public void DetectMoveableSummons() {
        foreach (Tile tile in tiles) {
            if (tile.GetSummon() && GetDestination(tile.GetSummon().GetId(), 1)) { //hardcode movement 1
                tile.SetValidState();
            } else {
                tile.SetInvalidState();
            }
        }
    }

    public void DetectSummons() {
        foreach (Tile tile in tiles) {
            if (tile.GetSummon()) {
                tile.SetValidState();
            } else {
                tile.SetInvalidState();
            }
        }
    }

    public void DetectSummonsExcluding(int id) {
        foreach (Tile tile in tiles) {
            if (tile.GetSummon() && tile.GetSummon().GetId() != id) {
                tile.SetValidState();
            } else {
                tile.SetInvalidState();
            }
        }
    }

    public Summon GetRandomSummonInStage(int stage) {
        List<Summon> summons = new List<Summon>();
        foreach (Tile tile in grid[stage]) {
            if (tile.IsOccupied()) {
                summons.Add(tile.GetSummon());
            }
        }
        if (summons.Count < 1) {
            return null;
        }
        return summons[UnityEngine.Random.Range(0, summons.Count)];
    }

    public void HandleSummoned() {
        foreach (Tile tile in tiles) {
            tile.SetNeutralState();
        }
        uiManager.SetLocationSelectionPrompt(false);
    }

    public void IncrementAttackCounter() {
        attackCounter += 1;
    }

    public void IncrementAttacksToWaitFor() {
        attacksToWaitFor += 1;
    }

    public void AddToQueue(Tile tile) {
        queue.Add(tile);
    }

    public List<Tile> GetQueue() {
        return queue;
    }

    public void ClearQueue() {
        queue.Clear();
    }

    // TODO: Move to Card Manager
    public void PlayCard(Card card) {
        CardType type = card.GetType();
        if (type == CardType.Summon) {
            StartCoroutine(PlaySummon(card));
        } else if (type == CardType.Spell) {
            StartCoroutine(PlaySpell(card));
        } else {
            Debug.Log("Unknown cardtype encountered: " + type);
        }
    }

    public Tile GetDestination(int summonId, int offset) {
        Tile currentTile = GetCurrentTile(summonId);
        return Array.Find(tiles, (Tile tile) => {
            if (currentTile && tile.row == currentTile.row && tile.column == currentTile.column + offset) {
                if (!tile.IsOccupied()) {
                    return true;
                }
            }
            return false;
        });
    }

    public Tile GetFirstTileInRow(int summonId) {
        Tile currentTile = GetCurrentTile(summonId);
        if (currentTile == null) {
            return null;
        }
        return grid[0][currentTile.row];
    }

    public IEnumerator ResolveStagesRoutine() {
        for (int i = stageLimit - 1; i >= 0; i--) {
            yield return StartCoroutine(ResolveAbilitiesForStage(i));
        }

        for (int i = stageLimit - 1; i >= 0; i--) {
            yield return StartCoroutine(ResolveAttacksForStage(i));
            yield return StartCoroutine(ResolveMovementForStage(i));
        }
    }

    public IEnumerator ResolveTurnForSummon(Summon summon) {
        if (!summon) {
            yield break;
        }
        yield return StartCoroutine(ResolveAbilityForSummon(summon));
        yield return StartCoroutine(ResolveAttackForSummon(summon));
        yield return StartCoroutine(ResolveMovementForSummon(summon));
    }
    
    Tile GetCurrentTile(int id) {
        return Array.Find(tiles, (Tile tile) => {
            Summon summon = tile.GetComponentInChildren<Summon>();
            if (summon) {
                if (summon.GetId() == id) {
                    return true;
                }
            }
            return false;
        });
    }

    IEnumerator PlaySummon(Card card) {
        uiManager.SetLocationSelectionPrompt(true);
        DetectSummonableSpace();
        yield return new WaitUntil(() => GetQueue().Count == 1);
        uiManager.SetLocationSelectionPrompt(false);
        Tile tile = GetQueue()[0];
        SetNeutral();
        summoner.Summon(tile);
        yield return new WaitUntil(() => summoner.GetDone());
        card.SummonAt(tile);
        tile.UpdateIndicatorPosition();
        yield return StartCoroutine(player.LoseMana(card.GetManaCost()));
        ClearQueue();
        cardManager.AddToDiscard(card);
        Destroy(card.gameObject);
    }

    IEnumerator PlaySpell(Card card) {
        summoner.CastSpell();
        yield return new WaitUntil(() => summoner.GetAnimationDone());
        yield return StartCoroutine(player.LoseMana(card.GetManaCost()));
        yield return StartCoroutine(card.ActivateEffect());
        if (card) {
            cardManager.AddToDiscard(card);
        }
    }

    //IEnumerator StageRoutine(int stageIndex) {
    //    yield return StartCoroutine(ResolveAbilitiesForStage(stageIndex));
    //    yield return StartCoroutine(ResolveAttacksForStage(stageIndex));
    //    yield return StartCoroutine(ResolveMovementForStage(stageIndex));
    //}

    IEnumerator ResolveMovementForSummon(Summon summon) {
        summon.Walk();
        yield return new WaitWhile(() => summon.DoneMoving());
    }

    IEnumerator ResolveAttackForSummon(Summon summon) {
        summon.Attack();
        yield return new WaitUntil(() => summon.DoneAttacking());
    }

    IEnumerator ResolveAbilityForSummon(Summon summon) {
        summon.UsePower();
        yield return new WaitUntil(() => summon.DonePower());
    }

    IEnumerator ResolveMovementForStage(int stageIndex) {
        for (int rowIndex = 0; rowIndex < rowLimit; rowIndex++) {
            Tile tile = grid[stageIndex][rowIndex];
            Summon summon = tile.GetComponentInChildren<Summon>();
            summon?.Walk();
        }

        yield return new WaitWhile(() => Array.Find(tiles, (Tile tile2) => {
            Summon summon = tile2.GetComponentInChildren<Summon>();
            return summon && !summon.DoneMoving() ? true : false;
        }));
    }

    IEnumerator ResolveAttacksForStage(int stageIndex) {
        Tile[] tilesInStage = new Tile[stageLimit];
        Array.Copy(grid[stageIndex], tilesInStage, stageLimit);
        System.Array.Sort(tilesInStage, (tileX, tileY) => {
            Summon summonX = tileX.GetComponentInChildren<Summon>();
            Summon summonY = tileY.GetComponentInChildren<Summon>();
            int x, y;
            x = summonX ? summonX.GetOrder() : -1;
            y = summonY ? summonY.GetOrder() : -1;
            return x - y;
        });

        foreach (Tile tile in tilesInStage) {
            Summon summon = tile.GetComponentInChildren<Summon>();
            if (summon != null) {
                summon.Attack();
                yield return new WaitUntil(() => summon.DoneAttacking());
            }
        }
    }

    IEnumerator ResolveAbilitiesForStage(int stageIndex) {
        Tile[] tilesInStage = new Tile[stageLimit];
        Array.Copy(grid[stageIndex], tilesInStage, stageLimit);
        System.Array.Sort(tilesInStage, (tileX, tileY) => {
            Summon summonX = tileX.GetComponentInChildren<Summon>();
            Summon summonY = tileY.GetComponentInChildren<Summon>();
            int x, y;
            x = summonX ? summonX.GetOrder() : -1;
            y = summonY ? summonY.GetOrder() : -1;
            return x - y;
        });

        foreach (Tile tile in tilesInStage) {
            Summon summon = tile.GetComponentInChildren<Summon>();
            if (summon != null) {
                summon.UsePower();
                yield return new WaitUntil(() => summon.DonePower());
            }
        }
    }
}
