﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AttackQueueManager : MonoBehaviour {
    public class AttackCommand {
        public EarthElemental.Moves moveName;
        public int[][] coords;
        public AttackCommand(EarthElemental.Moves _moveName, int[][] _coords) {
            moveName = _moveName;
            coords = _coords;
        }
        public AttackCommand(EarthElemental.Moves _moveName) {
            moveName = _moveName;
        }
    }
    public List<AttackCommand> attackCommands = new List<AttackCommand>();
    public BoardManager boardManager;
    public ThrowBoulderSkill rockThrow;
    public PebbleStormSkill pebbleStorm;
    public BoulderDropSkill boulderDrop;
    public CrystalizeSkill crystalize;
    public CrystalBlockSkill crystalBlock;

    public void Queue(AttackCommand command) {
        if (attackCommands.Count >= 2) {
            Debug.LogWarning("Queue is full");
            return;
        }
        if (attackCommands.Count == 0) {
            boardManager.ResetAllIndicators();
            if (command.coords != null) {
                foreach (int[] coord in command.coords) {
                    boardManager.GetTile(coord[0], coord[1]).SetAttackIndicator(true);
                }
            }
        }

        attackCommands.Add(command);
    }

    public IEnumerator ProcessNextAttack() {
        AttackCommand command = DeQueue();
        if (command == null) {
            Debug.LogWarning("No command dequeued");
            yield break;
        }
        if (command.moveName == EarthElemental.Moves.PEBBLESTORM) {
            yield return StartCoroutine(pebbleStorm.CastSkill());
        } else if (command.moveName == EarthElemental.Moves.BOULDERDROP) {
            yield return StartCoroutine(boulderDrop.CastSkill());
        } else if (command.moveName == EarthElemental.Moves.ROCKTHROW) {
            yield return StartCoroutine(rockThrow.CastSkill(command));
        } else if (command.moveName == EarthElemental.Moves.CRYSTALBLOCK) {
            yield return StartCoroutine(crystalBlock.CastSkill());
        } else if (command.moveName == EarthElemental.Moves.CRYSTALIZE) {
            yield return StartCoroutine(crystalize.CastSkill());
        } else {
            Debug.LogWarning("Unknown Attack processed");
        }
    }

    public AttackCommand DeQueue() {
        if (attackCommands.Count <= 0) {
            Debug.LogWarning("No attack command dequeued");
            return null;
        }
        AttackCommand attackCommand = attackCommands.First();
        attackCommands.Remove(attackCommand);
        //boardManager.ResetAllIndicators();
        //foreach (int[] coord in attackCommand.coords) {
        //    boardManager.GetTile(coord[0], coord[1]).SetAttackIndicator(true);
        //}
        return attackCommand;
    }

    public void RefreshIndicators(bool recalculate) {
        if (attackCommands.Count > 0) {
            if (recalculate) {
                UpdateCommand(attackCommands.First());
            } else {
                boardManager.ResetAllIndicators();
                foreach (int[] coord in attackCommands.First().coords) {
                    boardManager.GetTile(coord[0], coord[1]).SetAttackIndicator(true);
                }
                //UpdateIndicators(attackCommands.First());
            }

        }
    }

    void UpdateCommand(AttackCommand command) {
        boardManager.ResetAllIndicators();
        if (command.moveName == EarthElemental.Moves.PEBBLESTORM) {
        } else if (command.moveName == EarthElemental.Moves.BOULDERDROP) {
        } else if (command.moveName == EarthElemental.Moves.ROCKTHROW) {
            command.coords = rockThrow.CalculateTargets();
        } else if (command.moveName == EarthElemental.Moves.CRYSTALBLOCK) {
        } else if (command.moveName == EarthElemental.Moves.CRYSTALIZE) {
        } else {
            Debug.LogWarning("Unknown Attack processed");
        }

        if (command.coords != null) {
            foreach (int[] coord in command.coords) {
                boardManager.GetTile(coord[0], coord[1]).SetAttackIndicator(true);
            }
        }
    }
}
