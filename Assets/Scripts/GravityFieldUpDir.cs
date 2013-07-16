using UnityEngine;
using System.Collections;

public class GravityFieldUpDir : GravityFieldBase {

    protected override Vector3 GetUpVector(Vector3 position) {
        return transform.up;
    }
}
