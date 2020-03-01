
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    public Vector2Int mousePosV2I = new Vector2Int(0, 0);

    public Vector3 clickedPosition = new Vector3(0, 0, 0);
    public Vector2Int clickedPositionV2I = new Vector2Int(0, 0);
    public Vector2Int clickOffsetV2I = new Vector2Int(0, 0);

    public Dictionary<Vector2Int, GameObject> markerDict = new Dictionary<Vector2Int, GameObject>();
    //set by editor
    public GameObject markerMaster;
    public BlockObject blockObjectMaster;
    public GameObject backgroundMaster;
    
    public UnityEngine.UI.Text debugText;
    public GameObject background;

    void DebugTextSet() {
        this.debugText.text = "mousePos: " + mousePos + "\nmousePosV2I: " + mousePosV2I + "\nclickedPosition: " + clickedPositionV2I + "\nclickOffsetV2I: " + clickOffsetV2I;
    }

    void Awake() {
        this.debugText = GameObject.Find("DebugText").GetComponent<Text>();
        this.debugText.text = "TEST";
        this.levelData = LevelData.GenerateTestLevel(); 
        CreateBackground();
        // CreateMarkers();
        LoadLevelData(this.levelData);
        // DestroyMarkers();
    }

    void Start() {
        print("started");
    }

    void Update() {
        
        this.mousePos = GetMousePos();
        mousePosV2I = GameUtil.V3ToV2I(this.mousePos);

        DrawPathMouseToCenter();

        if (Input.GetMouseButtonDown(0)) {
            //if first time mouse clicked
            if  (this.mouseState == MouseStateEnum.DEFAULT) {
                this.mouseState = MouseStateEnum.CLICKED;
                this.clickedPosition = this.mousePos;
                this.clickedPositionV2I = GameUtil.V3ToV2I(this.clickedPosition);
                this.selectedBlock = GetBlockOnPosition(GameUtil.V3ToV2I(this.mousePos));
            } 
        } else if (Input.GetMouseButtonUp(0)) {
            //if not clicked
            
            if (this.mouseState == MouseStateEnum.HOLDING) {
                //place blocks down here
                print("let go");
                if (CheckValidMove(clickOffsetV2I)) {
                    foreach (BlockObject block in selectedList) {
                        block.pos = block.objectPos;
                    }
                } else {
                    foreach (BlockObject block in selectedList) {
                        block.transform.position = GameUtil.V2IOffsetV3(block.blockData.size, block.pos);
                        block.objectPos = block.pos;
                    }
                }

            }
            this.clickedPosition = new Vector3(0, 0, 0);
            this.clickedPositionV2I = GameUtil.V3ToV2I(this.clickedPosition);
            this.mouseState = MouseStateEnum.DEFAULT;
        }
        switch (this.mouseState) {
            case MouseStateEnum.DEFAULT:
                this.selectedBlock = null;
                UnGhostSelected();
                this.selectedList.Clear();
                break;
            case MouseStateEnum.CLICKED:
                if (this.selectedBlock != null) {
                    if (this.mousePos.y > this.clickedPosition.y + 0.5) {
                        //dragging up
                        if (!IsBlocked(true, selectedBlock)) {
                            this.mouseState = MouseStateEnum.HOLDING;
                            this.selectedList = SelectUp(this.selectedBlock);
                            GhostSelected();
                        }
                    } else if (this.mousePos.y < this.clickedPosition.y - 0.5) {
                        //dragging down
                        if (!IsBlocked(false, selectedBlock)) {
                            this.mouseState = MouseStateEnum.HOLDING;
                            this.selectedList = SelectDown(this.selectedBlock);
                            GhostSelected();
                        }
                    }
                }
                break;
            case MouseStateEnum.HOLDING:
                clickOffsetV2I = mousePosV2I -clickedPositionV2I;
                SnapToPosition(clickOffsetV2I);
                if (CheckValidMove(clickOffsetV2I)) {
                    foreach (BlockObject block in selectedList) {
                        block.Highlight(Color.blue);
                    }
                } else {
                    foreach (BlockObject block in selectedList) {
                        block.Highlight(Color.red);
                    }
                    // MoveSelectionToMouse();
                }
                
                // MoveSelectionToMouse();
                break;
        }
    DebugTextSet();
    }
// >>>>>>>>>>>>>>>>>>>>>>>>> TODO WRITE A SNAPPING FUNCTION AND ALSO WRITE A  FUNCTION TAHT CHECKS IF THE BLOCK IS IN A PLACE WHERE IT CAN ACTUALY BE PLACED <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

    void AddMarker(Vector2Int pos, Color color) {
        Vector3 markerpos = GameUtil.V2IToV3(pos) + new Vector3(0.5f, 0.75f, 0);
        GameObject marker = Instantiate(markerMaster, markerpos, Quaternion.identity);
        marker.GetComponent<Renderer>().material.color = color;
    }

    // make this less shitty later
    bool CheckValidMove(Vector2Int offset) {
        HashSet<Vector2Int> checkTopPositions = new HashSet<Vector2Int>();
        HashSet<Vector2Int> checkBotPositions = new HashSet<Vector2Int>();
        foreach (BlockObject block in selectedList) {
            for (int x = 0; x < block.blockData.size.x; x++) {
                for (int y = 0; y < block.blockData.size.y; y++) {
                    BlockObject maybeABlock = GetBlockOnPosition(block.pos + offset + new Vector2Int(x,y));
                    if (maybeABlock != null && !selectedList.Contains(maybeABlock)) {
                        print("IS BLOCKED! INVALID MOVE!!!");
                        return false;
                    }
                }
            }
            for (int x = block.objectPos.x; x < block.objectPos.x + block.blockData.size.x; x++) {
                int aboveY = block.objectPos.y + block.blockData.size.y;
                int belowY = block.objectPos.y - 1;
                Vector2Int topPos = new Vector2Int(x, aboveY);
                Vector2Int botPos = new Vector2Int(x, belowY);
                foreach (BlockObject otherBlock in selectedList) {
                    if (GetSelectedBlockOnPosition(topPos) == null) {
                        Vector2Int checkPos = new Vector2Int(x,aboveY);
                        if (GetSelectedBlockOnPosition(topPos + new Vector2Int(0,1)) == null) {
                            checkTopPositions.Add(checkPos);
                        }
                    }
                    if (GetSelectedBlockOnPosition(botPos) == null) {
                        Vector2Int checkPos = new Vector2Int(x,belowY);
                        if (GetSelectedBlockOnPosition(botPos - new Vector2Int(0,1)) == null) {
                            checkBotPositions.Add(checkPos);
                        }
                    }
                }
            }
        }
        // foreach (Vector2Int pos in checkTopPositions.Union(checkBotPositions)) {
        //     AddMarker(pos, Color.green);
        // }
        bool connectedOnTop = false;
        bool connectedOnBot = false;

        foreach (Vector2Int pos in checkTopPositions) {
            if (GetBlockOnPosition(pos) != null && !selectedList.Contains(GetBlockOnPosition(pos))) {
                connectedOnTop = true;
                // print("connected on top at" + pos);
            }
        }
        foreach (Vector2Int pos in checkBotPositions) {
            if (GetBlockOnPosition(pos) != null && !selectedList.Contains(GetBlockOnPosition(pos))) {
                connectedOnBot = true;
                // print("connected on bot at" + pos);
            }
        }
        if (connectedOnTop == true && connectedOnBot == true) {
            print("IS SANDWICHED! INVALID MOVE!!!");
            return false;
        } else if (connectedOnTop == false && connectedOnBot == false) {
            print("IS FLOATING! INVALID MOVE!!!");
            return false;
        } else {
            print ("VALID MOVE");
            return true;
        }
    }

    void SnapToPosition(Vector2Int offset) {
        foreach (BlockObject block in selectedList) {
            Vector2Int newPos = block.pos + offset;
            block.transform.position = GameUtil.V2IOffsetV3(block.blockData.size, newPos);
            block.objectPos = newPos;
        }
    }

    void DrawPathMouseToCenter() {
        Vector2Int centerGrid = GameUtil.V3ToV2I(mousePos);
        Vector3 center = GameUtil.V2IToV3(centerGrid) + new Vector3(0.5f, 0.75f, 0);
        Debug.DrawLine(mousePos + new Vector3(0,0,-1), center);
    }

    void MoveSelectionToMouse() {
        foreach (BlockObject block in selectedList) {
            Vector3 newPosition = GameUtil.V2IOffsetV3(block.blockData.size, block.pos) + this.mousePos - this.clickedPosition;
            newPosition.z = 0;
            block.transform.position = newPosition;
        }
    }



    void CreateBackground() {
        Vector3 backgroundOffset = new Vector3(0, 0, 1);
        this.background = Instantiate(backgroundMaster, GameUtil.V2IOffsetV3(this.levelData.boardSize, new Vector2Int(0, 0)) + backgroundOffset, Quaternion.identity);
        this.background.transform.localScale =  GameUtil.V2IToV3(this.levelData.boardSize) + new Vector3(0, 0, 0.1f);
    }

    void CreateMarkers() {
        for (int x = 0; x < this.levelData.boardSize.x; x++) {
            for (int y = 0; y < this.levelData.boardSize.y; y++) {
                Vector3 realLocation = GameUtil.V2IToV3(new Vector2Int(x,y));
                GameObject marker = Instantiate(this.markerMaster, realLocation, Quaternion.identity, transform);
                marker.name = "(" + x + ", " + y + ")";
                Vector2Int pos = new Vector2Int(x,y);
                this.markerDict[pos] = marker;
            }
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

    public BlockObject GetSelectedBlockOnPosition(Vector2Int objectPos) {
        foreach (BlockObject block in this.selectedList) {
            if (block.CheckSelfObjectPos(objectPos)) {
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

    // returns true if block cant be pulled from the direction of isUp
    
    public bool IsBlocked(bool isUp, BlockObject block) {
        bool isBlocked = false;
        List<BlockObject> ignoreList = new List<BlockObject>();
        if (isUp) {
            CheckSelectUpRecursive(block, ignoreList);
        } else {
            CheckSelectDownRecursive(block, ignoreList);
        }
        return isBlocked;

        void CheckSelectUpRecursive(BlockObject block1, List<BlockObject> ignoreList1) {
            if (isBlocked == false) {
                ignoreList1.Add(block1);
                if (block1.blockData.type == BlockTypeEnum.FIXED) {
                    isBlocked = true;
                    return;
                }
                foreach (BlockObject aboveBlock in GetBlocksAbove(block1)) {
                    if (!ignoreList1.Contains(aboveBlock)) {
                        CheckSelectUpRecursive(aboveBlock, ignoreList1);
                    }
                }
            }
            
        }

        void CheckSelectDownRecursive(BlockObject block1, List<BlockObject> ignoreList1) {
            if (isBlocked == false) {
                ignoreList1.Add(block1);
                if (block1.blockData.type == BlockTypeEnum.FIXED) {
                    isBlocked = true;
                    return;
                }
                foreach (BlockObject aboveBlock in GetBlocksBelow(block1)) {
                    if (!ignoreList1.Contains(aboveBlock)) {
                        CheckSelectDownRecursive(aboveBlock, ignoreList1);
                    }
                }
            }
        }
    }

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
                        foreach (BlockObject hangerConnectedBlock in GetBlocksConnected(hangerBlock, selectUpList)) {
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
        // isRoot = true will ignore the root Block in the recursive function
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

    // TODO a function that returns true if block is OK to be selected up/down
}