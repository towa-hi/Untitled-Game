using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : Singleton<GameManager> {
    public BoardManager boardManager;

    public Vector3 mousePos;
    public Vector3 oldMousePos;
    public Vector3 clickedPos;
    public Vector3 dragOffset;
    public Vector3 oldDragOffset;

    public GameModeEnum gameMode;
    public MouseStateEnum mouseState = MouseStateEnum.DEFAULT;
    
    void Awake() {
        boardManager.Init(LevelData.GenerateTestLevel());
    }

    void Update() {
        //set mousePos for this frame.
        this.oldMousePos = this.mousePos;
        this.mousePos = GetMousePos();
        // this switch must run first. 
        // changes CLICKED to HELD and UNCLICKED to DEFAULT after a new frame begins
        switch (this.mouseState) {
            case MouseStateEnum.CLICKED:
                this.mouseState = MouseStateEnum.HELD;
                break;
            case MouseStateEnum.UNCLICKED:
                this.dragOffset = Vector3.zero;
                this.clickedPos = Vector3.zero;
                this.mouseState = MouseStateEnum.DEFAULT;
                break;
        }
        // detect mouse button down and up to set CLICKED and UNCLICKED
        if (Input.GetMouseButtonDown(0)) {
            this.mouseState = MouseStateEnum.CLICKED;
        } else if (Input.GetMouseButtonUp(0)) {
            this.mouseState = MouseStateEnum.UNCLICKED;
        }

        switch (this.mouseState) {
            case MouseStateEnum.DEFAULT:
                break;
            case MouseStateEnum.CLICKED:
                // runs once for one frame before mouseState changes to HELD
                print("GameManger - clicked:" + this.mousePos);
                this.clickedPos = this.mousePos;
                OnClickDown();
                break;
            case MouseStateEnum.HELD:
                this.oldDragOffset = this.dragOffset;
                this.dragOffset = this.mousePos - this.clickedPos;
                OnHoldUpdate();
                break;
            case MouseStateEnum.UNCLICKED:
                // this should only happen for one frame before mouseState changes to DEFAULT
                print("GameManger - unclicked:" + this.mousePos +  " offset:" + this.dragOffset);
                OnUnclick();
                break;
        }
    }

    public void OnClickDown() {
    }

    public void OnHoldUpdate() {
    }

    public void OnUnclick() {
    }

    public Vector3 GetMousePos() {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity)) {
            return new Vector3(hit.point.x, hit.point.y, hit.point.z);
        } else {
            return this.mousePos;
        }
    }

}
