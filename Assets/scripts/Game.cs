using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Game
{
	
	public long id;

	public string secret;

	public string maxPlayers;

	public string dateStarted;

	public string[] players;

	public Game ()
	{
	}
}