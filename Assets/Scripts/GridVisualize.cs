using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridVisualize : MonoBehaviour
{
    public GameObject tile;

    public void DrawGrid<T>(MyGrid<T> grid) {
        print("drawing grid");
        for (int x = 0; x < grid.GetWidth(); x++) {
            for (int y = 0; y < grid.GetHeight(); y++) {
                AddNewTile(x,y);
            }
        }
    }

    public void AddNewTile(int x, int y) {
        GameObject currentTile = Instantiate(tile, new Vector3(x,y,0f), Quaternion.identity) as GameObject;
        currentTile.name = "Tile at (" + x + ", " + y + ")";
    }
}
