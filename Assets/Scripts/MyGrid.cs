using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyGrid : Singleton<MyGrid> {
    public GameObject gridCellMaster;
    public Dictionary<Vector2Int, MyGridCell> cellDict;
    // Start is called before the first frame update
    void Start() {
        cellDict = new Dictionary<Vector2Int, MyGridCell>();
        for (int x = 0; x < BoardManager.Instance.levelData.boardSize.x; x++) {
            for (int y = 0; y < BoardManager.Instance.levelData.boardSize.y; y++) {
                Vector2Int currentPos = new Vector2Int(x, y);
                GameObject cellObject = Instantiate(gridCellMaster, GameUtil.V2IOffsetV3(new Vector2Int(1,1), currentPos) + new Vector3(0, 0, -1.01f), Quaternion.Euler(-90, 0, 0), this.transform);
                MyGridCell cell = cellObject.GetComponent<MyGridCell>();
                cellDict[currentPos] = cell;
            }
        }
    }

    public void SetCell(Vector2Int aPos, Color aColor) {
        cellDict[aPos].SetActive(true);
        cellDict[aPos].SetColor(aColor);
    }

    public void ResetCell(Vector2Int aPos) {
        cellDict[aPos].SetActive(false);
        cellDict[aPos].SetColor(Color.white);
    }
}
