
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
    int BOARDHEIGHT = 20;
    int BOARDWIDTH = 20;
    public Dictionary<Vector2Int, GameObject> markerDict = new Dictionary<Vector2Int, GameObject>();

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
                UnGhostSelected();
                selectedList.Clear();
                break;
            case MouseStateEnum.CLICKED:
                if (mousePos.y < clickedPosition.y - 0.5) {
                    //dragging down
                    mouseState = MouseStateEnum.HOLDING;
                    //replace this with a recursive function later
                    // IsObstructed(selectedBlock, false);
                    
                    // selectedList.AddRange(GetBelowBlocks(selectedBlock));
                    GhostSelected();
                } else if (mousePos.y > clickedPosition.y + 0.5) {
                    //dragging up
                    mouseState = MouseStateEnum.HOLDING;
                    //replace this with a recursive function later
                    selectedList = SelectUp(selectedBlock);
                    HighlightSelected();
                    // selectedList.AddRange(GetAboveBlocks(selectedBlock));
                    GhostSelected();
                }
                break;
            case MouseStateEnum.HOLDING:
                break;
        }
    }



    void Awake() {
        CreateMarkers();
        levelData = LevelData.GenerateTestLevel();          // make test level
        LoadLevelData(levelData);
        // DestroyMarkers();
        void CreateMarkers() {
            for (int x = 0; x < BOARDHEIGHT; x++) {
                for (int y = 0; y < BOARDWIDTH; y++) {
                    GameObject marker = Instantiate(markerMaster, new Vector3(x,y,0f), Quaternion.identity, transform);
                    marker.name = "(" + marker.transform.position.x + ", " + marker.transform.position.y + ")";
                    Vector2Int pos = new Vector2Int(x,y);
                    markerDict[pos] = marker;
                }
            }
        }
    }

    void Start() {
        print("started");
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

    public void SetMarkerColor(Vector2Int pos, Color color) {
        markerDict[pos].GetComponent<Renderer>().material.color = color;
    }

    void HighlightSelected() {
        foreach (BlockObject block in selectedList) {
            block.Highlight(Color.red);
        }
    }

    void UnHighlightSelected() {
        foreach (BlockObject block in selectedList) {
            block.UnHighlight();
        }
    }

    void GhostSelected() {
        foreach (BlockObject block in selectedList) {
            block.SetState(BlockStateEnum.GHOST);
        }
    }

    void UnGhostSelected() {
        foreach (BlockObject block in selectedList) {
            block.SetState(BlockStateEnum.ACTIVE);
        }
    }

    public List<BlockObject> SelectUp(BlockObject rootBlock) {
        List<BlockObject> treeUpList = TreeUp(rootBlock);
        List<BlockObject> selectUpList = new List<BlockObject>(treeUpList);
        foreach (BlockObject currentBlock in treeUpList) {
            List<BlockObject> currentBlockHangers = GetBlocksBelow(currentBlock);
            foreach (BlockObject hangerBlock in currentBlockHangers) {
                if (!treeUpList.Contains(hangerBlock)) {
                    print("selectUp looking at hanger: " + hangerBlock.name);
                    if(!IsConnectedToFixed(hangerBlock, treeUpList)) {
                        selectUpList.Add(hangerBlock);
                        foreach(BlockObject hangerConnectedBlock in GetBlocksConnected(hangerBlock, selectUpList)) {
                            selectUpList.Add(hangerConnectedBlock);
                        }
                    }

                }
            }
        }
        return selectUpList;
    }

    public List<BlockObject> TreeUp(BlockObject rootBlock) {
        List<BlockObject> treeUpList = new List<BlockObject>();
        treeUpRecursive(rootBlock, treeUpList);
        foreach(BlockObject block in treeUpList) {
            print("treeUp contains: " + block.name);
        }
        return treeUpList.ToList();

        void treeUpRecursive(BlockObject block, List<BlockObject> list) {
            list.Add(block);
            List<BlockObject> testList = GetBlocksAbove(block);
            foreach(BlockObject currentBlock in GetBlocksAbove(block)) {
                if (currentBlock.blockData.type == BlockTypeEnum.FREE && !list.Contains(currentBlock)) {
                    treeUpRecursive(currentBlock, list);
                }
            }
        }
    }

    public bool IsConnectedToFixed(BlockObject rootBlock, List<BlockObject> ignoreList) {
        bool isConnectedToFixed = false;
        List<BlockObject> ignoreListClone = new List<BlockObject>(ignoreList);
        IsConnectedToFixedRecursive(rootBlock, ignoreListClone);
        return isConnectedToFixed;

        void IsConnectedToFixedRecursive(BlockObject block, List<BlockObject> ignoreListX) {
            ignoreListX.Add(block);
            if (block.blockData.type == BlockTypeEnum.FIXED) {
                isConnectedToFixed = true;
                return;
            }
            foreach (BlockObject aboveBlock in GetBlocksAbove(block)) {
                if (!ignoreListX.Contains(aboveBlock)) {
                    IsConnectedToFixedRecursive(aboveBlock, ignoreListX);
                }
            }
            foreach (BlockObject belowBlock in GetBlocksBelow(block)) {
                if (!ignoreListX.Contains(belowBlock)) {
                    IsConnectedToFixedRecursive(belowBlock, ignoreListX);
                }
            }
        }
    }

    public List<BlockObject> GetBlocksConnected(BlockObject rootBlock, List<BlockObject> ignoreList) {
        List<BlockObject> connectedBlocks = new List<BlockObject>();
        List<BlockObject> ignoreListClone = new List<BlockObject>(ignoreList);
        GetBlocksConnectedRecursive(rootBlock, ignoreListClone);
        return connectedBlocks;

        void GetBlocksConnectedRecursive(BlockObject block, List<BlockObject> ignoreListX) {
            ignoreListX.Add(block);
            connectedBlocks.Add(block);
            foreach (BlockObject aboveBlock in GetBlocksAbove(block)) {
                if (!ignoreListX.Contains(aboveBlock)) {
                    GetBlocksConnectedRecursive(aboveBlock, ignoreListX);
                }
            }
            foreach (BlockObject belowBlock in GetBlocksBelow(block)) {
                if (!ignoreListX.Contains(belowBlock)) {
                    GetBlocksConnectedRecursive(belowBlock, ignoreListX);
                }
            }
        }

    }
    public List<BlockObject> GetBlocksAbove(BlockObject block) {
        HashSet<BlockObject> aboveSet = new HashSet<BlockObject>();
        for (int x = block.pos.x; x < block.pos.x + block.blockData.size.x; x++) {
            int y = block.pos.y + block.blockData.size.y;
            Vector2Int currentPos = new Vector2Int(x,y);
            BlockObject maybeABlock = GetBlockOnPosition(currentPos);
            if (maybeABlock != null) {
                aboveSet.Add(maybeABlock);
            }
        }
        return aboveSet.ToList();
    }
    
    public List<BlockObject> GetBlocksBelow(BlockObject block) {
        HashSet<BlockObject> belowSet = new HashSet<BlockObject>();
        for (int x = block.pos.x; x < block.pos.x + block.blockData.size.x; x++) {
            int y = block.pos.y - 1;
            Vector2Int currentPos = new Vector2Int(x,y);
            BlockObject maybeABlock = GetBlockOnPosition(currentPos);
            if (maybeABlock != null) {
                belowSet.Add(maybeABlock);
            }
        }
        return belowSet.ToList();
    }

}