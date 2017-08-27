using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPopulator : MonoBehaviour
{

	public GameObject lobbyPanel;
	public GameObject secretPanel;
	public GameObject gamePanel;
	public InputField secretInputField;
	public InputField usernameField;
	public GameObject maxPlayersInputField;
	public GameObject maxPlayersLabel;
	public Text usernameText;
	public Text currentPlayersText;
	public Text maxPlayersText;
	public Text allPlayersText;
	public Button refreshButton;
	private static string serverUrl = "localhost:9091/";
	private static string gameUrl = serverUrl + "game/";
	bool joinGame = false;
	private string username;
	private bool currentGameMenu = false;
	private string gameId="";

	private LobbyPopulator ()
	{
	}

	void Start ()
	{
		
	}

	void Update ()
	{
		if(currentGameMenu){
		}
	}

	public void getGamesList ()
	{
		WWW www = new WWW (gameUrl + "/open");
	}

	public void joinGameRequest ()
	{
		WWWForm form = new WWWForm ();
		form.AddField ("secret", secretInputField.text);
		form.AddField ("username", username);
		WWW www = new WWW (gameUrl+gameId, form);
		StartCoroutine (WaitForRequest (www, RequestType.JOIN_GAME));
	}

	public void createGame ()
	{
		WWWForm form = new WWWForm ();
		form.AddField ("secret", secretInputField.text);
		form.AddField ("maxPlayers", maxPlayersInputField.GetComponent<InputField>().text);
		WWW www = new WWW (gameUrl, form);
		StartCoroutine (WaitForRequest (www, RequestType.CREATE_GAME));
	}

	IEnumerator WaitForRequest (WWW www, RequestType type)
	{
		yield return www;

		// check for errors
		if (www.error == null) {
			switch(type){
			case RequestType.CREATE_GAME:
				gameId = www.text;
				joinGameRequest ();
				break;
			case RequestType.GET_GAME:
				Game game = JsonUtility.FromJson<Game> (www.text);
				usernameText.text = username;
				maxPlayersText.text = game.maxPlayers;
				currentPlayersText.text = ""+game.players.Length;
				foreach(string u in game.players){
					allPlayersText.text += u + "\n";
				}
				break;
			case RequestType.GET_GAMES:
				break;
			case RequestType.JOIN_GAME:
				setGameMenu ();
				break;
			default:
				break;
			}
			Debug.Log ("WWW Ok!: " + www.text);
		} else {
			Debug.Log ("WWW Error: " + www.error);
		}    
	}

	public void refreshGamesList ()
	{
	}

	public void setLobbyMenu ()
	{
		currentGameMenu = false;
		secretPanel.SetActive (false);
		gamePanel.SetActive (false);
		lobbyPanel.SetActive (true);
	}

	public void setSecretMenuCreate ()
	{
		currentGameMenu = false;
		joinGame = false;
		setUsername ();
		secretPanel.SetActive (true);
		gamePanel.SetActive (false);
		lobbyPanel.SetActive (false);
	}

	public void setSecretMenuJoin ()
	{
		currentGameMenu = false;
		joinGame = true;
		setUsername ();
		secretPanel.SetActive (true);
		gamePanel.SetActive (false);
		lobbyPanel.SetActive (false);
		maxPlayersInputField.SetActive (false);
		maxPlayersLabel.SetActive (false);
	}

	public void setGameMenu ()
	{
		currentGameMenu = true;
		secretPanel.SetActive (false);
		gamePanel.SetActive (true);
		lobbyPanel.SetActive (false);
	}

	public void setUsername(){
		username = usernameField.text;
	}

	public void goToGameMenuButton(){
		if(joinGame){
			joinGameRequest ();
		}else{//creating game
			createGame();
		}
	}
}
