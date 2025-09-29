using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class GameManager : MonoBehaviourPun
{
    [Header("Players")]
    public string playerPrefabLocation;
    public PlayerController[] players;
    public Transform[] spawnPoints;
    public int alivePlayers;
    public float postGameTime;

    private int playersInGame;

    // instance 
    public static GameManager instance;

    void Awake()
    {
        instance = this;
    }

    // 1. Each client tells the server to tell everyone else the've started up ingame
    // 2. If I'm the master client and all playrs have run their start functin, call spawn player on everyone
    // In other words, don't spawn any player objects until all players are in-game and ready to go

    void Start()
    {
        // set the size of the players array and alive players interger
        players = new PlayerController[PhotonNetwork.PlayerList.Length];
        alivePlayers = players.Length;

        photonView.RPC("ImInGame", RpcTarget.AllBuffered);
    }

    // ImInGame gets called to all players when someone loads into the Game scene
    // This keeps track of how many players are in the game and if everyone is in, spawn the players

    [PunRPC]
    void ImInGame()
    {
        playersInGame++;
        
        if(PhotonNetwork.IsMasterClient && playersInGame == PhotonNetwork.PlayerList.Length)
        {
            photonView.RPC("SpawnPlayer", RpcTarget.All);
        }
    }

    // SpawnPlayer instantiates a player across the network
    
    [PunRPC]
    void SpawnPlayer()
    {
        GameObject playerOBj = PhotonNetwork.Instantiate(playerPrefabLocation, spawnPoints
            [Random.Range(0, spawnPoints.Length)].position, Quaternion.identity);

        // initialize the player for all other players

        playerOBj.GetComponent<PlayerController>().photonView.RPC("Initialize", RpcTarget.All, PhotonNetwork.LocalPlayer);
    }


    //**********OLD GetPlayer() functions**********

    // these functions help with finding other players in the game
    // public PlayerController GetPlayer(int playerId)
    // {
    //    return players.First(x => x.id == playerId);
    // }

    // public PlayerController GetPlayer(GameObject playerObj)
    // {
    //     return players.First(x => x.gameObject == playerObj);
    // }

    public PlayerController GetPlayer(int playerId)
    {
        foreach(PlayerController player in players)
        {
            if (player != null && player.id == playerId)
            {
                return player;
            }
        }

        return null;
    }

    public PlayerController GetPlayer(GameObject playerObject)
    {
        foreach(PlayerController player in players)
        {
            if(player != null && player.gameObject == playerObject)
            {
                return player;
            }
        }
        
        return null;
    }

    public void CheckWinCondition()
    {
        if(alivePlayers == 1)
        {
            photonView.RPC("WinGame", RpcTarget.All, players.First(x => !x.dead).id);
        }
    }

    [PunRPC]
    void WinGame(int winningPlayer)
    {
        // set UI win text
        GameUI.instance.SetWinText(GetPlayer(winningPlayer).photonPlayer.NickName);

        Invoke("GoBackToMenu", postGameTime);
    }

    void GoBackToMenu()
    {
        NetworkManager.instance.ChangeScene("Menu");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
