using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game
{
	
	public long id { get; set; }

	public string secret{ get; set; }

	public string maxPlayers{ get; set; }

	public string dateStarted{ get; set; }

	public string[] players{ get; set; }

	public Game ()
	{
	}
}