using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.EventSystems;

public class BoardManager : Singleton<BoardManager> {
    public LevelData levelData;
    public GameManager gameManager;
    public List<BlockObject> blockList;
    public List<BlockObject> tempFixedBlockList;
    public List<MobObject> mobList;

    public MobObject player;

    public BlockObject blockMaster;
    public MobObject mobMaster;
    public MobObject playerMaster;
    public GameObject backgroundMaster;

    
    void Awake() {
        this.gameManager = GameManager.Instance;
    }

    void Update() {
        // TODO: make this call the frame an entity moves to a new position and not here
        FixBlocksBelowEntity();
        switch (this.gameManager.mouseState) {
            case MouseStateEnum.DEFAULT:
                break;
            case MouseStateEnum.CLICKED:
                // runs once for one frame before mouseState changes to HELD
                break;
            case MouseStateEnum.HELD:
                break;
            case MouseStateEnum.UNCLICKED:
                // runs once for one frame before mouseState changes to DEFAULT
                break;
        }
    }

    public void Init(LevelData aLevelData) {
        this.levelData = aLevelData;
        Vector3 backgroundOffset = new Vector3(0, 0, 1);
        Vector3 backgroundThiccness = new Vector3(0, 0, 0.1f);
        GameObject newBackground = Instantiate(this.backgroundMaster, GameUtil.V2IOffsetV3(this.levelData.boardSize, Vector2Int.zero) + backgroundOffset, Quaternion.identity);
        newBackground.transform.localScale =  GameUtil.V2IToV3(this.levelData.boardSize) + backgroundThiccness;
        newBackground.name = "Background";
        foreach (BlockData blockData in aLevelData.blockDataList) {
            BlockObject newBlockObject = Instantiate(this.blockMaster, GameUtil.V2IOffsetV3(blockData.size, blockData.pos), Quaternion.identity);
            newBlockObject.transform.parent = this.transform;
            newBlockObject.Init(blockData);
            this.blockList.Add(newBlockObject);
        }
        foreach (MobData mobData in aLevelData.mobDataList) {
            MobObject mobPrefab = this.mobMaster; 
            switch (mobData.mobPrefabName) {
                case "Player":
                        mobPrefab = this.playerMaster;
                    break;
            }
            
            MobObject newMobObject = Instantiate(mobPrefab, GameUtil.V2IOffsetV3(mobData.size, mobData.pos), Quaternion.identity);
            newMobObject.transform.parent = this.transform;
            newMobObject.Init(mobData);
            this.mobList.Add(newMobObject);
            if (newMobObject.tag == "MyPlayer") {
                this.player = newMobObject;
            }
        }
    }

    public void AddBlock(BlockObject aBlock, Vector2Int aPos) {
        aBlock.pos = aPos;
        this.blockList.Add(aBlock);
    }

    public void RemoveEntity(EntityObject aEntity) {
        if (aEntity is BlockObject) {
            blockList.Remove(aEntity as BlockObject);
        } else if (aEntity is MobObject) {
            mobList.Remove(aEntity as MobObject);
        }
        Destroy(aEntity.gameObject);
    }

    public static bool CanAddBlockHere(BlockObject aBlock, Vector2Int aPos) {
        for (int x = aPos.x; x < aPos.x + aBlock.size.x; x++) {
            for (int y = aPos.y; y < aPos.y +aBlock.size.y; y++) {
                Vector2Int currentPos = new Vector2Int(x, y);
                // if block is out of bounds
                if (!GameUtil.IsInside(currentPos, Vector2Int.zero, BoardManager.Instance.levelData.boardSize)) {
                    return false;
                }
                // if block is blocked by another block or an entity
                foreach (BlockObject block in BoardManager.Instance.blockList) {
                    if (block.IsInsideSelf(currentPos)) {
                        return false;
                    }
                }
                foreach (MobObject mob in BoardManager.Instance.mobList) {
                    if (mob.IsInsideSelf(currentPos)) {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    public void FixBlocksBelowEntity() {
        foreach (BlockObject block in this.tempFixedBlockList) {
            block.ResetColor();
            block.isUnderEntity = false;
            block.state = BlockStateEnum.ACTIVE;
        }
        this.tempFixedBlockList = new List<BlockObject>();
        HashSet<BlockObject> blocksUnderEntities = new HashSet<BlockObject>();
        foreach (MobObject mobObject in mobList) {
            if (!mobObject.canSelectUnder) {
                for (int x = mobObject.pos.x; x < mobObject.pos.x + mobObject.size.x; x++) {
                    BlockObject maybeABlock = GetBlockOnPosition(new Vector2Int(x, mobObject.pos.y - 1));
                    if (maybeABlock != null && maybeABlock.state == BlockStateEnum.ACTIVE) {
                        maybeABlock.isUnderEntity = true;
                        maybeABlock.state = BlockStateEnum.FIXED;
                        blocksUnderEntities.Add(maybeABlock);
                    }
                }
            }
        }
        this.tempFixedBlockList = blocksUnderEntities.ToList();
    }

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

    public static EntityObject GetEntityOnPosition(Vector2Int aPos) {
        EntityObject maybeAEntity = GetBlockOnPosition(aPos);
        if (maybeAEntity == null) {
            maybeAEntity = GetMobOnPosition(aPos);
        }
        return maybeAEntity;
    }

    public static bool CheckValidMove(Vector2Int aOffset, List<BlockObject> aSelectedList) {
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
                    if (PlayingManager.GetSelectedBlockOnPosition(topPos) == null) {
                        // print("toppos does not contain a selected block");
                        Vector2Int checkPos = new Vector2Int(x,aboveY);
                        checkTopPositions.Add(checkPos);
                    }
                    if (PlayingManager.GetSelectedBlockOnPosition(botPos) == null) {
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

    public static bool IsBlocked(bool aIsUp, BlockObject aBlock) {
        bool isBlocked = false;
        List<BlockObject> ignoreList = new List<BlockObject>();
        if (aIsUp) {
            CheckSelectUpRecursive(aBlock, ignoreList);
            foreach (BlockObject selectedBlock in SelectUp(aBlock)) {
                if (selectedBlock.isUnderEntity) {
                    print("selection blocked because " + selectedBlock.name + " is under an entity");
                    isBlocked = true;
                }
            }
        } else {
            CheckSelectDownRecursive(aBlock, ignoreList);
            foreach (BlockObject selectedBlock in SelectDown(aBlock)) {
                if (selectedBlock.isUnderEntity) {
                    print("selection blocked because " + selectedBlock.name + " is under an entity");
                    isBlocked = true;
                }
            }
        }
        return isBlocked;

        void CheckSelectUpRecursive(BlockObject rBlock, List<BlockObject> rIgnoreList) {
            if (isBlocked == false) {
                rIgnoreList.Add(rBlock);
                if (rBlock.state == BlockStateEnum.FIXED  || rBlock.isUnderEntity == true) {
                    if (rBlock.isUnderEntity == true) {
                        print("BoardManager - CheckSelectUpRecursive encountered block under entity");
                    }
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
                if (rBlock.state == BlockStateEnum.FIXED || rBlock.isUnderEntity == true) {
                    if (rBlock.isUnderEntity == true) {
                        print("BoardManager - CheckSelectDownRecursive encountered block under entity");
                    }
                    isBlocked = true;
                    return;
                }
                foreach (BlockObject belowBlock in GetBlocksBelow(rBlock)) {
                    if (!rIgnoreList.Contains(belowBlock)) {
                        CheckSelectDownRecursive(belowBlock, rIgnoreList);
                    }
                }
            }
        }
    }

    // returns a list of blocks selected when dragging up on a block
    public static List<BlockObject> SelectUp(BlockObject aRootBlock) {
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
    public static List<BlockObject> SelectDown(BlockObject aRootBlock) {
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
    public static List<BlockObject> TreeUp(BlockObject aRootBlock) {
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
    public static List<BlockObject> TreeDown(BlockObject aRootBlock) {
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
    public static bool IsConnectedToFixed(BlockObject aRootBlock, List<BlockObject> aIgnoreList) {
        bool isConnectedToFixed = false;
        List<BlockObject> ignoreListClone = new List<BlockObject>(aIgnoreList);
        IsConnectedToFixedRecursive(aRootBlock, ignoreListClone);
        return isConnectedToFixed;

        void IsConnectedToFixedRecursive(BlockObject rBlock, List<BlockObject> rIgnoreList) {
            rIgnoreList.Add(rBlock);
            // OK so the reason why this uses BlockTypeEnum.FIXED is because
            // a block with state = BlockStateEnum.FIXED is not always gonna be fixed
            // so this we need to use BlockTypeEnum.FIXED or else floaters will happen
            if (rBlock.type == BlockTypeEnum.FIXED) {
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
    public static List<BlockObject> GetBlocksConnected(BlockObject aRootBlock, List<BlockObject> aIgnoreList) {
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
    public static List<BlockObject> GetBlocksAbove(BlockObject aBlock) {
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
    public static List<BlockObject> GetBlocksBelow(BlockObject aBlock) {
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
