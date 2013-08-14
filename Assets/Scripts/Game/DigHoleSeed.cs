using UnityEngine;
using System.Collections;

/// <summary>
/// Make sure to add an interactor for this object
/// </summary>
public class DigHoleSeed : DigHole {
    /// <summary>
    /// Called to plant an item, effectively removing this object
    /// </summary>
    public void PlantItem(int itemID) {
        //spawn item

        //remove this dig hole
        if(!DigHoleManager.instance.RemoveDig(this))
            Object.Destroy(gameObject);
    }

    protected override void OnInteract(Interactor interact, GameObject source) {
        //let player select a seed, then player tells us to plant or not
        Debug.Log("Open seed inventory");
    }
}
