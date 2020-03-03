using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public EntityData entityData;
    public EntityTypeEnum entityType;
    public Vector2Int pos;
    public Color color;

    void Move(Vector2Int newPos) {
        Vector3 newV3Pos = GameUtil.V2IOffsetV3(this.entityData.size, newPos);
        print("moving from " + transform.position + " to " + newV3Pos);
        Debug.DrawLine(transform.position, newV3Pos, Color.red, 1f);
        StartCoroutine(MoveCoroutine(newV3Pos));
        this.pos = newPos;
    }

    public IEnumerator MoveCoroutine(Vector3 targetPos) {
        Vector3 currentPos = transform.position;
        float t = 0f;
        while (t < 1) {
            t += Time.deltaTime / 1f;
            transform.position = Vector3.Lerp(currentPos, targetPos, t);
            yield return null;
        }
    }

    void Awake() {
        this.pos = new Vector2Int(0,0);
        this.color = Color.yellow;
        GetComponent<Renderer>().material.color = this.color;
        this.entityData = new EntityData();
        this.entityData.Init(new Vector2Int(2,3), EntityTypeEnum.PLAYER);
    }
    // Start is called before the first frame update
    void Start()
    {
        Move(new Vector2Int(12,1));   
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
