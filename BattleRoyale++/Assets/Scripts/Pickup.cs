using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

// create an enumerator for our pickup types
public enum PickupType
{
    Health,
    Ammo
}
public class Pickup : MonoBehaviourPun
{
    public PickupType type;
    public int value;

    // when a player picks up a pickup one of two functions will be called
    // Heal() and GiveAmmo()

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Pickup triggered");
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            // get the player
            PlayerController player = GameManager.instance.GetPlayer(other.gameObject);

            if (type == PickupType.Health)
            {
                player.photonView.RPC("Heal", player.photonPlayer, value);
            }

            else if (type == PickupType.Ammo)
            {
                player.photonView.RPC("GiveAmmo", player.photonPlayer, value);
            }

            photonView.RPC("DestroyPickup", RpcTarget.AllBuffered);
        }

    }

    [PunRPC]
    public void DestroyPickup()
    {
        Destroy(gameObject);
    }
}
