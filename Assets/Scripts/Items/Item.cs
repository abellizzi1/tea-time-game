using UnityEngine;

public abstract class Item : MonoBehaviour
{
    protected PlayerController player;     // available to derived classes

    public abstract string toString();

    public abstract string tagString();

    public abstract void onPickup();
}