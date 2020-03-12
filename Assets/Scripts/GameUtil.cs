using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUtil {

    static double BLOCKHEIGHT = 1.5;

    // convert a Vector2Int point to Vector3 point with 0 as z 
    public static Vector3 V2IToV3(Vector2Int aPos) {
        return new Vector3(aPos.x, aPos.y * (float)GameUtil.BLOCKHEIGHT, 0f);
    }
    // convert a Vector2Int position to corresponding Vector3 position with offset for block size
    public static Vector3 V2IOffsetV3(Vector2Int aSize, Vector2Int aPos) {
        float newX = (float)aPos.x + (float)aSize.x/2;
        float newY = ((float)aPos.y + (float)aSize.y/2) * (float)GameUtil.BLOCKHEIGHT;
        return new Vector3(newX, newY, 0);
    }
    // convert a Vector3 to a Vector2Int ignoring z
    public static Vector2Int V3ToV2I(Vector3 aPos) {
        return new Vector2Int((int)aPos.x, (int)(aPos.y * 1.0/GameUtil.BLOCKHEIGHT));
    }
    //check if a vector2int is within the bounds of two other vector2ints
    public static bool IsInside(Vector2Int aPos, Vector2Int aBotBound, Vector2Int aTopBound) {
        Rect boundingBox = new Rect(aBotBound, aTopBound);
        return boundingBox.Contains(aPos);
    }
}
