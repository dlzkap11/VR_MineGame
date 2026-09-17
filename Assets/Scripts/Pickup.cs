using ShatterStone;
using UnityEngine;

public class Pickup : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("OrePickup")) //Checks Tag of collided object.
        {
            other.GetComponent<PickupController>()?.CollectItem(); //Triggers Ore Hit on node.
        }
    }
}
