using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemDrop : MonoBehaviour, IComparer<ItemDrop.Data> {
    [System.Serializable]
    public class Data {
        public int id = -1;
        public float weight = 1.0f;

        private float mRange = 0.0f;

        public float range {
            get { return mRange; }
            set { mRange = value; }
        }
    }

    public Transform dropPoint;

    public Data[] drops;
    public float noneWeight = 1.0f;

    private float mMaxRange;

    private Data mPicker = new Data();

    private int mPickInd = -1;

    public ItemEntity Drop() {
        ItemEntity ret;

        GeneratePick();

        if(mPickInd != -1) {
            ret = ItemManager.instance.SpawnItem(mPicker.id, dropPoint.position, dropPoint.rotation);
        }
        else {
            ret = null;
        }

        return ret;
    }

    void Awake() {
        if(dropPoint == null)
            dropPoint = transform;

        //prep up randomization

        mMaxRange = noneWeight;

        foreach(Data drop in drops) {
            mMaxRange += drop.weight;
            drop.range = mMaxRange;
        }
    }

    void GeneratePick() {
        mPicker.range = Random.value * mMaxRange;

        if(mPicker.range > noneWeight) {
            mPickInd = System.Array.BinarySearch(drops, mPicker, this);

            if(mPickInd < 0) {
                mPickInd = ~mPickInd;
                if(mPickInd >= drops.Length)
                    mPickInd = drops.Length - 1;
            }

            mPicker.id = drops[mPickInd].id;
        }
        else {
            mPicker.id = -1;
            mPickInd = -1;
        }
    }

    public int Compare(Data obj1, Data obj2) {

        if(obj1 != null && obj2 != null) {
            float v = obj1.range - obj2.range;

            if(Mathf.Abs(v) <= float.Epsilon)
                return 0;
            else if(v < 0.0f)
                return -1;
            else
                return 1;
        }
        else if(obj1 == null && obj2 != null) {
            return 1;
        }
        else if(obj2 == null && obj1 != null) {
            return -1;
        }

        return 0;
    }
}
