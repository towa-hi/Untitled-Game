using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyGrid<T> {
    private int width;
    private int height;
    private T[,] gridArray;

    public MyGrid(int width, int height) {
        this.width = width;
        this.height = height;
        gridArray = new T[width, height];
    }

    public int GetWidth() {
        return width;
    }

    public int GetHeight() {
        return height;
    }

    public void SetCell(int x, int y, T value) {
        if (HasCell(x,y)) {
            gridArray[x,y] = value;
        } else {
            //throw exception i guess
            throw new System.ArgumentOutOfRangeException();
        }
    }

    public T GetCell(int x, int y) {
        if (HasCell(x,y)) {
            return gridArray[x,y];
        } else {
            //throw another exception
            throw new System.ArgumentOutOfRangeException();
        }
    }

    public bool HasCell(int x, int y) {
        if (x >= 0 && y >= 0 && x < width && y < height) {
            return true;
        } else {
            return false;
        }
    }
}