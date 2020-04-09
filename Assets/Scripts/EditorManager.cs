using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorManager : Singleton<EditorManager> {
    public LevelData levelData;
    public List<BlockObject> blockList;
    public List<BlockObject> selectedList;
    public List<MobObject> mobList;
    
    public Vector3 mousePos;
    public Vector3 oldMousePos;
    
    void Awake() {
        
    }
}
