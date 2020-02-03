
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
                UnGhostSelected();
                selectedList.Clear();
                break;
            case MouseStateEnum.CLICKED:
                if (mousePos.y < clickedPosition.y - 0.5) {
                    //dragging down
                    mouseState = MouseStateEnum.HOLDING;
                    //replace this with a recursive function later
                    TravelDownRoot(selectedBlock);
                    // selectedList.AddRange(GetBelowBlocks(selectedBlock));
                    GhostSelected();
                } else if (mousePos.y > clickedPosition.y + 0.5) {
                    //dragging up
                    mouseState = MouseStateEnum.HOLDING;
                    //replace this with a recursive function later
                    TravelUpRoot(selectedBlock);
                    // selectedList.AddRange(GetAboveBlocks(selectedBlock));
                    GhostSelected();
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

    // public List<BlockObject> GetSelection(bool isUp) {
    //     HashSet<BlockObject> blockSet = new HashSet<BlockObject>();

    //     switch (isUp) {
    //         case true:
    //             break;
    //         case false:
    //             break;
    //     }
    //     return blockSet.ToList();
    // }
    
    // this traveluproot and traveldownroot is testing to see if this tree of block is movable or attached to an anchor. 
    // if there are any falses in the testCol it means the whole tree is unmovable from that direction
    // does not explore past the initial tree yet

    public void TravelUpRoot(BlockObject rootBlock) {
        // testCols is a dict of x values and whether or not that column is blocked or not
        Dictionary<int, bool> testCols = new Dictionary<int, bool>();       
        // start recursive function
        TravelUp(rootBlock);    
        //debug shit
        string testOutput = "";     
        foreach (KeyValuePair<int, bool> kvp in testCols) {
            testOutput += kvp.Key.ToString() + " " + kvp.Value.ToString() + " ";
        }
        print(testOutput);
    
        void TravelUp(BlockObject block) {
            // set of blocks above this block 
            HashSet<BlockObject> rootSet = new HashSet<BlockObject>();
            for (int x = block.pos.x; x < block.pos.x + block.blockData.size.x; x++) {
                // iterate thru x vals for the length of the block and make currentPos 1 y above the top of the block
                Vector2Int currentPos = new Vector2Int(x, block.pos.y + block.blockData.size.y);
                // coodinates to draw some debug lines
                Vector3 currentPosLineOrigin = new Vector3(currentPos.x + 0.5f, currentPos.y - 0.25f, -1);
                Vector3 currentPosLineDestination = new Vector3(currentPos.x + 0.5f, currentPos.y + 0.25f, -1);
                // returns null if no block else returns a block at that location
                BlockObject maybeABlock = GetBlockOnPosition(currentPos);
                if (maybeABlock != null) {
                    switch (maybeABlock.blockData.type) {
                        case BlockTypeEnum.FREE:
                            Debug.DrawLine(currentPosLineOrigin, currentPosLineDestination, Color.white, 10f);
                            TravelUp(maybeABlock);
                            testCols[x] = true;
                            break;
                        case BlockTypeEnum.FIXED:
                            Debug.DrawLine(currentPosLineOrigin, currentPosLineDestination, Color.red, 10f);
                            testCols[x] = false;
                            break;
                    }
                }
            }
        }
    }

    public void TravelDownRoot(BlockObject rootBlock) {
        Dictionary<int, bool> testCols = new Dictionary<int, bool>();
        TravelDown(rootBlock);
        string testOutput = "";
        foreach (KeyValuePair<int, bool> kvp in testCols) {
            testOutput += kvp.Key.ToString() + " " + kvp.Value.ToString() + " ";
        }
        print(testOutput);
    
        void TravelDown(BlockObject block) {
            HashSet<BlockObject> rootSet = new HashSet<BlockObject>();
            for (int x = block.pos.x; x < block.pos.x + block.blockData.size.x; x++) {
                testCols[x] = true;
                Vector2Int currentPos = new Vector2Int(x, block.pos.y - 1);
                Vector3 currentPosLineOrigin = new Vector3(currentPos.x + 0.5f, currentPos.y + 1.25f, -1);
                Vector3 currentPosLineDestination = new Vector3(currentPos.x + 0.5f, currentPos.y + 0.75f, -1);
                BlockObject maybeABlock = GetBlockOnPosition(currentPos);
                if (maybeABlock != null) {
                    switch (maybeABlock.blockData.type) {
                        case BlockTypeEnum.FREE:
                            Debug.DrawLine(currentPosLineOrigin, currentPosLineDestination, Color.yellow, 10f);
                            TravelDown(maybeABlock);
                            break;
                        case BlockTypeEnum.FIXED:
                            Debug.DrawLine(currentPosLineOrigin, currentPosLineDestination, Color.red, 10f);
                            testCols[x] = false;
                            break;
                    }
                }
            }
            
        }
    }

    public List<BlockObject> GetAboveBlocks(BlockObject rootBlock) {
        print("GetAboveBlocks started with" + rootBlock.name);
        // make a  set for all the blocks  found
        HashSet<BlockObject> blockSet = new HashSet<BlockObject>();
        // if block is fixed return empty list
        if (rootBlock.blockData.type == BlockTypeEnum.FIXED) {
            print("YOU SELECTED A FIXED BLOCK");
            return blockSet.ToList();
        }
        // add root block
        blockSet.Add(rootBlock);

        HashSet<BlockObject> rootAbove = new HashSet<BlockObject>();
        for (int x = rootBlock.pos.x; x < rootBlock.pos.x + rootBlock.blockData.size.x; x++) {
            Vector2Int currentPos = new Vector2Int(x, rootBlock.pos.y + rootBlock.blockData.size.y);
            BlockObject maybeABlock = GetBlockOnPosition(currentPos);
            if (maybeABlock != null) {
                if (maybeABlock.blockData.type == BlockTypeEnum.FREE) {
                    // add this block to above set for root
                  rootAbove.Add(maybeABlock);
                }
                
            }
        }
        // add blocks above current block to blockset
        blockSet.UnionWith(rootAbove);

        // blockset should now contain the root block and all the blocks above it
        // rootAbove should now contain only the blocks above root

        // for each block above root
        foreach (BlockObject currentBlock in rootAbove) {
            if (currentBlock.blockData.type == BlockTypeEnum.FREE) {
                Debug.DrawLine(rootBlock.transform.position + new Vector3(0,0,-1), currentBlock.transform.position + new Vector3(0,0,-1), Color.blue, 5f);
               GetAboveRecursive(currentBlock);
            }
        }
        return blockSet.ToList();

        // this recursive function mutates 
        void GetAboveRecursive(BlockObject block) {
            print("executed GetAboveBlock on " + block.name);
            HashSet<BlockObject> aboveBlockSet = new HashSet<BlockObject>();
            for (int x = block.pos.x; x < block.pos.x + block.blockData.size.x; x++) {
                Vector2Int currentPos = new Vector2Int(x, block.pos.y + block.blockData.size.y);
                // print("checking" + currentPos);
                BlockObject maybeABlock = GetBlockOnPosition(currentPos);
                if (maybeABlock != null) {
                    if (maybeABlock.blockData.type == BlockTypeEnum.FREE) {
                        // add this block to above set for root
                        aboveBlockSet.Add(maybeABlock);
                    }
                }
            }
            // aboveBlockSet now has all the blocks above this one
            blockSet.UnionWith(aboveBlockSet);
            // now do it recursively
            foreach (BlockObject currentBlock in aboveBlockSet) {
                Debug.DrawLine(block.transform.position + new Vector3(0,0,-1), currentBlock.transform.position + new Vector3(0,0,-1), Color.red, 5f);
                GetAboveRecursive(currentBlock);
            }
        }
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


