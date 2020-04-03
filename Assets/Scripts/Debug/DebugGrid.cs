using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DebugGrid {
    public int width;
    public int height;
    int[,] gridArray;
    TextMesh[,] textArray;
    float cellSizeX;
    float cellSizeY;

    int[,] blankGridArray;

    public DebugGrid(Vector2Int aLevelSize) {
        this.width = aLevelSize.x;
        this.height = aLevelSize.y;
        this.cellSizeX = 1f;
        this.cellSizeY= GameUtil.BLOCKHEIGHT;

        this.gridArray = new int[this.width,this.height];
        this.textArray = new TextMesh[this.width,this.height];
        this.blankGridArray = new int[this.width,this.height];
        for (int x = 0; x < gridArray.GetLength(0); x++) {
            for (int y = 0; y < gridArray.GetLength(1); y++) {
                this.blankGridArray[x,y] = 0;
            }
        }

        for (int x = 0; x < gridArray.GetLength(0); x++) {
            for (int y = 0; y < gridArray.GetLength(1); y++) {
                gridArray[x,y] = 0;
                GameObject gameObject = new GameObject("World_Text", typeof(TextMesh));
                Transform transform = gameObject.transform;
                transform.SetParent(null, false);
                transform.localPosition = GetWorldPosition(x,y);
                TextMesh textMesh = gameObject.GetComponent<TextMesh>();
                textMesh.anchor = TextAnchor.MiddleCenter;
                textMesh.alignment = TextAlignment.Center;
                textMesh.text = gridArray[x,y].ToString();
                textMesh.fontSize = 8;
                this.textArray[x,y] = textMesh;
            }
        }
    }

    private Vector3 GetWorldPosition(int x, int y) {
        return new Vector3(x * cellSizeX + (cellSizeX * 0.5f), y * cellSizeY + (cellSizeY * 0.5f), - 1f);
    }

    public void SetCell(Vector2Int aCell, int aValue) {
        if (gridArray[aCell.x, aCell.y] != aValue) {
            gridArray[aCell.x, aCell.y] = aValue;
            textArray[aCell.x, aCell.y].text = aValue.ToString();
        }
    }

    public void Reset() {
        Array.Copy(this.blankGridArray, this.gridArray, this.gridArray.Length);
        for (int x = 0; x < gridArray.GetLength(0); x++) {
            for (int y = 0; y < gridArray.GetLength(1); y++) {
                int newValue = 0;
                textArray[x, y].text = newValue.ToString();
            }
        }
    }
}
