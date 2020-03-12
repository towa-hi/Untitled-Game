using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobObject : MonoBehaviour, EntityInterface {
    MobData mobData;
    Vector2Int pos;

    public Vector2Int GetSize() {
        return this.mobData.size;
    }

    public Vector2Int GetPos() {
        return this.pos;
    }

    public bool IsInsideSelf(Vector2Int aPos) {
        
        return GameUtil.IsInside(aPos, this.pos, this.pos + this.mobData.size);
    }
}
