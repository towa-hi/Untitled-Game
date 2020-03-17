using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobObject : MonoBehaviour {
    Vector2Int pos;
    MobData mobData;
    
    public void Init (Vector2Int pos, MobData mobData) {
        this.pos = pos;
        this.mobData = mobData;
    }
}
