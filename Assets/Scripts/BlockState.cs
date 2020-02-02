using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockState {
    public Vector2Int pos;
    public BlockStateEnum stateEnum;
    public bool movable;
    
    public BlockState(int x, int y) {
        this.pos = new Vector2Int(x, y);
        this.stateEnum = BlockStateEnum.ACTIVE;
    }
    
}
