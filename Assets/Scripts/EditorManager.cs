using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorManager : Singleton<EditorManager> {

    public BlockObject blockGhost;
    
    public SelectionStateEnum selectionState;
    public EditModeEnum editMode;
    public GameObject placeView;
    public GameObject removeView;
    public GameObject editView;
    public EntityObject oldHoveredEntity;
    public EntityObject hoveredEntity;

    void Awake() {
        this.selectionState = SelectionStateEnum.DEFAULT;
        this.editMode = EditModeEnum.ADD;
    }

    void Update() {
        switch(this.editMode) {
            case EditModeEnum.ADD:
                SwitchEditModeAdd();
                break;
            case EditModeEnum.REMOVE:
                SwitchEditModeRemove();
                break;
            case EditModeEnum.SELECT:
                
                break;
        }
    }

    void SwitchEditModeAdd() {
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
    // TODO: move this preview highlight stuff to BoardManager
    void SwitchEditModeRemove() {
        switch (GameManager.Instance.mouseState) {
            case MouseStateEnum.DEFAULT:
                this.oldHoveredEntity = this.hoveredEntity;
                this.hoveredEntity = BoardManager.GetEntityOnPosition(GameUtil.V3ToV2I(GameManager.Instance.mousePos));
                if (this.hoveredEntity != this.oldHoveredEntity) {
                    if (this.oldHoveredEntity != null) {
                        foreach (Vector2Int pos in this.oldHoveredEntity.GetOccupiedPos()) {
                            MyGrid.Instance.ResetCell(pos);
                        }
                    }
                    
                    if (this.hoveredEntity != null) {
                        foreach (Vector2Int pos in this.hoveredEntity.GetOccupiedPos()) {
                            MyGrid.Instance.SetCell(pos, Color.red);
                        }
                    }
                }
                break;
            case MouseStateEnum.CLICKED:
                EntityObject clickedEntity = BoardManager.GetEntityOnPosition(GameUtil.V3ToV2I(GameManager.Instance.clickedPos));
                if (clickedEntity != null) {
                    BoardManager.Instance.RemoveEntity(clickedEntity);
                    // BoardManager.Instance.HighlightEntityPosition(clickedEntity);
                    foreach (Vector2Int pos in hoveredEntity.GetOccupiedPos()) {
                        MyGrid.Instance.ResetCell(pos);
                    }
                }
                break;
        }
    }

    public void BlockSelectorItemOnPointerDown(BlockData aBlockGhostData) {
        if (editMode == EditModeEnum.ADD && selectionState == SelectionStateEnum.DEFAULT) {
            print("EditorManager - PreviewBlock");
            this.blockGhost = Instantiate(BoardManager.Instance.blockMaster);
            this.blockGhost.Init(aBlockGhostData);
            this.selectionState = SelectionStateEnum.HOLDING;
        } else {
            throw new System.Exception("Block selector item activated but editMode != ADD");
        }
        
    }

    public void BlockSelectorItemOnPointerUp() {
        if (editMode == EditModeEnum.ADD && selectionState == SelectionStateEnum.HOLDING) {
            if (BoardManager.CanAddBlockHere(this.blockGhost, GameUtil.V3ToV2I(GameManager.Instance.mousePos))) {
                BoardManager.Instance.AddBlock(this.blockGhost, GameUtil.V3ToV2I(GameManager.Instance.mousePos));
                this.blockGhost.transform.SetParent(transform);
            } else {
                Destroy(this.blockGhost.gameObject);
            }
            this.selectionState = SelectionStateEnum.DEFAULT;
            this.blockGhost = null;
        } else {
            throw new System.Exception("Block selector block preview let go but editMode != ADD");
        }
    }

    void SnapGhostToPosition(Vector2Int aPos) {
        // TODO: add hysteresis
        this.blockGhost.transform.position = GameUtil.V2IOffsetV3(this.blockGhost.size, aPos);
        this.blockGhost.ghostPos = aPos;
    }

    public void SetEditMode(EditModeEnum aEditMode) {
        print("EditorManager - EditMode changed to " + aEditMode.ToString());
        this.editMode = aEditMode;
        this.selectionState = SelectionStateEnum.DEFAULT;
        this.blockGhost = null;
        switch (this.editMode) {
            case EditModeEnum.ADD:
                this.placeView.SetActive(true);
                break;
            case EditModeEnum.REMOVE:
                this.placeView.SetActive(false);
                // this.placeView.enabled = false;
                break;
            case EditModeEnum.SELECT:
                this.placeView.SetActive(false);
                // this.placeView.enabled = false;
                break;
        }
    }

    public void OnPlaceButtonClick() {
        SetEditMode(EditModeEnum.ADD);
    }

    public void OnRemoveButtonClick() {
        SetEditMode(EditModeEnum.REMOVE);
    }

    public void OnEditButtonClick() {
        SetEditMode(EditModeEnum.SELECT);
    }
    
}
