using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUtil {
    // convert a Vector2Int point to Vector3 point with 0 as z 
    public static Vector3 V2IToV3(Vector2Int gridVector) {
        return new Vector3(gridVector.x, gridVector.y, 0f);
    }
    // convert a Vector2Int position to corresponding Vector3 position with offset for block size
    public static Vector3 V2IOffsetV3(Vector2Int size, Vector2Int pos) {
        return new Vector3((float)size.x/2 + pos.x, (float)size.y/2 + pos.y, 0);
    }
    // convert a Vector3 to a Vector2Int ignoring z
    public static Vector2Int V3ToV2I(Vector3 pos) {
        return new Vector2Int((int)pos.x, (int)pos.y);
    }
}
