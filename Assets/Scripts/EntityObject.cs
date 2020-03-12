using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityObject : MonoBehaviour {

    public EntityData entityData;
    public Vector2Int pos;
    public Vector2Int objectPos;
    public Vector2Int destination;
    public EntityStateEnum state;
    public FacingEnum facing;
    public float moveSpeed;
    public float turnSpeed;

    void Awake() {
        // setting speed for now
        this.moveSpeed = 1f;
        this.turnSpeed = 1f;
    
        this.facing = FacingEnum.RIGHT;
        this.entityData = ScriptableObject.CreateInstance("EntityData") as EntityData;
        this.entityData.Init(new Vector2Int(2,3), EntityTypeEnum.PLAYER);
        this.state = EntityStateEnum.DEFAULT;
        // set color to yellow
        this.GetComponent<Renderer>().material.color = Color.yellow;
    }

    public void Init(Vector2Int startingPos) {
        this.pos = startingPos;
        this.objectPos = startingPos;
        this.transform.position = GameUtil.V2IOffsetV3(this.entityData.size, this.pos);
    }
    // Start is called before the first frame update
    void Start() {
        // Move(new Vector2Int(12,1));

        StartCoroutine(DoNext());
    }

    // Update is called once per frame
    void Update() {
        
    }

    void SetState(EntityStateEnum newState) {
        this.state = newState;
        switch (newState) {
            case EntityStateEnum.DEFAULT:
                break;
            case EntityStateEnum.FALLING:
                break;
            case EntityStateEnum.JUMPING:
                break;
            case EntityStateEnum.LANDING:
                break;
            case EntityStateEnum.WALKING:
                break;
        }
    } 
    IEnumerator DoNext() {
        print("ai started");
        Vector2Int offset = new Vector2Int(0,0);
        Vector2Int destination = new Vector2Int(0,0);
        switch (this.facing) {
            case FacingEnum.RIGHT:
                offset = new Vector2Int(1,0);
                destination = this.pos + offset;
                break;
            case FacingEnum.LEFT:
                offset = new Vector2Int(-1, 0);
                destination = this.pos + offset;
                break;
        }
        if (CheckAdjacentDirection(offset) && CheckFloor(offset)) {
            print("right path clear");
            yield return StartCoroutine(MoveCoroutine(GameUtil.V2IOffsetV3(this.entityData.size, destination)));
            this.pos = destination;
            BoardManager.Instance.DestroyMarkers();
            print(" move coroutine done");

            yield return StartCoroutine(DoNext());
        } else {
            print("path blocked, rotating");
            yield return StartCoroutine(TurnCoroutine());
            if (this.facing == FacingEnum.RIGHT) {
                this.facing = FacingEnum.LEFT;
            } else if (this.facing == FacingEnum.LEFT) {
                this.facing = FacingEnum.RIGHT;
            }
            yield return StartCoroutine(DoNext());
            // if 1 up is free, jump there
            // if 1 down is free jump there
            // if neither are free turn around
        }
    }

    // TODO: figure out why this is fast
    // TODO: this only rotates once because 180f is hard coded (maybe??)
    public IEnumerator TurnCoroutine() {
        print("turning around");
        float t = 0f;
        Quaternion currentRotation = this.transform.rotation;
        Quaternion newRotation = Quaternion.AngleAxis(180, Vector3.up) * currentRotation;
        while (t < 1) {
            t += Time.deltaTime / turnSpeed;
            float rotation = Mathf.Lerp(transform.eulerAngles.y, 180f, t);
            this.transform.rotation = Quaternion.Lerp(currentRotation, newRotation, t);
            // transform.eulerAngles = new Vector3(transform.eulerAngles.x, rotation, transform.eulerAngles.z);
            yield return null;
        }
        this.transform.rotation = newRotation;

        
    }

    // TODO: figure out why this is slow and what moveSpeed actually does
    public IEnumerator MoveCoroutine(Vector3 targetPos) {
        print("moving from " + transform.position + " to " + targetPos);
        Vector3 currentPos = transform.position;
        Debug.DrawLine(transform.position, targetPos, Color.red, 1f);
        float t = 0f;
        while (t < 1) {
            t += Time.deltaTime / moveSpeed;
            transform.position = Vector3.Lerp(currentPos, targetPos, t);
            yield return null;
        }
    }

    public List<Vector2Int> GetOccupiedPos() {
        List<Vector2Int> posList = new List<Vector2Int>();
        for (int x = this.pos.x; x < this.pos.x + this.entityData.size.x; x++) {
            for (int y = this.pos.y; y < this.pos.y + this.entityData.size.y; y++) {
                posList.Add(new Vector2Int(x, y));
            }
        }
        return posList;
    }

    public bool CheckAdjacentDirection(Vector2Int offset) {
        bool isFree = true;
        List<Vector2Int> checkPosList = GetOccupiedPos();
        foreach(Vector2Int currentPos in checkPosList) {
            Vector2Int checkPos = currentPos + offset;
            BlockObject maybeABlock = BoardManager.Instance.GetBlockOnPosition(checkPos);
            
            if (maybeABlock != null) {
                BoardManager.Instance.AddMarker(checkPos, Color.red);
                isFree = false;
            } else {
                BoardManager.Instance.AddMarker(checkPos, Color.green);
            }
        }
        return isFree;
    }

    // check if floor exists under this position
    public bool CheckFloor(Vector2Int offset) {
        bool hasFloor = false;
        for (int x = pos.x + offset.x; x < pos.x + offset.x + entityData.size.x; x++) {
            Vector2Int checkPos = new Vector2Int(x, pos.y - 1);
            BlockObject maybeABlock = BoardManager.Instance.GetBlockOnPosition(checkPos);
            if (maybeABlock != null) {
                hasFloor = true;
                BoardManager.Instance.AddMarker(checkPos, Color.red);
            } else {
                BoardManager.Instance.AddMarker(checkPos, Color.green);
            }
        }
        return hasFloor;
    }

    public bool CheckSelfObjectPos(Vector2Int pos) {
        if (pos.x >= this.objectPos.x && pos.x < this.objectPos.x + this.entityData.size.x && pos.y >= this.objectPos.y && pos.y < this.objectPos.y + this.entityData.size.y) {
            return true;
        } else {
            return false;
        }
    }
}
