using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockData : ScriptableObject {
    public Vector2Int size;
    public BlockTypeEnum type;
    public void Init(int width, int height, BlockTypeEnum type) {
        this.size = new Vector2Int(width, height);
        this.type = type;
    }
}

