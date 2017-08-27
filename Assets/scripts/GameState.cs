using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
	private static string serverUrl = "localhost:9091/";
	private static string gameUrl = serverUrl + "game/";
	private static GameState instance;

	public static GameState getInstance ()
	{
		if (instance == null) {
			instance = new GameState ();
		}
		return instance;
	}

	private GameState ()
	{
	}

	public void createGame (string secret, string m)
	{
		WWWForm form = new WWWForm ();
		form.AddField ("secret", secret);
		form.AddField ("maxPlayers", m);
		WWW www = new WWW (gameUrl, form);

		StartCoroutine (WaitForRequest (www));
	}

	public IEnumerator getGame (long gameId)
	{
		WWW www = new WWW (gameUrl + gameId);
		yield return www;
		if (www.error == null) {
			Game game = JsonUtility.FromJson<Game> (www.text);
		} else {
			Debug.Log ("ERROR: " + www.error);
		} 
	}

	IEnumerator WaitForRequest (WWW www)
	{
		yield return www;

		// check for errors
		if (www.error == null) {
			Debug.Log ("WWW Ok!: " + www.text);
		} else {
			Debug.Log ("WWW Error: " + www.error);
		}    
	}
}
