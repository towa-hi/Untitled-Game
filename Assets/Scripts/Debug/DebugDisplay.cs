using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugDisplay : MonoBehaviour {
    DebugGrid grid;
    void Start() {
        this.grid = new DebugGrid(BoardManager.Instance.levelData.boardSize);

    }

    void Update() {
        for (int x = 0; x < grid.width; x++) {
            for (int y = 0; y < grid.height; y++) {
                Vector2Int checkPos = new Vector2Int(x,y);
                BlockObject maybeABlock = BoardManager.GetBlockOnPosition(checkPos);
                if (maybeABlock != null) {
                    this.grid.SetCell(checkPos, 1);
                } else {
                    this.grid.SetCell(checkPos, 0);
                }
            }
        }
    }
}
