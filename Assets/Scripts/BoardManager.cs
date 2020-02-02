
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BoardManager : MonoBehaviour {
    public LevelData levelData;
    public int strokes = 0;
    public List<BlockObject> blockList;
    // List<EntityObject> entityList;

    public List<BlockObject> selectedList;
    public BlockObject selectedBlock;
    public MouseStateEnum mouseState = MouseStateEnum.DEFAULT;
    public Vector3 mousePos;
    public Vector3 clickedPosition;

    //set by editor
    public GameObject markerMaster;
    public BlockObject blockObjectMaster;
    public GameObject background;

    void Update() {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray,out hit, Mathf.Infinity)) {
            mousePos = new Vector3(hit.point.x, hit.point.y, hit.point.z);
        }
        if (Input.GetMouseButtonDown(0)) {
            //if first time mouse clicked
            if  (mouseState == MouseStateEnum.DEFAULT) {
                mouseState = MouseStateEnum.CLICKED;
                clickedPosition = mousePos;
                selectedBlock = GetBlockOnPosition(GameUtil.V3ToV2I(mousePos));
            } 
        } else if (Input.GetMouseButtonUp(0)) {
            //if not clicked
            mouseState = MouseStateEnum.DEFAULT;
        }
        switch (mouseState) {
            case MouseStateEnum.DEFAULT:
                selectedBlock = null;
                UnHighlightSelected();
                selectedList.Clear();
                break;
            case MouseStateEnum.CLICKED:
                if (mousePos.y < clickedPosition.y - 0.5) {
                    //dragging down
                    mouseState = MouseStateEnum.HOLDING;
                    selectedList.Add(selectedBlock);
                    //replace this with a recursive function later
                    selectedList.AddRange(GetBelowBlocks(selectedBlock));
                    HighlightSelected();
                } else if (mousePos.y > clickedPosition.y + 0.5) {
                    //dragging up
                    mouseState = MouseStateEnum.HOLDING;
                    selectedList.Add(selectedBlock);
                    //replace this with a recursive function later
                    selectedList.AddRange(GetAboveBlocks(selectedBlock));
                    HighlightSelected();
                }
                break;
            case MouseStateEnum.HOLDING:
                break;
        }
    }

    void HighlightSelected() {
        foreach (BlockObject block in selectedList) {
            block.Highlight();
        }
    }

    void UnHighlightSelected() {
        foreach (BlockObject block in selectedList) {
            block.UnHighlight();
        }
    }
    void Awake() {
        CreateMarkers();
        levelData = LevelData.GenerateTestLevel();          // make test level
        LoadLevelData(levelData);
        // DestroyMarkers();
    }

    void Start() {
        print("started");
        //blockList[0].Move(new Vector2Int(4,4));
    }

    void CreateMarkers() {
        for (int x = 0; x < 10; x++) {
            for (int y = 0; y < 10; y++) {
                GameObject marker = Instantiate(markerMaster, new Vector3(x,y,0f), Quaternion.identity, transform);
                marker.name = "(" + marker.transform.position.x + ", " + marker.transform.position.y + ")";
            }
        }
    }

    void DestroyMarkers() {
        GameObject[] destroyThis = GameObject.FindGameObjectsWithTag("Marker");
        foreach (GameObject marker in destroyThis) {
            GameObject.Destroy(marker);
        }
    }

    void LoadLevelData(LevelData levelData) {
        foreach (KeyValuePair<BlockData, BlockState> pair in levelData.blockDataDict) {
            blockList.Add(CreateBlockObject(pair.Key, pair.Value));
        }
    }

    BlockObject CreateBlockObject(BlockData blockData, BlockState blockState) {
        BlockObject newBlockObject = Instantiate(blockObjectMaster, GameUtil.V2IOffsetV3(blockData.size, blockState.pos), Quaternion.identity);
        newBlockObject.Init(blockData, blockState);
        return newBlockObject;
    }

    public BlockObject GetBlockOnPosition(Vector2Int pos) {
        foreach (BlockObject block in blockList) {
            if (block.CheckSelfPos(pos)) {
                return block;
            }
        }
        return null;
    }

    public List<BlockObject> GetAboveBlocks(BlockObject block) {
        HashSet<BlockObject> aboveBlockSet = new HashSet<BlockObject>();
        for (int x = block.pos.x; x < block.pos.x + block.blockData.size.x; x++) {
            Vector2Int currentPos = new Vector2Int(x, block.pos.y + block.blockData.size.y);
            // print("checking" + currentPos);
            BlockObject maybeABlock = GetBlockOnPosition(currentPos);
            if (maybeABlock != null) {
                print(maybeABlock.name);
                aboveBlockSet.Add(maybeABlock);
            }
        }
        return aboveBlockSet.ToList();
    }

    public List<BlockObject> GetBelowBlocks(BlockObject block) {
        HashSet<BlockObject> belowBlockSet = new HashSet<BlockObject>();
        for (int x = block.pos.x; x < block.pos.x + block.blockData.size.x; x++) {
            Vector2Int currentPos = new Vector2Int(x, block.pos.y - 1);
            // print("checking" + currentPos);
            BlockObject maybeABlock = GetBlockOnPosition(currentPos);
            if (maybeABlock != null) {
                print(maybeABlock.name);
                belowBlockSet.Add(maybeABlock);
            }
        }
        return belowBlockSet.ToList();
    }
}


