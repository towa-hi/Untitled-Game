using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorManager : Singleton<EditorManager> {

    public BlockObject blockGhost;
    
    public SelectionStateEnum selectionState;
    void Awake() {
        this.selectionState = SelectionStateEnum.DEFAULT;
    }

    void Update() {
        switch (GameManager.Instance.mouseState){ 
            case MouseStateEnum.DEFAULT:
                break;
            case MouseStateEnum.CLICKED:
                break;
            case MouseStateEnum.HELD:
                switch (this.selectionState) {
                    case SelectionStateEnum.DEFAULT:
                        break;
                    case SelectionStateEnum.HOLDING:
                        SnapGhostToPosition(GameUtil.V3ToV2I(GameManager.Instance.mousePos));
                        break;
                    case SelectionStateEnum.INVALID:
                        break;
                }
                break;
            case MouseStateEnum.UNCLICKED:
                switch (this.selectionState) {
                    case SelectionStateEnum.DEFAULT:
                        break;
                    case SelectionStateEnum.HOLDING:
                        break;
                    case SelectionStateEnum.INVALID:
                        break;
                }
                break;
        }
    }

    public void BlockSelectorItemOnPointerDown(BlockData aBlockGhostData) {
        print("EditorManager - PreviewBlock");
        this.blockGhost = Instantiate(BoardManager.Instance.blockMaster);
        this.blockGhost.Init(aBlockGhostData);
        this.selectionState = SelectionStateEnum.HOLDING;
    }

    public void BlockSelectorItemOnPointerUp() {
        if (BoardManager.CanAddBlockHere(this.blockGhost, GameUtil.V3ToV2I(GameManager.Instance.mousePos))) {
            BoardManager.Instance.AddBlock(this.blockGhost, GameUtil.V3ToV2I(GameManager.Instance.mousePos));
            this.blockGhost.transform.SetParent(transform);
        } else {
            Destroy(this.blockGhost.gameObject);
        }
        this.selectionState = SelectionStateEnum.DEFAULT;
        this.blockGhost = null;

    }
    void SnapGhostToPosition(Vector2Int aPos) {
        // TODO: add hysteresis
        this.blockGhost.transform.position = GameUtil.V2IOffsetV3(this.blockGhost.size, aPos);
        this.blockGhost.ghostPos = aPos;
    }
    
}
