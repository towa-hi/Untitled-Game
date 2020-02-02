using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityData : ScriptableObject {
    public Vector2Int size;
    public EntityTypeEnum type;
    
    public void Init(int width, int height, EntityTypeEnum type) {
        this.size = new Vector2Int(width, height);
        this.type = type;
    }
}
