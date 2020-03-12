using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface EntityInterface {
    Vector2Int GetSize();
    Vector2Int GetPos();
    bool IsInsideSelf(Vector2Int aPos);
}
