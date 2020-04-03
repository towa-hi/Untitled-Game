using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.EventSystems;

public class BoardManager : Singleton<BoardManager> {
    // don't edit these in editor
    public LevelData levelData;
    public int strokes = 0;
    public List<BlockObject> blockList;
    public List<BlockObject> selectedList;
    public List<MobObject> mobList;
    public Dictionary<Vector2Int, GameObject> markerDict = new Dictionary<Vector2Int, GameObject>();
    public MouseStateEnum mouseState = MouseStateEnum.UNCLICKED;
    public TimeStateEnum timeState = TimeStateEnum.NORMAL;
    public SelectionStateEnum selectionState = SelectionStateEnum.UNSELECTED;
    public Vector3 mousePos;
    public Vector3 oldMousePos;
    public MobObject player;
    public List<BlockObject> tempFixedBlockList;
    // relevant when selecting
    public BlockObject clickedBlock;
    public Vector3 clickedPos;
    public Vector3 clickOffset;
    public Vector3 oldClickOffset;
    public bool isValidMove;

    //set by editor
    public GameObject markerMaster;
    public BlockObject blockObjectMaster;
    public GameObject backgroundMaster;
    public GameObject playerMaster;
    
    public UnityEngine.UI.Text debugText;
    public UnityEngine.UI.Text mapText;
    public GameObject background;
    public PostProcessVolume volume;

    void Awake() {
        this.levelData = LevelData.GenerateTestLevel();
        this.blockList = BlockDataListToBlockObjectList(this.levelData.blockDataList);
        this.background = CreateBackground();
        CreatePlayer();
    }

    static List<BlockObject> BlockDataListToBlockObjectList(List<BlockData> aBlockDataList) {
        List<BlockObject> newBlockList = new List<BlockObject>();
        foreach (BlockData blockData in aBlockDataList) {
            BlockObject newBlockObject = Instantiate(BoardManager.Instance.blockObjectMaster, GameUtil.V2IOffsetV3(blockData.size, blockData.pos), Quaternion.identity);
            newBlockObject.Init(blockData);
            newBlockList.Add(newBlockObject);
        }
        return newBlockList;
    }

    void DebugTextSet() {
        this.debugText.text =   "timeState: " + this.timeState +
                                "\nmouseState: " + this.mouseState +
                                "\nselectionState: " + this.selectionState +
                                "\nmousePos: " + this.mousePos + 
                                "\nmousePosV2I: " + GameUtil.V3ToV2I(this.mousePos) + 
                                "\nclickedPos: " + this.clickedPos +
                                "\nclickedPositionV2I: " + GameUtil.V3ToV2I(this.clickedPos) + 
                                "\nclickOffset: " + this.clickOffset +
                                "\nclickedBlock: " + this.clickedBlock;
        string mapString = "";

        for (int y = levelData.boardSize.y - 1; y >= 0; y--) {
            for (int x = 0; x < levelData.boardSize.x; x++) {
                Vector2Int currentPos = new Vector2Int(x, y);
                BlockObject maybeABlock = GetBlockOnPosition(currentPos);
                if (maybeABlock != null) {
                    if (maybeABlock.state == BlockStateEnum.FIXED) {
                        mapString += "<color=black>█</color>";
                    } else {
                        if (maybeABlock.state == BlockStateEnum.GHOST) {
                            mapString += "<color=blue>▓</color>";
                        } else if (maybeABlock.state == BlockStateEnum.INVALID) {
                            mapString += "<color=red>▓</color>";
                        } else {
                            mapString += "<color=grey>█</color>";
                        }
                    }
                } else if (GetMobOnPosition(currentPos) != null) {
                    mapString += "<color=yellow>█</color>";
                } else {
                    mapString += "░";
                }
            }
            mapString += "\n";
        }
        this.mapText.text = mapString;
    }

    void Update() {
        // TODO make this less dumb
        FixBlocksBelowEntity();

        this.mousePos = GetMousePos();
        
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()) {
            this.mouseState = MouseStateEnum.HELD;
            OnClickDown();
        }
        
        if (Input.GetMouseButtonUp(0)) {
            this.mouseState = MouseStateEnum.UNCLICKED;
            OnClickRelease();
            if (this.selectionState == SelectionStateEnum.HOLDING) {
                DeselectBlocks();
                this.selectionState = SelectionStateEnum.UNSELECTED;
                ResumeTime();
            }
        }

        switch (this.mouseState) {
            case MouseStateEnum.UNCLICKED:
                break;
            case MouseStateEnum.CLICKED:

                break;
            case MouseStateEnum.HELD:
                float dragThreshold = 0.2f;
                this.clickOffset = this.mousePos - this.clickedPos;
                if (this.clickedBlock != null && this.clickedBlock.state == BlockStateEnum.ACTIVE) {
                    if (this.mousePos.y > this.clickedPos.y + dragThreshold) {
                        //dragging up
                        if (!IsBlocked(true, this.clickedBlock)) {
                            SelectBlocks(SelectUp(this.clickedBlock));
                            PauseTime();
                            this.selectionState = SelectionStateEnum.HOLDING;
                        }
                    } else if (this.mousePos.y < this.clickedPos.y - dragThreshold) {
                        //dragging down
                        if (!IsBlocked(false, this.clickedBlock)) {
                            SelectBlocks(SelectDown(this.clickedBlock));
                            PauseTime();
                            this.selectionState = SelectionStateEnum.HOLDING;
                        }
                    }
                }
                break;
        }

        switch (this.selectionState) {
            case SelectionStateEnum.HOLDING:
                // only do every time mouse cursor moves between tiles
                if (GameUtil.V3ToV2I(this.clickOffset) != GameUtil.V3ToV2I(this.oldClickOffset)) {
                    SnapToPosition(GameUtil.V3ToV2I(this.clickOffset));
                    this.isValidMove = CheckValidMove(GameUtil.V3ToV2I(this.clickOffset), this.selectedList);
                    foreach (BlockObject block in selectedList) {
                        if (this.isValidMove) {
                            block.SetState(BlockStateEnum.GHOST);
                        } else {
                            block.SetState(BlockStateEnum.INVALID);
                        }
                    }
                    if (this.isValidMove) {
                        SetSelectedListState(BlockStateEnum.GHOST);
                    } else {
                        SetSelectedListState(BlockStateEnum.INVALID);
                    }
                }
                break;
            case SelectionStateEnum.INVALID:
                break;
            case SelectionStateEnum.SELECTED:
                break;
            case SelectionStateEnum.UNSELECTED:
                break;
        }
        
        this.oldClickOffset = this.clickOffset;
        DebugTextSet();
    }

    void PauseTime() {
        this.timeState = TimeStateEnum.PAUSED;
        volume.GetComponent<PostProcessVolume>().enabled = true;
        // volume.GetComponent<PostProcessVolume>().isGlobal = true;
    }

    void ResumeTime() {
        this.timeState = TimeStateEnum.NORMAL;
        volume.GetComponent<PostProcessVolume>().enabled = false;

        // volume.GetComponent<PostProcessVolume>().isGlobal = false;
    }
    void SetSelectedListState(BlockStateEnum aState) {
        foreach (BlockObject block in this.selectedList) {
            block.SetState(aState);
        }
    }
    

    void SelectBlocks(List<BlockObject> aBlockList) {
        this.selectedList = aBlockList;
        foreach (BlockObject block in this.selectedList) {
            block.SetState(BlockStateEnum.GHOST);
        }
    }

    void DeselectBlocks() {
        if (isValidMove) {
            foreach (BlockObject block in this.selectedList) {
                block.pos = block.ghostPos;
                block.SetState(BlockStateEnum.ACTIVE);
            }
        } else {
            foreach (BlockObject block in this.selectedList) {
                block.transform.position = GameUtil.V2IOffsetV3(block.size, block.pos);
                block.ghostPos = block.pos;
                block.SetState(BlockStateEnum.ACTIVE);
            }
        }
        this.selectedList = null;
        
    }

    void OnClickDown() {
        this.clickedPos = GetMousePos();
        this.clickedBlock = GetClickedBlock();
    }

    void OnClickRelease() {
        this.clickedPos = new Vector3(0, 0, 0);
        this.clickedBlock = null;
    }

    BlockObject GetClickedBlock() {
        RaycastHit[] hits;
        hits = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition), 100);
        foreach (RaycastHit hit in hits) {
            if (hit.transform.gameObject.tag == "Block") {
                return hit.transform.gameObject.GetComponent<BlockObject>();
            }
        }
        return null;
    }

    void CreatePlayer() {
        player = Instantiate(this.playerMaster, GameUtil.V2IOffsetV3(new Vector2Int(2,3), new Vector2Int(7,1)), Quaternion.identity).GetComponent<MobObject>() as MobObject;
        Vector2Int startingPos = new Vector2Int(1, 16);
        MobData playerData = MobData.GeneratePlayer(startingPos);
        player.Init(playerData);
        this.mobList.Add(player);
    }

    void SnapToPosition(Vector2Int aOffset) {
        foreach (BlockObject block in this.selectedList) {
            Vector2Int newPos = block.pos + aOffset;
            block.transform.position = GameUtil.V2IOffsetV3(block.size, newPos);
            block.ghostPos = newPos;
        }
    }

    GameObject CreateBackground() {
        Vector3 backgroundOffset = new Vector3(0, 0, 1);
        Vector3 backgroundThiccness = new Vector3(0, 0, 0.1f);
        GameObject newBackground = Instantiate(this.backgroundMaster, GameUtil.V2IOffsetV3(this.levelData.boardSize, Vector2Int.zero) + backgroundOffset, Quaternion.identity);
        newBackground.transform.localScale =  GameUtil.V2IToV3(this.levelData.boardSize) + backgroundThiccness;
        return newBackground;
    }

    public void AddMarker(Vector2Int aPos, Color aColor) {
        Vector3 markerpos = GameUtil.V2IToV3(aPos) + new Vector3(0.5f, 0.75f, 0);
        GameObject marker = Instantiate(this.markerMaster, markerpos, Quaternion.identity);
        marker.GetComponent<Renderer>().material.color = aColor;
    }

    public void DestroyMarkers() {
        foreach (GameObject marker in GameObject.FindGameObjectsWithTag("Marker")) {
            GameObject.Destroy(marker);
        }
    }

    Vector3 GetMousePos() {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity)) {
            return new Vector3(hit.point.x, hit.point.y, hit.point.z);
        } else {
            return this.mousePos;
        }
    }

    // returns block occupying a grid position
    public static BlockObject GetBlockOnPosition(Vector2Int aPos) {
        foreach (BlockObject block in BoardManager.Instance.blockList) {
            if (block.IsInsideSelf(aPos)) {
                return block;
            }
        }
        return null;
    }

    public static MobObject GetMobOnPosition(Vector2Int aPos) {
        foreach (MobObject mob in BoardManager.Instance.mobList) {
            if (mob.IsInsideSelf(aPos)) {
                return mob;
            }
        }    
        return null;
    }

    public static BlockObject GetSelectedBlockOnPosition(Vector2Int aPos) {
        foreach (BlockObject block in BoardManager.Instance.selectedList) {
            if (block.IsInsideSelf(aPos)) {
                return block;
            }
        }
        return null;
    }

    // TODO optimize this to not do every frame
    void FixBlocksBelowEntity() {
        foreach (BlockObject block in this.tempFixedBlockList) {
            block.ResetColor();
            if (block.type != BlockTypeEnum.FIXED) {
                block.state = BlockStateEnum.ACTIVE;
            }
        }
        this.tempFixedBlockList = new List<BlockObject>();
        HashSet<BlockObject> blocksUnderPlayer = new HashSet<BlockObject>();
        for (int x = this.player.pos.x; x < this.player.pos.x + this.player.size.x; x++) {
            BlockObject maybeABlock = GetBlockOnPosition(new Vector2Int(x, this.player.pos.y - 1));
            if (maybeABlock != null && maybeABlock.type != BlockTypeEnum.FIXED) {
                blocksUnderPlayer.Add(maybeABlock);
            }
        }
        this.tempFixedBlockList = blocksUnderPlayer.ToList();
        foreach (BlockObject currentBlock in this.tempFixedBlockList) {
            currentBlock.SetState(BlockStateEnum.FIXED);
        }
    }

    // BLOCK SELECTION FUNCTIONS
    // returns true if block cant be pulled from the direction of isUp

    // make this less shitty later
    static bool CheckValidMove(Vector2Int aOffset, List<BlockObject> aSelectedList) {
        // print("CHECKING VALID MOVE");
        HashSet<Vector2Int> checkTopPositions = new HashSet<Vector2Int>();
        HashSet<Vector2Int> checkBotPositions = new HashSet<Vector2Int>();
        foreach (BlockObject block in aSelectedList) {
            // for each position inside block
            for (int x = 0; x < block.size.x; x++) {
                for (int y = 0; y < block.size.y; y++) {
                    Vector2Int currentPos = block.pos + aOffset + new Vector2Int(x, y);
                    // check if in bounds of level
                    if (!GameUtil.IsInside(currentPos, Vector2Int.zero, BoardManager.Instance.levelData.boardSize)) {
                        // print("IS BLOCKED! NOT IN LEVEL BOUNDS");
                        return false;
                    }
                    BlockObject maybeABlock = GetBlockOnPosition(currentPos);
                    // check if this position is occupied by something not itself
                    if (maybeABlock != null && !aSelectedList.Contains(maybeABlock)) {
                        // print("IS BLOCKED! INVALID MOVE!!!");
                        return false;
                    }
                    // check if a mob exists on this position
                    MobObject maybeAMob = GetMobOnPosition(currentPos);
                    if (maybeAMob != null) {
                        // print("IS BLOCKED! MOB EXISTS HERE");
                        return false;
                    }
                }
            }
            // fill in checktop/checkbot positions as a list of pos that need to be checked 
            for (int x = block.ghostPos.x; x < block.ghostPos.x + block.size.x; x++) {
                int aboveY = block.ghostPos.y + block.size.y;
                int belowY = block.ghostPos.y - 1;
                Vector2Int topPos = new Vector2Int(x, aboveY);
                Vector2Int botPos = new Vector2Int(x, belowY);
                foreach (BlockObject otherBlock in aSelectedList) {
                    if (GetSelectedBlockOnPosition(topPos) == null) {
                        // print("toppos does not contain a selected block");
                        Vector2Int checkPos = new Vector2Int(x,aboveY);
                        checkTopPositions.Add(checkPos);
                    }
                    if (GetSelectedBlockOnPosition(botPos) == null) {
                        // print("botpos does not contain a selected block");
                        Vector2Int checkPos = new Vector2Int(x,belowY);
                        checkBotPositions.Add(checkPos);
                    }
                }
            }
        }
        bool connectedOnTop = false;
        bool connectedOnBot = false;
        foreach (Vector2Int pos in checkTopPositions) {
            // print("examining pos " + pos);
            if (GetBlockOnPosition(pos) != null && !aSelectedList.Contains(GetBlockOnPosition(pos))) {
                connectedOnTop = true;
                // print("connected on top at" + pos);
            }
        }
        foreach (Vector2Int pos in checkBotPositions) {
            // print("examining pos " + pos);
            if (GetBlockOnPosition(pos) != null && !aSelectedList.Contains(GetBlockOnPosition(pos))) {
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

    static bool IsBlocked(bool aIsUp, BlockObject aBlock) {
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
                if (rBlock.state == BlockStateEnum.FIXED) {
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
                if (rBlock.state == BlockStateEnum.FIXED ) {
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
    static List<BlockObject> SelectUp(BlockObject aRootBlock) {
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
    static List<BlockObject> SelectDown(BlockObject aRootBlock) {
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
    static List<BlockObject> TreeUp(BlockObject aRootBlock) {
        List<BlockObject> treeUpList = new List<BlockObject>();
        treeUpRecursive(aRootBlock, treeUpList);
        return treeUpList;

        void treeUpRecursive(BlockObject block, List<BlockObject> list) {
            list.Add(block);
            foreach (BlockObject currentBlock in GetBlocksAbove(block)) {
                if (currentBlock.state == BlockStateEnum.ACTIVE && !list.Contains(currentBlock)) {
                    treeUpRecursive(currentBlock, list);
                }
            }
        }
    }

    // returns a list of rootBlock + blocks that are below rootBlock or connected to a block below rootBlock
    static List<BlockObject> TreeDown(BlockObject aRootBlock) {
        List<BlockObject> treeDownList = new List<BlockObject>();
        treeDownRecursive(aRootBlock, treeDownList);
        return treeDownList;

        void treeDownRecursive(BlockObject block, List<BlockObject> list) {
            list.Add(block);
            foreach (BlockObject currentBlock in GetBlocksBelow(block)) {
                if (currentBlock.state == BlockStateEnum.ACTIVE && !list.Contains(currentBlock)) {
                    treeDownRecursive(currentBlock, list);
                }
            }
        }
    }

    // returns whether or not rootBlock has a connection to a fixed block
    static bool IsConnectedToFixed(BlockObject aRootBlock, List<BlockObject> aIgnoreList) {
        bool isConnectedToFixed = false;
        List<BlockObject> ignoreListClone = new List<BlockObject>(aIgnoreList);
        IsConnectedToFixedRecursive(aRootBlock, ignoreListClone);
        return isConnectedToFixed;

        void IsConnectedToFixedRecursive(BlockObject rBlock, List<BlockObject> rIgnoreList) {
            rIgnoreList.Add(rBlock);
            if (rBlock.state == BlockStateEnum.FIXED) {
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
    static List<BlockObject> GetBlocksConnected(BlockObject aRootBlock, List<BlockObject> aIgnoreList) {
        List<BlockObject> connectedBlocks = new List<BlockObject>();
        List<BlockObject> ignoreListClone = new List<BlockObject>(aIgnoreList);
        // isRoot = true will ignore the root Block in the recursive function
        bool isRoot = true;
        GetBlocksConnectedRecursive(aRootBlock, ignoreListClone);
        return connectedBlocks;

        void GetBlocksConnectedRecursive(BlockObject rBlock, List<BlockObject> ignoreListX) {
            if (isRoot == false) {
                ignoreListX.Add(rBlock);
                connectedBlocks.Add(rBlock);
            }
            isRoot = false;
            foreach (BlockObject aboveBlock in GetBlocksAbove(rBlock)) {
                if (!ignoreListX.Contains(aboveBlock)) {
                    GetBlocksConnectedRecursive(aboveBlock, ignoreListX);
                }
            }
            foreach (BlockObject belowBlock in GetBlocksBelow(rBlock)) {
                if (!ignoreListX.Contains(belowBlock)) {
                    GetBlocksConnectedRecursive(belowBlock, ignoreListX);
                }
            }
        }

    }

    // returns a list of blocks directly above block
    static List<BlockObject> GetBlocksAbove(BlockObject aBlock) {
        HashSet<BlockObject> aboveSet = new HashSet<BlockObject>();
        for (int x = aBlock.pos.x; x < aBlock.pos.x + aBlock.size.x; x++) {
            int y = aBlock.pos.y + aBlock.size.y;
            Vector2Int currentPos = new Vector2Int(x,y);
            BlockObject maybeABlock = GetBlockOnPosition(currentPos);
            if (maybeABlock != null) {
                aboveSet.Add(maybeABlock);
            }
        }
        return aboveSet.ToList();
    }
    
    // returns a list of blocks directly below block
    static List<BlockObject> GetBlocksBelow(BlockObject aBlock) {
        HashSet<BlockObject> belowSet = new HashSet<BlockObject>();
        for (int x = aBlock.pos.x; x < aBlock.pos.x + aBlock.size.x; x++) {
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