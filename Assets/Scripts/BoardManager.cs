
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BoardManager : MonoBehaviour {
    // don't edit these in editor
    public LevelData levelData;
    public int strokes = 0;
    public List<BlockObject> blockList;
    public List<BlockObject> selectedList;
    public BlockObject selectedBlock;
    public MouseStateEnum mouseState = MouseStateEnum.DEFAULT;
    public Vector3 mousePos;
    public Vector3 clickedPosition;
    public Dictionary<Vector2Int, GameObject> markerDict = new Dictionary<Vector2Int, GameObject>();

    int BOARDHEIGHT = 20;
    int BOARDWIDTH = 20;
    //set by editor
    public GameObject markerMaster;
    public BlockObject blockObjectMaster;
    public GameObject background;
    
    void Awake() {
        CreateMarkers();
        this.levelData = LevelData.GenerateTestLevel();          // make test level
        LoadLevelData(this.levelData);
        // DestroyMarkers();
        void CreateMarkers() {
            for (int x = 0; x < this.BOARDHEIGHT; x++) {
                for (int y = 0; y < this.BOARDWIDTH; y++) {
                    GameObject marker = Instantiate(this.markerMaster, new Vector3(x,y,0f), Quaternion.identity, transform);
                    marker.name = "(" + marker.transform.position.x + ", " + marker.transform.position.y + ")";
                    Vector2Int pos = new Vector2Int(x,y);
                    this.markerDict[pos] = marker;
                }
            }
        }
    }

    void Start() {
        print("started");
    }

    void Update() {
        this.mousePos = GetMousePos();
        if (Input.GetMouseButtonDown(0)) {
            //if first time mouse clicked
            if  (this.mouseState == MouseStateEnum.DEFAULT) {
                this.mouseState = MouseStateEnum.CLICKED;
                this.clickedPosition = this.mousePos;
                this.selectedBlock = GetBlockOnPosition(GameUtil.V3ToV2I(this.mousePos));
            } 
        } else if (Input.GetMouseButtonUp(0)) {
            //if not clicked
            this.mouseState = MouseStateEnum.DEFAULT;
        }
        switch (this.mouseState) {
            case MouseStateEnum.DEFAULT:
                this.selectedBlock = null;
                UnGhostSelected();
                this.selectedList.Clear();
                break;
            case MouseStateEnum.CLICKED:
                if (this.mousePos.y > this.clickedPosition.y + 0.5) {
                    //dragging up
                    this.mouseState = MouseStateEnum.HOLDING;
                    this.selectedList = SelectUp(this.selectedBlock);
                    GhostSelected();
                } else if (this.mousePos.y < this.clickedPosition.y - 0.5) {
                    //dragging down
                    this.mouseState = MouseStateEnum.HOLDING;
                    this.selectedList = SelectDown(this.selectedBlock);
                    GhostSelected();
                }
                break;
            case MouseStateEnum.HOLDING:
                break;
        }
    }

    void DestroyMarkers() {
        GameObject[] destroyList = GameObject.FindGameObjectsWithTag("Marker");
        foreach (GameObject marker in destroyList) {
            GameObject.Destroy(marker);
        }
    }

    Vector3 GetMousePos() {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray,out hit, Mathf.Infinity)) {
            return new Vector3(hit.point.x, hit.point.y, hit.point.z);
        } else {
            return this.mousePos;
        }

    }
    void LoadLevelData(LevelData levelData) {
        foreach (KeyValuePair<BlockData, BlockState> pair in levelData.blockDataDict) {
            this.blockList.Add(CreateBlockObject(pair.Key, pair.Value));
        }
    }

    BlockObject CreateBlockObject(BlockData blockData, BlockState blockState) {
        BlockObject newBlockObject = Instantiate(this.blockObjectMaster, GameUtil.V2IOffsetV3(blockData.size, blockState.pos), Quaternion.identity);
        newBlockObject.Init(blockData, blockState);
        return newBlockObject;
    }

    // returns block occupying a grid position
    public BlockObject GetBlockOnPosition(Vector2Int pos) {
        foreach (BlockObject block in this.blockList) {
            if (block.CheckSelfPos(pos)) {
                return block;
            }
        }
        return null;
    }

    public void SetMarkerColor(Vector2Int pos, Color color) {
        this.markerDict[pos].GetComponent<Renderer>().material.color = color;
    }

    void HighlightSelected() {
        foreach (BlockObject block in this.selectedList) {
            block.Highlight(Color.red);
        }
    }

    void UnHighlightSelected() {
        foreach (BlockObject block in this.selectedList) {
            block.UnHighlight();
        }
    }

    void GhostSelected() {
        foreach (BlockObject block in this.selectedList) {
            block.SetState(BlockStateEnum.GHOST);
        }
    }

    void UnGhostSelected() {
        foreach (BlockObject block in this.selectedList) {
            block.SetState(BlockStateEnum.ACTIVE);
        }
    }

    // BLOCK SELECTION FUNCTIONS

    // returns a list of blocks selected when dragging up on a block
    public List<BlockObject> SelectUp(BlockObject rootBlock) {
        List<BlockObject> treeUpList = TreeUp(rootBlock);
        List<BlockObject> selectUpList = new List<BlockObject>(treeUpList);
        foreach (BlockObject currentBlock in treeUpList) {
            List<BlockObject> currentBlockHangers = GetBlocksBelow(currentBlock);
            foreach (BlockObject hangerBlock in currentBlockHangers) {
                if (!treeUpList.Contains(hangerBlock)) {
                    if(!IsConnectedToFixed(hangerBlock, treeUpList)) {
                        selectUpList.Add(hangerBlock);
                        print("SelectUp added " + hangerBlock + " to selection");
                        foreach (BlockObject hangerConnectedBlock in GetBlocksConnected(hangerBlock, selectUpList)) {
                            print("SelectUp foreach added " + hangerConnectedBlock + " to selection");
                            selectUpList.Add(hangerConnectedBlock);
                        }
                    }
                }
            }
        }
        return selectUpList;
    }

    // returns a list of blocks selected when dragging down on a block
    public List<BlockObject> SelectDown(BlockObject rootBlock) {
        List<BlockObject> treeDownList = TreeDown(rootBlock);
        List<BlockObject> selectDownList = new List<BlockObject>(treeDownList);
        foreach (BlockObject currentBlock in treeDownList) {
            List<BlockObject> currentBlockHangers = GetBlocksAbove(currentBlock);
            foreach (BlockObject hangerBlock in currentBlockHangers) {
                if (!treeDownList.Contains(hangerBlock)) {
                    if (!IsConnectedToFixed(hangerBlock, treeDownList)) {
                        selectDownList.Add(hangerBlock);
                        foreach (BlockObject hangerConnectedBlock in GetBlocksConnected(hangerBlock, selectDownList)) {
                            selectDownList.Add(hangerConnectedBlock);
                        }
                    }
                }
            }
        }
        return selectDownList;
    }

    // returns a list of rootBlock + blocks that are above rootBlock or connected to a block above rootBlock
    public List<BlockObject> TreeUp(BlockObject rootBlock) {
        List<BlockObject> treeUpList = new List<BlockObject>();
        treeUpRecursive(rootBlock, treeUpList);
        return treeUpList;

        void treeUpRecursive(BlockObject block, List<BlockObject> list) {
            list.Add(block);
            print("treeUpRecursive added " + block + " to selection");
            foreach (BlockObject currentBlock in GetBlocksAbove(block)) {
                if (currentBlock.blockData.type == BlockTypeEnum.FREE && !list.Contains(currentBlock)) {
                    treeUpRecursive(currentBlock, list);
                }
            }
        }
    }

    // returns a list of rootBlock + blocks that are below rootBlock or connected to a block below rootBlock
    public List<BlockObject> TreeDown(BlockObject rootBLock) {
        List<BlockObject> treeDownList = new List<BlockObject>();
        treeDownRecursive(rootBLock, treeDownList);
        return treeDownList;

        void treeDownRecursive(BlockObject block, List<BlockObject> list) {
            list.Add(block);
            foreach (BlockObject currentBlock in GetBlocksBelow(block)) {
                if (currentBlock.blockData.type == BlockTypeEnum.FREE && !list.Contains(currentBlock)) {
                    treeDownRecursive(currentBlock, list);
                }
            }
        }
    }

    // returns whether or not rootBlock has a connection to a fixed block
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

    // returns all blocks connected to this rootBlock while ignoring blocks inside ignoreList
    public List<BlockObject> GetBlocksConnected(BlockObject rootBlock, List<BlockObject> ignoreList) {
        List<BlockObject> connectedBlocks = new List<BlockObject>();
        List<BlockObject> ignoreListClone = new List<BlockObject>(ignoreList);
        bool isRoot = true;
        GetBlocksConnectedRecursive(rootBlock, ignoreListClone);
        return connectedBlocks;

        void GetBlocksConnectedRecursive(BlockObject block, List<BlockObject> ignoreListX) {
            if (isRoot == false) {
                ignoreListX.Add(block);
                connectedBlocks.Add(block);
            }
            isRoot = false;
            
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

    // returns a list of blocks directly above block
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
    
    // returns a list of blocks directly below block
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