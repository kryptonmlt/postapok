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
	public GameObject GamesList;
	public Text usernameText;
	public Text currentPlayersText;
	public Text maxPlayersText;
	public Text allPlayersText;
	public Button refreshButton;
	public Text gameName;
	public GameObject joinGameButton;
	private static string serverUrl = "localhost:9901/";
	private static string gameUrl = serverUrl + "game/";
	bool joinGame = false;
	private string username;
	private bool currentGameMenu = false;
	private string gameId = "";

	private LobbyPopulator ()
	{
	}

	void Start ()
	{
		refreshGamesList ();
	}

	void Update ()
	{
		if (currentGameMenu) {
			
		}
	}

	public void getGamesListRequest ()
	{
		WWW www = new WWW (gameUrl + "/open");
		StartCoroutine (WaitForRequest (www, RequestType.GET_GAMES));
	}

	public void getGameRequest (string id)
	{
		WWW www = new WWW (gameUrl + "/" + id);
		StartCoroutine (WaitForRequest (www, RequestType.GET_GAME));
	}

	public void unJoinGameRequest ()
	{
		WWWForm form = new WWWForm ();
		form.AddField ("secret", secretInputField.text);
		form.AddField ("username", username);
		form.AddField ("join", "false");
		WWW www = new WWW (gameUrl + gameId, form);
		StartCoroutine (WaitForRequest (www, RequestType.REMOVE_PLAYER));
	}

	public void joinGameRequest ()
	{
		WWWForm form = new WWWForm ();
		form.AddField ("secret", secretInputField.text);
		form.AddField ("username", username);
		form.AddField ("join", "true");
		WWW www = new WWW (gameUrl + gameId, form);
		StartCoroutine (WaitForRequest (www, RequestType.JOIN_GAME));
	}

	public void createGame ()
	{
		WWWForm form = new WWWForm ();
		form.AddField ("secret", secretInputField.text);
		form.AddField ("maxPlayers", maxPlayersInputField.GetComponent<InputField> ().text);
		WWW www = new WWW (gameUrl, form);
		StartCoroutine (WaitForRequest (www, RequestType.CREATE_GAME));
	}

	IEnumerator WaitForRequest (WWW www, RequestType type)
	{
		yield return www;

		// check for errors
		if (www.error == null) {
			switch (type) {
			case RequestType.CREATE_GAME:
				gameId = www.text;
				joinGameRequest ();
				break;
			case RequestType.GET_GAME:
				Game game = JsonUtility.FromJson<Game> (www.text);
				maxPlayersText.text = game.maxPlayers;
				gameName.text = "Game " + game.id;
				usernameText.text = "None";
				if (game.players != null) {
					usernameText.text = game.players [0];
					currentPlayersText.text = "" + game.players.Length;
					allPlayersText.text = "";
					foreach (string u in game.players) {
						allPlayersText.text += u + "\n";
					}
				}
				break;
			case RequestType.REMOVE_PLAYER:
				break;
			case RequestType.GET_GAMES:
				destroyChildButtons (GamesList);
				GameCollection games = JsonUtility.FromJson<GameCollection> (www.text);
				foreach (Game g in games.games) {
					Debug.Log ("open: " + g.id);
					GameObject joinButton = Instantiate (joinGameButton);
					joinButton.transform.SetParent (GamesList.transform);
					Text buttonText = joinButton.GetComponentInChildren<Text> ();
					if (g.players != null && g.players.Length != 0) {
						buttonText.text = "Game " + g.id + " - " + g.players [0] + " (" + g.players.Length + "/" + g.maxPlayers + ")";
					} else {
						buttonText.text = "Game " + g.id + " - None (0/" + g.maxPlayers + ")";
					}
					Button button = joinButton.GetComponentInChildren<Button> ();
					button.onClick.AddListener (() => {
						setSecretMenuJoin ("" + g.id);
					});
				}
				break;
			case RequestType.JOIN_GAME:
				getGameRequest (gameId);
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

	public void destroyChildButtons (GameObject list)
	{
		Button[] buttons = list.GetComponentsInChildren<Button> ();
		foreach (Button b in buttons) {
			Destroy (b.gameObject);
		}
	}

	public void refreshGamesList ()
	{
		getGamesListRequest ();
	}

	public void unJoinGameAndGoToLobby ()
	{
		unJoinGameRequest ();	
		setLobbyMenu ();
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

	public void setSecretMenuJoin (string gId)
	{
		gameId = gId;
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

	public void setUsername ()
	{
		username = usernameField.text;
	}

	public void goToGameMenuButton ()
	{
		if (joinGame) {
			joinGameRequest ();
		} else {//creating game
			createGame ();
		}
	}
}
