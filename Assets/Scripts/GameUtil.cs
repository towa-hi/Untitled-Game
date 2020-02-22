using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUtil {

    static double BLOCKHEIGHT = 1.5;

    // convert a Vector2Int point to Vector3 point with 0 as z 
    public static Vector3 V2IToV3(Vector2Int gridVector) {
        return new Vector3(gridVector.x, gridVector.y * (float)GameUtil.BLOCKHEIGHT, 0f);
    }
    // convert a Vector2Int position to corresponding Vector3 position with offset for block size
    public static Vector3 V2IOffsetV3(Vector2Int size, Vector2Int pos) {
        float newX = (float)pos.x + (float)size.x/2;
        float newY = ((float)pos.y + (float)size.y/2) * (float)GameUtil.BLOCKHEIGHT;
        return new Vector3(newX, newY, 0);
    }
    // convert a Vector3 to a Vector2Int ignoring z
    public static Vector2Int V3ToV2I(Vector3 pos) {
        return new Vector2Int((int)pos.x, (int)(pos.y * 1.0/GameUtil.BLOCKHEIGHT));
    }
}
