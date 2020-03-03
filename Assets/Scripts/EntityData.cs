using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityData : ScriptableObject {
    public Vector2Int size;
    public EntityTypeEnum type;
    
    public void Init(Vector2Int size, EntityTypeEnum type) {
        this.size = size;
        this.type = type;
    }
}
