using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockData : ScriptableObject {
    public Vector2Int size;
    public Vector2Int pos;
    public Color color;
    public BlockTypeEnum type;

    public void Init(Vector2Int aSize, Vector2Int aPos, Color aColor, BlockTypeEnum aType) {
        this.size = aSize;
        this.pos = aPos;
        this.color = aColor;
        this.type = aType;
    }
}