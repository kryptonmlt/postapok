using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPopulator : MonoBehaviour
{

	public GameObject lobbyPanel;
	public GameObject secretPanel;
	public GameObject gamePanel;
	private static LobbyPopulator instance;

	private LobbyPopulator ()
	{
	}

	void Start ()
	{
		
	}

	void Update ()
	{
		
	}

	public static LobbyPopulator getInstance ()
	{
		if (instance == null) {
			instance = new LobbyPopulator ();
		}
		return instance;
	}

	public void setLobbyMenu ()
	{
		secretPanel.SetActive (false);
		gamePanel.SetActive (false);
		lobbyPanel.SetActive (true);
	}

	public void setSecretMenu ()
	{
		secretPanel.SetActive (true);
		gamePanel.SetActive (false);
		lobbyPanel.SetActive (false);
	}

	public void setGameMenu ()
	{
		secretPanel.SetActive (false);
		gamePanel.SetActive (true);
		lobbyPanel.SetActive (false);
	}
}
