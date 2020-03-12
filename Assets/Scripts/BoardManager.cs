using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class BoardManager : Singleton<BoardManager> {
    // don't edit these in editor
    public LevelData levelData;
    public int strokes = 0;
    public List<BlockObject> blockList;
    public List<BlockObject> selectedList;
    public List<EntityObject> entityList;
    public Dictionary<Vector2Int, GameObject> markerDict = new Dictionary<Vector2Int, GameObject>();
    public MouseStateEnum mouseState = MouseStateEnum.DEFAULT;
    public TimeStateEnum timeState = TimeStateEnum.NORMAL;
    public Vector3 mousePos;
    public EntityObject player;

    // relevant when selecting
    public BlockObject selectedBlock;
    public Vector3 clickedPosition = new Vector3(0, 0, 0);
    public Vector2Int clickOffsetV2I = Vector2Int.zero;

    //set by editor
    public GameObject markerMaster;
    public BlockObject blockObjectMaster;
    public GameObject backgroundMaster;
    public GameObject playerMaster;
    
    public UnityEngine.UI.Text debugText;
    public UnityEngine.UI.Text mapText;
    public GameObject background;

    
    void DebugTextSet() {
        this.debugText.text =   "timeState:" + this.timeState +
                                "\nmousePos: " + this.mousePos + 
                                "\nmousePosV2I: " + GameUtil.V3ToV2I(this.mousePos) + 
                                "\nclickedPosition: " + GameUtil.V3ToV2I(this.clickedPosition) + 
                                "\nclickOffsetV2I: " + this.clickOffsetV2I;
        string mapString = "";

        Dictionary<Vector2Int, BlockObject> dataDict = DataToDict();
        for (int y = levelData.boardSize.y - 1; y >= 0; y--) {
            for (int x = 0; x < levelData.boardSize.x; x++) {
                Vector2Int currentPos = new Vector2Int(x, y);
                if (dataDict.ContainsKey(currentPos)) {
                    if (dataDict[currentPos].blockData.type == BlockTypeEnum.FIXED) {
                        mapString += "<color=black>█</color>";
                    } else {
                        if (dataDict[currentPos].stateEnum == BlockStateEnum.GHOST) {
                            mapString += "<color=blue>▓</color>";
                        } else {
                            mapString += "<color=green>█</color>";
                        }
                    }
                } else if (GetEntityOnPosition(currentPos) != null) {
                    mapString += "<color=red>█</color>";
                } else {
                    mapString += "░";
                }
            }
            mapString += "\n";
        }
        this.mapText.text = mapString;
    }

    Dictionary<Vector2Int, BlockObject> DataToDict() {
        Dictionary<Vector2Int, BlockObject> dataDict = new Dictionary<Vector2Int, BlockObject>();
        foreach(BlockObject block in this.blockList) {
            for (int x = block.pos.x; x < block.pos.x + block.blockData.size.x; x++) {
                for (int y = block.pos.y; y < block.pos.y + block.blockData.size.y; y++) {
                    Vector2Int currentPos = new Vector2Int(x,y);
                    dataDict[currentPos] = block;
                }
            }
        }
        return dataDict;
    }

    void CreatePlayer() {
        //TODO figure out why this doesnt work
        player = Instantiate(this.playerMaster, GameUtil.V2IOffsetV3(new Vector2Int(2,3), new Vector2Int(7,1)), Quaternion.identity).GetComponent<EntityObject>();
        player.Init(new Vector2Int(5,16));
        this.entityList.Add(player);
    }

    void Awake() {
        this.levelData = LevelData.GenerateTestLevel(); 
        LoadLevelData(this.levelData);
        CreateBackground();
    }

    void Start() {
        print("started");
        CreatePlayer();
    }

    void Update() {
        // TODO: optimize this to not be dumb
        this.mousePos = GetMousePos();

        if (Input.GetMouseButtonDown(0)) {
            //if first time mouse clicked
            if  (this.mouseState == MouseStateEnum.DEFAULT) {
                this.mouseState = MouseStateEnum.CLICKED;
                this.clickedPosition = this.mousePos;
                this.selectedBlock = GetBlockOnPosition(GameUtil.V3ToV2I(this.mousePos));
            } 
        } else if (Input.GetMouseButtonUp(0)) {
            //if let go
            if (this.mouseState == MouseStateEnum.HOLDING) {
                //place blocks down here
                if (CheckValidMove(this.clickOffsetV2I)) {
                    foreach (BlockObject block in this.selectedList) {
                        block.pos = block.ghostPos;
                    }
                } else {
                    foreach (BlockObject block in this.selectedList) {
                        block.transform.position = GameUtil.V2IOffsetV3(block.blockData.size, block.pos);
                        block.ghostPos = block.pos;
                    }
                }
                // clear selection stuff
                this.selectedBlock = null;
                UnGhostSelected();
                this.selectedList.Clear();
                this.timeState = TimeStateEnum.NORMAL;
            }
            this.clickedPosition = new Vector3(0, 0, 0);
            this.mouseState = MouseStateEnum.DEFAULT;
        }
        switch (this.mouseState) {
            case MouseStateEnum.DEFAULT:
                break;
            case MouseStateEnum.CLICKED:
                if (this.selectedBlock != null) {
                    if (this.mousePos.y > this.clickedPosition.y + 0.5) {
                        //dragging up
                        if (!IsBlocked(true, this.selectedBlock)) {
                            this.timeState = TimeStateEnum.PAUSED;
                            this.mouseState = MouseStateEnum.HOLDING;
                            this.selectedList = SelectUp(this.selectedBlock);
                            GhostSelected();
                        }
                    } else if (this.mousePos.y < this.clickedPosition.y - 0.5) {
                        //dragging down
                        if (!IsBlocked(false, this.selectedBlock)) {
                            this.timeState = TimeStateEnum.PAUSED;
                            this.mouseState = MouseStateEnum.HOLDING;
                            this.selectedList = SelectDown(this.selectedBlock);
                            GhostSelected();
                        }
                    }
                }
                break;
            case MouseStateEnum.HOLDING:
                // TODO: make it so it follows the cursor freely when placement state is floating
                this.clickOffsetV2I = GameUtil.V3ToV2I(this.mousePos - this.clickedPosition);
                SnapToPosition(this.clickOffsetV2I);
                if (CheckValidMove(this.clickOffsetV2I)) {
                    foreach (BlockObject block in selectedList) {
                        block.Highlight(Color.blue);
                    }
                } else {
                    foreach (BlockObject block in selectedList) {
                        block.Highlight(Color.red);
                    }
                }
                break;
        }
    DebugTextSet();
    }

    public void AddMarker(Vector2Int aPos, Color aColor) {
        Vector3 markerpos = GameUtil.V2IToV3(aPos) + new Vector3(0.5f, 0.75f, 0);
        GameObject marker = Instantiate(this.markerMaster, markerpos, Quaternion.identity);
        marker.GetComponent<Renderer>().material.color = aColor;
    }

    // make this less shitty later
    bool CheckValidMove(Vector2Int aOffset) {
        HashSet<Vector2Int> checkTopPositions = new HashSet<Vector2Int>();
        HashSet<Vector2Int> checkBotPositions = new HashSet<Vector2Int>();
        foreach (BlockObject block in this.selectedList) {
            for (int x = 0; x < block.blockData.size.x; x++) {
                for (int y = 0; y < block.blockData.size.y; y++) {
                    BlockObject maybeABlock = GetBlockOnPosition(block.pos + aOffset + new Vector2Int(x,y));
                    if (maybeABlock != null && !this.selectedList.Contains(maybeABlock)) {
                        // print("IS BLOCKED! INVALID MOVE!!!");
                        return false;
                    }
                }
            }
            for (int x = block.ghostPos.x; x < block.ghostPos.x + block.blockData.size.x; x++) {
                int aboveY = block.ghostPos.y + block.blockData.size.y;
                int belowY = block.ghostPos.y - 1;
                Vector2Int topPos = new Vector2Int(x, aboveY);
                Vector2Int botPos = new Vector2Int(x, belowY);
                foreach (BlockObject otherBlock in this.selectedList) {
                    if (GetSelectedBlockOnPosition(topPos) == null) {
                        Vector2Int checkPos = new Vector2Int(x,aboveY);
                        if (GetSelectedBlockOnPosition(topPos + Vector2Int.up) == null) {
                            checkTopPositions.Add(checkPos);
                        }
                    }
                    if (GetSelectedBlockOnPosition(botPos) == null) {
                        Vector2Int checkPos = new Vector2Int(x,belowY);
                        if (GetSelectedBlockOnPosition(botPos + Vector2Int.down) == null) {
                            checkBotPositions.Add(checkPos);
                        }
                    }
                }
            }
        }
        bool connectedOnTop = false;
        bool connectedOnBot = false;

        foreach (Vector2Int pos in checkTopPositions) {
            if (GetBlockOnPosition(pos) != null && !this.selectedList.Contains(GetBlockOnPosition(pos))) {
                connectedOnTop = true;
                // print("connected on top at" + pos);
            }
        }
        foreach (Vector2Int pos in checkBotPositions) {
            if (GetBlockOnPosition(pos) != null && !this.selectedList.Contains(GetBlockOnPosition(pos))) {
                connectedOnBot = true;
                // print("connected on bot at" + pos);
            }
        }
        if (connectedOnTop == true && connectedOnBot == true) {
            // print("IS SANDWICHED! INVALID MOVE!!!");
            return false;
        } else if (connectedOnTop == false && connectedOnBot == false) {
            // print("IS FLOATING! INVALID MOVE!!!");
            return false;
        } else {
            // print ("VALID MOVE");
            return true;
        }
    }

    void SnapToPosition(Vector2Int aOffset) {
        if (GameUtil.IsInside(GameUtil.V3ToV2I(this.mousePos), Vector2Int.zero, this.levelData.boardSize)) {
            foreach (BlockObject block in this.selectedList) {
                Vector2Int newPos = block.pos + aOffset;
                block.transform.position = GameUtil.V2IOffsetV3(block.blockData.size, newPos);
                block.ghostPos = newPos;
            }
        }
    }

    void CreateBackground() {
        Vector3 backgroundOffset = new Vector3(0, 0, 1);
        Vector3 backgroundThiccness = new Vector3(0, 0, 0.1f);
        this.background = Instantiate(this.backgroundMaster, GameUtil.V2IOffsetV3(this.levelData.boardSize, Vector2Int.zero) + backgroundOffset, Quaternion.identity);
        this.background.transform.localScale =  GameUtil.V2IToV3(this.levelData.boardSize) + backgroundThiccness;
    }

    public void DestroyMarkers() {
        foreach (GameObject marker in GameObject.FindGameObjectsWithTag("Marker")) {
            GameObject.Destroy(marker);
        }
    }

    // TODO: fix that bug where it flickers back and forth because of some raycasting fuckery
    Vector3 GetMousePos() {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity)) {
            return new Vector3(hit.point.x, hit.point.y, hit.point.z);
        } else {
            return this.mousePos;
        }
    }

    void LoadLevelData(LevelData aLevelData) {
        foreach (KeyValuePair<BlockData, BlockState> pair in aLevelData.blockDataDict) {
            this.blockList.Add(CreateBlockObject(pair.Key, pair.Value));
        }
    }

    BlockObject CreateBlockObject(BlockData aBlockData, BlockState aBlockState) {
        BlockObject newBlockObject = Instantiate(this.blockObjectMaster, GameUtil.V2IOffsetV3(aBlockData.size, aBlockState.pos), Quaternion.identity);
        newBlockObject.Init(aBlockData, aBlockState);
        return newBlockObject;
    }

    // returns block occupying a grid position
    public BlockObject GetBlockOnPosition(Vector2Int aPos) {
        foreach (BlockObject block in this.blockList) {
            if (block.CheckSelfPos(aPos)) {
                return block;
            }
        }
        return null;
    }

    public EntityObject GetEntityOnPosition(Vector2Int aPos) {
        foreach (EntityObject entity in this.entityList) {
            if (entity.CheckSelfPos(aPos)) {
                return entity;
            }
        }    
        return null;
    }

    public BlockObject GetSelectedBlockOnPosition(Vector2Int aPos) {
        foreach (BlockObject block in this.selectedList) {
            if (block.CheckGhostPos(aPos)) {
                return block;
            }
        }
        return null;
    }

    public void SetMarkerColor(Vector2Int pos, Color color) {
        this.markerDict[pos].GetComponent<Renderer>().material.color = color;
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

    public bool IsBlocked(bool aIsUp, BlockObject aBlock) {
        bool isBlocked = false;
        List<BlockObject> ignoreList = new List<BlockObject>();
        if (aIsUp) {
            CheckSelectUpRecursive(aBlock, ignoreList);
        } else {
            CheckSelectDownRecursive(aBlock, ignoreList);
        }
        return isBlocked;

        void CheckSelectUpRecursive(BlockObject rBlock, List<BlockObject> rIgnoreList) {
            if (isBlocked == false) {
                rIgnoreList.Add(rBlock);
                if (rBlock.blockData.type == BlockTypeEnum.FIXED) {
                    isBlocked = true;
                    return;
                }
                foreach (BlockObject aboveBlock in GetBlocksAbove(rBlock)) {
                    if (!rIgnoreList.Contains(aboveBlock)) {
                        CheckSelectUpRecursive(aboveBlock, rIgnoreList);
                    }
                }
            }
            
        }

        void CheckSelectDownRecursive(BlockObject rBlock, List<BlockObject> rIgnoreList) {
            if (isBlocked == false) {
                rIgnoreList.Add(rBlock);
                if (rBlock.blockData.type == BlockTypeEnum.FIXED) {
                    isBlocked = true;
                    return;
                }
                foreach (BlockObject aboveBlock in GetBlocksBelow(rBlock)) {
                    if (!rIgnoreList.Contains(aboveBlock)) {
                        CheckSelectDownRecursive(aboveBlock, rIgnoreList);
                    }
                }
            }
        }
    }

    // returns a list of blocks selected when dragging up on a block
    public List<BlockObject> SelectUp(BlockObject aRootBlock) {
        List<BlockObject> treeUpList = TreeUp(aRootBlock);
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
    public List<BlockObject> SelectDown(BlockObject aRootBlock) {
        List<BlockObject> treeDownList = TreeDown(aRootBlock);
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
    public List<BlockObject> TreeUp(BlockObject aRootBlock) {
        List<BlockObject> treeUpList = new List<BlockObject>();
        treeUpRecursive(aRootBlock, treeUpList);
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
    public List<BlockObject> TreeDown(BlockObject aRootBlock) {
        List<BlockObject> treeDownList = new List<BlockObject>();
        treeDownRecursive(aRootBlock, treeDownList);
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
    public bool IsConnectedToFixed(BlockObject aRootBlock, List<BlockObject> aIgnoreList) {
        bool isConnectedToFixed = false;
        List<BlockObject> ignoreListClone = new List<BlockObject>(aIgnoreList);
        IsConnectedToFixedRecursive(aRootBlock, ignoreListClone);
        return isConnectedToFixed;

        void IsConnectedToFixedRecursive(BlockObject rBlock, List<BlockObject> rIgnoreList) {
            rIgnoreList.Add(rBlock);
            if (rBlock.blockData.type == BlockTypeEnum.FIXED) {
                isConnectedToFixed = true;
                return;
            }
            foreach (BlockObject aboveBlock in GetBlocksAbove(rBlock)) {
                if (!rIgnoreList.Contains(aboveBlock)) {
                    IsConnectedToFixedRecursive(aboveBlock, rIgnoreList);
                }
            }
            foreach (BlockObject belowBlock in GetBlocksBelow(rBlock)) {
                if (!rIgnoreList.Contains(belowBlock)) {
                    IsConnectedToFixedRecursive(belowBlock, rIgnoreList);
                }
            }
        }
    }

    // returns all blocks connected to this rootBlock while ignoring blocks inside ignoreList
    public List<BlockObject> GetBlocksConnected(BlockObject aRootBlock, List<BlockObject> aIgnoreList) {
        List<BlockObject> connectedBlocks = new List<BlockObject>();
        List<BlockObject> ignoreListClone = new List<BlockObject>(aIgnoreList);
        // isRoot = true will ignore the root Block in the recursive function
        bool isRoot = true;
        GetBlocksConnectedRecursive(aRootBlock, ignoreListClone);
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
    public List<BlockObject> GetBlocksAbove(BlockObject aBlock) {
        HashSet<BlockObject> aboveSet = new HashSet<BlockObject>();
        for (int x = aBlock.pos.x; x < aBlock.pos.x + aBlock.blockData.size.x; x++) {
            int y = aBlock.pos.y + aBlock.blockData.size.y;
            Vector2Int currentPos = new Vector2Int(x,y);
            BlockObject maybeABlock = GetBlockOnPosition(currentPos);
            if (maybeABlock != null) {
                aboveSet.Add(maybeABlock);
            }
        }
        return aboveSet.ToList();
    }
    
    // returns a list of blocks directly below block
    public List<BlockObject> GetBlocksBelow(BlockObject aBlock) {
        HashSet<BlockObject> belowSet = new HashSet<BlockObject>();
        for (int x = aBlock.pos.x; x < aBlock.pos.x + aBlock.blockData.size.x; x++) {
            int y = aBlock.pos.y - 1;
            Vector2Int currentPos = new Vector2Int(x,y);
            BlockObject maybeABlock = GetBlockOnPosition(currentPos);
            if (maybeABlock != null) {
                belowSet.Add(maybeABlock);
            }
        }
        return belowSet.ToList();
    }

}