using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class PlayingManager : Singleton<PlayingManager> {
    public List<BlockObject> selectedList;
    public BlockObject clickedBlock;
    public SelectionStateEnum selectionState = SelectionStateEnum.DEFAULT;
    public TimeStateEnum timeState;

    public static float dragThreshold = 0.2f;

    void Awake() {
        this.timeState = TimeStateEnum.NORMAL;
    }
    void Update() {
        switch (GameManager.Instance.mouseState){ 
            case MouseStateEnum.DEFAULT:
                break;
            case MouseStateEnum.CLICKED:
                // this should only happen for one frame before mouseState changes to HELD
                this.clickedBlock = GetClickedBlock();
                break;
            case MouseStateEnum.HELD:
                switch (this.selectionState) {
                    case SelectionStateEnum.DEFAULT:
                        if (this.clickedBlock != null && this.clickedBlock.state == BlockStateEnum.ACTIVE) {
                            if (GameManager.Instance.dragOffset.y > PlayingManager.dragThreshold) {
                                // dragging up
                                print("dragging up block");
                                PauseTime();
                                SelectBlocks(BoardManager.SelectUp(this.clickedBlock));
                                this.selectionState = SelectionStateEnum.HOLDING;
                            } else if (GameManager.Instance.dragOffset.y < PlayingManager.dragThreshold * -1) {
                                // dragging down
                                print("dragging down block");
                                PauseTime();
                                SelectBlocks(BoardManager.SelectDown(this.clickedBlock));
                                this.selectionState = SelectionStateEnum.HOLDING;
                            }
                        }
                        break;
                    case SelectionStateEnum.HOLDING:
                        // move blocks
                        MoveSelected();
                        break;
                    case SelectionStateEnum.INVALID:
                        // move blocks
                        MoveSelected();
                        break;
                }
                break;
            case MouseStateEnum.UNCLICKED:
                // runs once for one frame before mouseState changes to DEFAULT
                print("PlayingManager - unclicked");
                this.clickedBlock = null;
                switch (this.selectionState) {
                    case SelectionStateEnum.DEFAULT:
                        break;
                    case SelectionStateEnum.HOLDING:
                        foreach (BlockObject selectedBlock in this.selectedList) {
                            selectedBlock.pos = selectedBlock.ghostPos;
                            selectedBlock.SetState(BlockStateEnum.ACTIVE);
                        }
                        this.selectedList.Clear();
                        this.selectionState = SelectionStateEnum.DEFAULT;
                        break;
                    case SelectionStateEnum.INVALID:
                        foreach (BlockObject selectedBlock in this.selectedList) {
                            selectedBlock.SmoothMove(GameUtil.V2IOffsetV3(selectedBlock.size, selectedBlock.pos));
                            selectedBlock.ghostPos = selectedBlock.pos;
                            selectedBlock.SetState(BlockStateEnum.ACTIVE);
                        }
                        this.selectedList.Clear();
                        this.selectionState = SelectionStateEnum.DEFAULT;
                        break;
                }
                ResumeTime();
                break;
                
        }
        
    }

    void PauseTime() {
        this.timeState = TimeStateEnum.PAUSED;
        // volume.GetComponent<PostProcessVolume>().enabled = true;
        // volume.GetComponent<PostProcessVolume>().isGlobal = true;
    }

    void ResumeTime() {
        this.timeState = TimeStateEnum.NORMAL;
        // volume.GetComponent<PostProcessVolume>().enabled = false;
        // volume.GetComponent<PostProcessVolume>().isGlobal = false;
    }
    void MoveSelected() {
        Vector2Int dragOffsetInt = GameUtil.V3ToV2I(GameManager.Instance.dragOffset);
        Vector2Int oldDragOffsetInt = GameUtil.V3ToV2I(GameManager.Instance.oldDragOffset);
        if (dragOffsetInt != oldDragOffsetInt) {
            SnapToPosition(dragOffsetInt);
            if (BoardManager.CheckValidMove(dragOffsetInt, this.selectedList)) {
                this.selectionState = SelectionStateEnum.HOLDING;
                foreach (BlockObject selectedBlock in this.selectedList) {
                    selectedBlock.SetState(BlockStateEnum.GHOST);
                }
            } else {
                this.selectionState = SelectionStateEnum.INVALID;
                foreach (BlockObject selectedBlock in this.selectedList) {
                    selectedBlock.SetState(BlockStateEnum.INVALID);
                }
            }
        }
    }

    void SnapToPosition(Vector2Int aOffset) {
        // TODO: add hysteresis
        foreach (BlockObject block in this.selectedList) {
            Vector2Int newPos = block.pos + aOffset;
            block.transform.position = GameUtil.V2IOffsetV3(block.size, newPos);
            block.ghostPos = newPos;
        }
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

    void SelectBlocks(List<BlockObject> aBlockList) {
        this.selectedList = aBlockList;
        foreach (BlockObject block in this.selectedList) {
            block.SetState(BlockStateEnum.GHOST);
        }
    }

    public static BlockObject GetSelectedBlockOnPosition(Vector2Int aPos) {
        foreach (BlockObject block in PlayingManager.Instance.selectedList) {
            if (block.IsInsideSelf(aPos)) {
                return block;
            }
        }
        return null; 
    }
    // BLOCK SELECTION FUNCTIONS
    // returns true if block cant be pulled from the direction of isUp

    // make this less shitty later

}
