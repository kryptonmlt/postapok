using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Random = UnityEngine.Random;
using System.Text;
using System.IO;
using UnityEngine.UI;

public class GridManager: MonoBehaviour
{
	public int ID = 0;
	public Button endTurnButton;

	public int getId ()
	{
		return ID++;
	}

	public bool hideObjects = true;

	public Dictionary<int, GameObject> gameobjects = new Dictionary<int, GameObject> ();
	public Dictionary<int, List<GameObject>> ObjsPaths = new Dictionary<int, List<GameObject>> ();
	public Dictionary<int, Path<Tile>> ObjsPathsTiles = new Dictionary<int, Path<Tile>> ();

	//selectedTile stores the tile mouse cursor is hovering on
	public Tile selectedTile = null;
	//TB of the tile which is the start of the path
	public Dictionary<int, TileBehaviour> originTileTB = new Dictionary<int, TileBehaviour> ();
	public Dictionary<int, TileBehaviour> destTileTB = new Dictionary<int, TileBehaviour> ();

	public Dictionary<int, TileBehaviour> getOriginTileTB ()
	{
		return originTileTB;
	}
	//TB of the tile which is the end of the path
	public TileBehaviour tb = null;

	public static GridManager instance = null;

	//Line should be initialised to some 3d object that can fit nicely in the center of a hex tile and will be used to indicate the path. For example, it can be just a simple small sphere with some material attached to it. Initialise the variable using inspector pane.
	public GameObject Line;

	private float hexWidth;
	private float hexHeight;
	private float groundWidth;
	private float groundHeight;
	private float groundOffset;

	private Shader selfIllumShader;
	private Shader standardShader;

	static Vector3 downmouseposition;
	static bool draw = false;
	private bool clicked = false;
	private bool showSplitMenu = false;
	private GameObject splitSelection = null;
	private Vector3 splitMenuPos;

	private Texture2D rectangleTexture;
	public float fAlpha = 0.25f;

	private GameObject camp;
	private GameObject refinery;
	private GameObject windmill;
	private GameObject junkyard;
	private GameObject mountain;
	private GameObject junk;
	private GameObject tree;
	private GameObject Hex;
	private GameObject Ground;
	private GameObject fanatic;
	private GameObject truck;
	private GameObject car;
	private GameObject bike;
	private List<GameObject> stones = new List<GameObject> ();
	private List<GameObject> selectionMenu = new List<GameObject> ();
	//0=fanatic,1=bike,2=car,3=truck,4=refinery,5=windmill,6=junkyard
	private List<Texture> textures = new List<Texture> ();

	public static LinkedList<GameObject> unitSelected = new LinkedList<GameObject> ();

	private int turn = 0;
	private int players = 0;
	private int round = 0;
	private PlayerData[] playerData;
	private Text waterResource;
	private Text petrolResource;
	private Text scrapResource;
	private Text turnResource;
	private Text playerResource;

	public Dictionary<Point, TileBehaviour> board = new Dictionary<Point, TileBehaviour> ();

	private int[] initialResources = { 10, 10, 12 };
	private int[] fanaticCost = { 1, 0, 1 };
	private int[] bikeCost = { 1, 1, 2 };
	private int[] carCost = { 2, 2, 3 };
	private int[] truckCost = { 3, 3, 4 };
	private int[] waterMillCost = { 0, 0, 4 };
	private int[] junkYardCost = { 0, 0, 4 };
	private int[] refineryCost = { 0, 0, 4 };
	private int[] structureUpgradeCost = { 0, 0, 4 };
	private int[] unitUpgradeCost = { 0, 0, 10 };

	private int resourceLimitGain = 5;
	private int upgradeBenefit = 1;
	private int viewRange = 2;
	public int globalInterval = 0;
	private List<CharacterMovement> chMovements = new List<CharacterMovement> ();

	Dictionary<int, LandType> TerrainType = new Dictionary<int, LandType> () {
		{ 0,LandType.Base },
		{ 1,LandType.Oasis },
		{ 2,LandType.Junkyard },
		{ 3,LandType.OilField },
		{ 4,LandType.Desert },
		{ 5,LandType.Mountain }
	};

	public void deSelect ()
	{
		//remove highlight
		foreach (GameObject unit in unitSelected) {
			Renderer[] renderers = unit.GetComponentsInChildren<Renderer> ();
			foreach (Renderer renderer in renderers) {
				renderer.material.shader = standardShader;
			}
		}
		GridManager.unitSelected = new LinkedList<GameObject> ();
		clearSelectionMenu ();
		foreach (TileBehaviour tb in board.Values) {
			tb.decolour ();
		}
	}


	void setSizes ()
	{
		hexWidth = Hex.GetComponent<Renderer> ().bounds.size.x;
		hexHeight = Hex.GetComponent<Renderer> ().bounds.size.z;
		groundWidth = Ground.GetComponent<Renderer> ().bounds.size.x;
		groundOffset = Ground.GetComponent<Renderer> ().transform.position.y;
		groundHeight = Ground.GetComponent<Renderer> ().bounds.size.z;
	}

	void OnGUI ()
	{
		if (GridManager.draw == true) {
			Color colPreviousGUIColor = GUI.color;
			GUI.color = new Color (colPreviousGUIColor.r, colPreviousGUIColor.g, colPreviousGUIColor.b, fAlpha);
			GUI.DrawTexture (new Rect (downmouseposition.x, Screen.height - downmouseposition.y, Input.mousePosition.x - downmouseposition.x, downmouseposition.y - Input.mousePosition.y), rectangleTexture);
			GUI.color = colPreviousGUIColor;
		}
		Vector2 targetPos;
		foreach (int key in gameobjects.Keys) {
			GameObject go = gameobjects [key];
			if (go != null) {
				targetPos = Camera.main.WorldToScreenPoint (go.transform.position);
				GOProperties gop = (GOProperties)go.GetComponent (typeof(GOProperties));
				if (gop.shown) {
					GUI.Box (new Rect (targetPos.x, Screen.height - targetPos.y, 20, 20), gop.Quantity.ToString ());
				}
			}
		}
		if (showSplitMenu) {			
			splitMenu (Camera.main.WorldToScreenPoint (splitMenuPos), splitSelection);
		}
	}

	void splitMenu (Vector3 targetPos, GameObject selection)
	{
		GOProperties gop = (GOProperties)selection.GetComponent (typeof(GOProperties));
		if (GUI.Button (new Rect (targetPos.x, Screen.height - targetPos.y, 20, 20), "+")) {
			if (gop.tempQuantity < gop.Quantity) {
				gop.tempQuantity++;
			}
		}
		if (GUI.Button (new Rect (targetPos.x + 40, Screen.height - targetPos.y, 20, 20), "-")) {
			if (gop.tempQuantity > 1) {
				gop.tempQuantity--;
			}
		}
		GUI.Box (new Rect (targetPos.x + 20, Screen.height - targetPos.y, 20, 20), gop.tempQuantity.ToString ());
		if (GUI.Button (new Rect (targetPos.x + 60, Screen.height - targetPos.y, 20, 20), "✓")) {
			showSplitMenu = false;
			if (gop.Quantity != gop.tempQuantity) {
				
				GameObject splitObject = createObject (originTileTB [gop.UniqueID], getPrefab (selection), gop.PlayerId, false);
				GOProperties gopSplit = (GOProperties)splitObject.GetComponent (typeof(GOProperties));
				gopSplit.Quantity = gop.Quantity - gop.tempQuantity;
				gameobjects.Add (gopSplit.UniqueID, splitObject);
				gop.Quantity = gop.tempQuantity;
				show (splitObject, true);
			}
		}
	}

	GameObject getPrefab (GameObject obj)
	{
		GOProperties gop = (GOProperties)obj.GetComponent (typeof(GOProperties));
		switch (gop.type) {
		case "ThirdPersonController":
			return fanatic;
		case "Apo_Car_2015":
			return car;
		case "f_noladder":
			return truck;
		case "bike":
			return bike;
		}
		return null;
	}

	void updateResourcesMenu (int playerId)
	{
		PlayerData data = playerData [playerId];
		turnResource.text = "" + round;
		playerResource.text = "" + (playerId + 1);
		waterResource.text = "" + data.water;
		petrolResource.text = "" + data.petrol;
		scrapResource.text = "" + data.scrap;
	}

	void Update ()
	{
		updateResourcesMenu (getCurrentPlayerId ());

		bool moving = isAnyMoving ();
		updateGlobalInterval (moving);

		if (turn == players && !moving) {
			chMovements.Clear ();
			resolution ();
			turn = 0;
			round++;
			hideEnemyObjects (getCurrentPlayerId ());
		}
			

		if (Input.GetKeyDown (KeyCode.Escape)) {
			deSelect ();
			GameObject[] allUnits = GameObject.FindGameObjectsWithTag ("Unit");
			foreach (GameObject unit in allUnits) {
				Renderer[] renderers = unit.GetComponentsInChildren<Renderer> ();
				foreach (Renderer renderer in renderers) {
					renderer.material.shader = standardShader;
				}
			}
		}

		if (Input.GetMouseButtonDown (0) & !unitSelected.Any () & moving == false) {
			GridManager.downmouseposition = Input.mousePosition;
			GridManager.draw = true;
			clicked = true;
			
		} else if (Input.GetMouseButtonUp (0) & !unitSelected.Any () & moving == false & clicked == true) {

			// Single hit 
			RaycastHit hitInfo = new RaycastHit ();
			GameObject selected = null;
			if (Physics.Raycast (Camera.main.ScreenPointToRay (Input.mousePosition), out hitInfo)) {
				selected = hitInfo.transform.gameObject;
				GOProperties gop = (GOProperties)selected.GetComponent (typeof(GOProperties));
				if (hitInfo.transform.gameObject.tag == "Unit" & !unitSelected.Contains (hitInfo.transform.gameObject) && gop.PlayerId == turn) {
					unitSelected.AddLast (hitInfo.transform.gameObject);
					Renderer[] renderers = hitInfo.transform.gameObject.GetComponentsInChildren<Renderer> ();
					foreach (Renderer renderer in renderers) {
						renderer.material.shader = selfIllumShader;
					}
				}
			}
			// Rectangular hit
			GridManager.draw = false;
			RaycastHit hit1;
			Physics.Raycast (Camera.main.ScreenPointToRay (downmouseposition), out hit1);
			Vector3 v1 = hit1.point;
			RaycastHit hit2;
			Physics.Raycast (Camera.main.ScreenPointToRay (Input.mousePosition), out hit2);
			Vector3 v2 = hit2.point;
			GameObject[] allUnits = GameObject.FindGameObjectsWithTag ("Unit");
			foreach (GameObject unit in allUnits) {
				Vector3 pos = unit.transform.position;
				//is inside the box
				if (Mathf.Max (v1.x, v2.x) >= pos.x && Mathf.Min (v1.x, v2.x) <= pos.x
				    && Mathf.Max (v1.z, v2.z) >= pos.z && Mathf.Min (v1.z, v2.z) <= pos.z) {
					GOProperties gop = (GOProperties)unit.GetComponent (typeof(GOProperties));
					if (selected != null && unit != selected && gop.PlayerId == turn) {
						unitSelected.AddLast (unit);
						Renderer[] renderers = unit.GetComponentsInChildren<Renderer> ();
						foreach (Renderer renderer in renderers) {
							renderer.material.shader = selfIllumShader;
						}
					}
				}
			}
			if (unitSelected.Count == 1) {
				TileBehaviour landHit = retrieveTileBehaviourOfObject (unitSelected.First ());
				updateSelectionMenu (landHit);
			}
			clicked = false;
		}
		highlightAccessibleTiles ();
	}

	public int getCurrentPlayerId ()
	{
		return turn >= players ? turn - 1 : turn;
	}


	public void hideEnemyObjects (int playerId)
	{
		//set all enemies to false
		for (int i = 0; i < players; i++) {
			string linesName = "Lines" + i;
			GameObject lines = GameObject.Find (linesName);
			if (lines != null) {
				Renderer[] renderers = lines.GetComponentsInChildren<Renderer> ();
				foreach (Renderer renderer in renderers) {
					if (hideObjects) {
						renderer.enabled = playerId == i;
					} else {
						renderer.enabled = true;
					}
				}
			}
		}

		foreach (int key in gameobjects.Keys) {
			GameObject go = gameobjects [key];
			GOProperties gop = (GOProperties)go.GetComponent (typeof(GOProperties));
			if (hideObjects) {
				show (go, gop.PlayerId == playerId);
			} else {
				show (go, true);
			}
		}
		//check if there is anyone in range
		foreach (int key in gameobjects.Keys) {
			GameObject go = gameobjects [key];
			GOProperties gop = (GOProperties)go.GetComponent (typeof(GOProperties));
			if (gop.PlayerId == playerId) {
				foreach (TileBehaviour tb in board.Values) {					
					//find shortest path between enemy tile and friendly unit
					TileBehaviour test = getTileOfUnit (gop.UniqueID);
					var path = PathFinder.FindPath (test.tile, tb.tile);
					//show objs on enemy tile if in range
					if (path != null && path.TotalCost <= viewRange) {
						foreach (GameObject objOnTile in tb.objsOnTile) {
							GOProperties gopOnTile = (GOProperties)objOnTile.GetComponent (typeof(GOProperties));
							if (gopOnTile.structureShown == null) {
								show (objOnTile, true);
							} else if (!gopOnTile.structureShown [playerId]) {
								gopOnTile.structureShown [playerId] = true;	
								if (hideObjects) {
									show (objOnTile, gopOnTile.structureShown [playerId]);	
								} else {
									show (objOnTile, true);
								}
							}
						}
					} else {
						//if out of view range check structures
						foreach (GameObject objOnTile in tb.objsOnTile) {
							GOProperties gopOnTile = (GOProperties)objOnTile.GetComponent (typeof(GOProperties));
							if (gopOnTile.structureShown != null) {
								if (hideObjects) {
									show (objOnTile, gopOnTile.structureShown [playerId]);
								} else {
									show (objOnTile, true);
								}
							}
						}
					}
				}
			}
		}
	}

	public TileBehaviour getTileOfUnit (int uniqueId)
	{
		foreach (TileBehaviour tb in board.Values) {
			foreach (GameObject objOnTile in tb.objsOnTile) {
				GOProperties gop = (GOProperties)objOnTile.GetComponent (typeof(GOProperties));
				if (gop.UniqueID == uniqueId) {
					return tb;
				}
			}
		}
		Debug.Log (uniqueId + " NOT FOUND");
		// id not found
		return null;
	}

	public void show (GameObject obj, bool show)
	{
		GOProperties gop = (GOProperties)obj.GetComponent (typeof(GOProperties));
		gop.shown = show;
		Renderer[] renderers = obj.GetComponentsInChildren<Renderer> ();
		foreach (Renderer renderer in renderers) {
			renderer.enabled = show;
		}
	}

	public int getLeastMovementOfSelectedUnits (List<GameObject> units)
	{
		int movement = int.MaxValue;
		foreach (GameObject unit in units) {
			GOProperties gop = (GOProperties)unit.GetComponent (typeof(GOProperties));
			if (gop.MovementValue < movement) {
				movement = gop.MovementValue;
			}
		}
		return movement;
	}

	public void highlightAccessibleTiles ()
	{
		foreach (TileBehaviour tb in board.Values) {
			if (unitSelected.Count > 0) {
				if (canUnitsGoToTile (unitSelected, tb)) {
					tb.highlightMovementPossible ();
				}
			}
		}
	}

	public bool canUnitsGoToTile (LinkedList<GameObject> units, TileBehaviour tb)
	{
		bool possible = true;
		foreach (GameObject unit in units) {
			if (!canUnitGoToTile (unit, tb)) {
				return false;
			}
		}
		return possible;
	}

	public bool canUnitGoToTile (GameObject unit, TileBehaviour tb)
	{	
		GOProperties gop = (GOProperties)unit.GetComponent (typeof(GOProperties));
		var path = PathFinder.FindPath (originTileTB [gop.UniqueID].tile, tb.tile);
		if (path == null) {
			return false;
		}
		return path.TotalCost <= gop.MovementValue;
	}

	//The method used to calculate the number hexagons in a row and number of rows
	//Vector2.x is gridWidthInHexes and Vector2.y is gridHeightInHexes
	Vector2 calcGridSize ()
	{
		//According to the math textbook hexagon's side length is half of the height
		float sideLength = hexHeight / 2;
		//the number of whole hex sides that fit inside inside ground height
		int nrOfSides = (int)(groundHeight / sideLength);
		//I will not try to explain the following calculation because I made some assumptions, which might not be correct in all cases, to come up with the formula. So you'll have to trust me or figure it out yourselves.
		int gridHeightInHexes = (int)(nrOfSides * 2 / 3);
		//When the number of hexes is even the tip of the last hex in the offset column might stick up.
		//The number of hexes in that case is reduced.
		if (gridHeightInHexes % 2 == 0
		    && (nrOfSides + 0.5f) * sideLength > groundHeight)
			gridHeightInHexes--;
		//gridWidth in hexes is calculated by simply dividing ground width by hex width
		return new Vector2 ((int)(groundWidth / hexWidth), gridHeightInHexes);
	}
	//Method to calculate the position of the first hexagon tile
	//The center of the hex grid is (0,0,0)
	Vector3 calcInitPos ()
	{
		Vector3 initPos;
		initPos = new Vector3 (-groundWidth / 2 + hexWidth / 2, 0,
			groundHeight / 2 - hexWidth / 2);

		return initPos;
	}

	public Vector3 calcWorldCoord (Vector2 gridPos)
	{
		Vector3 initPos = calcInitPos ();
		float offset = 0;
		if (gridPos.y % 2 != 0)
			offset = hexWidth / 2;

		float x = initPos.x + offset + gridPos.x * hexWidth;
		float z = initPos.z - gridPos.y * hexHeight * 0.75f;
		//If your ground is not a plane but a cube you might set the y coordinate to sth like groundDepth/2 + hexDepth/2
		return new Vector3 (x, groundOffset, z);
	}

	public bool isAnyMoving ()
	{
		bool moving = false;
		foreach (GameObject active in GameObject.FindGameObjectsWithTag("Unit")) {
			if (active != null) {
				CharacterMovement characterAction = (CharacterMovement)active.GetComponent (typeof(CharacterMovement));
				if (characterAction.IsMoving == true) {
					moving = characterAction.IsMoving;
				}
			}
		}
		return moving;
	}

	void updateGlobalInterval (bool moving)
	{
		if (moving) {
			bool everyoneWaiting = true;
			foreach (CharacterMovement ch in chMovements) {
				if (ch.IsMoving) {
					if (ch.unitInterval < globalInterval) {
						everyoneWaiting = false;
					}
				}
			}
			if (everyoneWaiting) {
				globalInterval++;
			}
		}
	}

	void endTurnTask ()
	{
		bool moving = isAnyMoving ();
		if (!moving) {
			//remove split menu
			showSplitMenu = false;
			deSelect ();
			if (turn == players - 1) {
				globalInterval = 1;
				GridManager.draw = false;
				List<CharacterMovement> chMovementsTemp = new List<CharacterMovement> ();
				foreach (GameObject unit in GameObject.FindGameObjectsWithTag("Unit")) {
					GOProperties gop = (GOProperties)unit.GetComponent (typeof(GOProperties));
					if (unit != null & ObjsPathsTiles.ContainsKey (gop.UniqueID)) {
						CharacterMovement characterAction = (CharacterMovement)unit.GetComponent (typeof(CharacterMovement));
						characterAction.StartMoving (ObjsPathsTiles [gop.UniqueID].ToList ());
						originTileTB [gop.UniqueID].removeObjectFromTile (gop.UniqueID);
						//destTileTB [gop.UniqueID].getNextPosition (unit);
						chMovementsTemp.Add (characterAction);
					}
				}
				chMovements = chMovementsTemp;
			}
			turn++;
			hideEnemyObjects (getCurrentPlayerId ());
		}
	}

	void resolution ()
	{
		List<int> objectsForDeletion = new List<int> ();
		List<int> keys = new List<int> (gameobjects.Keys);
		keys.Sort ();
		//join quantities
		foreach (int key1 in keys) {
			GOProperties gop1 = (GOProperties)gameobjects [key1].GetComponent (typeof(GOProperties));
			foreach (int key2 in keys) {
				if (key1 < key2) {
					GOProperties gop2 = (GOProperties)gameobjects [key2].GetComponent (typeof(GOProperties));
					if (originTileTB [gop1.UniqueID].tile == originTileTB [gop2.UniqueID].tile && gop1.PlayerId == gop2.PlayerId && gop2.type == gop1.type) {
						int idToKeep = originTileTB [gop1.UniqueID].objectTypeExists (gop1.type);
						int idToDelete = gop1.UniqueID;
						if (idToKeep == gop1.UniqueID) {
							idToDelete = gop2.UniqueID;
						} else if (idToKeep != gop2.UniqueID) {
							Debug.Log ("THIS SHOULD NOT HAPPEN - one of the ids should have been correct");
						}
						if (!objectsForDeletion.Contains (idToDelete)) {
							GOProperties gopKeep = (GOProperties)gameobjects [idToKeep].GetComponent (typeof(GOProperties));
							GOProperties gopDelete = (GOProperties)gameobjects [idToDelete].GetComponent (typeof(GOProperties));
							gopKeep.Quantity += gopDelete.Quantity;
							objectsForDeletion.Add (idToDelete);
						} else {
							Debug.Log ("THIS SHOULD NOT HAPPEN - we shouldnt try to delete twice");
						}
					}
				}
			}
		}

		//delete duplicate objects that were joined
		foreach (int guid in objectsForDeletion) {
			Destroy (gameobjects [guid]);
			gameobjects.Remove (guid);
		}

		keys = new List<int> (gameobjects.Keys);
		keys.Sort ();
		List<GameObject> attackers = new List<GameObject> ();
		List<GameObject> defenders = new List<GameObject> ();
		//find who is attacking/defending
		foreach (int key1 in keys) {
			GOProperties gop1 = (GOProperties)gameobjects [key1].GetComponent (typeof(GOProperties));
			foreach (int key2 in keys) {
				if (key1 <= key2) {
					GOProperties gop2 = (GOProperties)gameobjects [key2].GetComponent (typeof(GOProperties));

					if (originTileTB [gop1.UniqueID].tile == originTileTB [gop2.UniqueID].tile && gop1.PlayerId != gop2.PlayerId) {
						if (!attackers.Contains (gameobjects [key1]) & !defenders.Contains (gameobjects [key1])) {
							attackers.Add (gameobjects [key1]);
						}
						if (!attackers.Contains (gameobjects [key2]) && !defenders.Contains (gameobjects [key2])) {
							defenders.Add (gameobjects [key2]);
						}
					}
				}
			}
		}

		int attackersvalue = 0;
		int defendersvalue = 0;
		foreach (GameObject attacker in attackers) {
			GOProperties gop = (GOProperties)attacker.GetComponent (typeof(GOProperties));
			attackersvalue += gop.AttackValue * gop.Quantity;
		}
		foreach (GameObject defender in defenders) {
			GOProperties gop = (GOProperties)defender.GetComponent (typeof(GOProperties));
			defendersvalue += gop.AttackValue * gop.Quantity;
		}

		if (attackersvalue >= defendersvalue) {
			foreach (GameObject defender in defenders) {
				GOProperties gop = (GOProperties)defender.GetComponent (typeof(GOProperties));
				Destroy (defender);
				gameobjects.Remove (gop.UniqueID);
			}
		} else {
			foreach (GameObject attacker in attackers) {
				GOProperties gop = (GOProperties)attacker.GetComponent (typeof(GOProperties));
				Debug.Log (attackersvalue);
				Debug.Log (defendersvalue);
				Destroy (attacker);
				gameobjects.Remove (gop.UniqueID);
			}
		}
		calculateResourcesForEveryone ();
	}

	void calculateResourcesForEveryone ()
	{
		int[,] resourcesGained = new int[playerData.Length, 3];

		//calculate resources
		foreach (TileBehaviour tb in board.Values) {
			if (tb.built) {
				int f = tb.getFanaticsOnTile ();
				if (f > resourceLimitGain) {
					f = resourceLimitGain;
				}
				if (tb.upgraded) {
					f += upgradeBenefit;
				}
				int playerOwner = tb.getPlayerOwner ();
				switch (tb.getTile ().landType) {
				case LandType.Oasis:
					resourcesGained [playerOwner, 0] += f;
					break;
				case LandType.OilField:
					resourcesGained [playerOwner, 1] += f;
					break;
				case LandType.Junkyard:
					resourcesGained [playerOwner, 2] += f;
					break;
				default:
					break;
				}
			}
		}
		//update resources
		for (int i = 0; i < playerData.Length - 1; i++) {
			PlayerData p = playerData [i];
			p.water += resourcesGained [i, 0];
			p.petrol += resourcesGained [i, 1];
			p.scrap += resourcesGained [i, 2];
		}
	}

	void Start ()
	{
		LoadResources ();
		for (int i = 0; i < 4; i++) {
			selectionMenu.Add (GameObject.Find ("sel" + i));
		}
		Button btn = endTurnButton.GetComponent<Button> ();
		btn.onClick.AddListener (endTurnTask);
		rectangleTexture = new Texture2D (1, 1);
		rectangleTexture.SetPixel (0, 0, Color.black);
		rectangleTexture.Apply ();
		selfIllumShader = Shader.Find ("Outlined/Silhouetted Bumped Diffuse");
		standardShader = Shader.Find ("Standard");
		setSizes ();
		createGrid ();
		generateAndShowPath ();

		playerData = new PlayerData[players + 1];
		for (int i = 0; i < players; i++) {
			playerData [i] = new PlayerData (initialResources [0], initialResources [1], initialResources [2]);
		}
		hideEnemyObjects (getCurrentPlayerId ());
	}

	void Awake ()
	{
		instance = this;
	}

	void LoadResources ()
	{
		camp = Resources.Load ("Models/structures/barracks/prefabs/barracks", typeof(GameObject)) as GameObject;
		refinery = Resources.Load ("Models/structures/petrolfactory/Prefabs/refinery", typeof(GameObject)) as GameObject;
		windmill = Resources.Load ("Models/structures/windmill/prefabs/windmIll", typeof(GameObject)) as GameObject;
		junkyard = Resources.Load ("Models/structures/junkYard/prefabs/crane", typeof(GameObject)) as GameObject;
		mountain = Resources.Load ("Models/extras/mountain/prefab/mountain", typeof(GameObject)) as GameObject;
		junk = Resources.Load ("Models/extras/junkLand/Prefabs/junkmount", typeof(GameObject)) as GameObject;
		tree = Resources.Load ("Models/extras/trees/Prefabs/tree", typeof(GameObject)) as GameObject;
		Hex = Resources.Load ("Models/Grid/HexGrid/prefab/HexGrid", typeof(GameObject)) as GameObject;
		Ground = Resources.Load ("Models/Grid/GroundSize/prefab/Ground", typeof(GameObject)) as GameObject;
		fanatic = Resources.Load ("Models/ThirdCharacterController/Prefabs/ThirdPersonController", typeof(GameObject)) as GameObject;
		truck = Resources.Load ("Models/Vehicles/F04/Prefabs/f_noladder", typeof(GameObject)) as GameObject;
		car = Resources.Load ("Models/Vehicles/car/Prefabs/Apo_car_2015", typeof(GameObject)) as GameObject;
		bike = Resources.Load ("Models/Vehicles/bike/prefabs/bike", typeof(GameObject)) as GameObject;
		stones.Add (Resources.Load ("Models/extras/stone/Prefabs/rockUp0", typeof(GameObject)) as GameObject);
		stones.Add (Resources.Load ("Models/extras/stone/Prefabs/rockUp1", typeof(GameObject)) as GameObject);
		stones.Add (Resources.Load ("Models/extras/stone/Prefabs/rockUp2", typeof(GameObject)) as GameObject);
		stones.Add (Resources.Load ("Models/extras/stone/Prefabs/rockUp3", typeof(GameObject)) as GameObject);
		textures.Add (Resources.Load ("Textures/fanatic") as Texture);
		textures.Add (Resources.Load ("Textures/bike") as Texture);
		textures.Add (Resources.Load ("Textures/car") as Texture);
		textures.Add (Resources.Load ("Textures/truck") as Texture);
		textures.Add (Resources.Load ("Textures/refinery") as Texture);
		textures.Add (Resources.Load ("Textures/windmill") as Texture);
		textures.Add (Resources.Load ("Textures/junkyard") as Texture);
		textures.Add (Resources.Load ("Textures/UPGRADE") as Texture);

		waterResource = GameObject.Find ("ActionMenu/PlayerResources/WaterText").GetComponent<Text> ();
		petrolResource = GameObject.Find ("ActionMenu/PlayerResources/PetrolText").GetComponent<Text> ();
		scrapResource = GameObject.Find ("ActionMenu/PlayerResources/ScrapText").GetComponent<Text> ();
		turnResource = GameObject.Find ("ActionMenu/PlayerResources/TurnText").GetComponent<Text> ();
		playerResource = GameObject.Find ("ActionMenu/PlayerResources/PlayerText").GetComponent<Text> ();
	}

	void createGrid ()
	{
		List<int> loadedMap = LoadGameFile ();
		Vector2 gridSize = calcGridSize ();
		GameObject hexGridGO = new GameObject ("HexGrid");
		//board is used to store tile locations
		Dictionary<Point, Tile> tempBoard = new Dictionary<Point, Tile> ();
		int landPos = 0;
		for (float y = 0; y < gridSize.y; y++) {
			float sizeX = gridSize.x;
			//if the offset row sticks up, reduce the number of hexes in a row
			if (y % 2 != 0 && (gridSize.x + 0.5) * hexWidth > groundWidth)
				sizeX--;
			for (float x = 0; x < sizeX; x++,landPos++) {
				GameObject hex = (GameObject)Instantiate (Hex);
				Vector2 gridPos = new Vector2 (x, y);
				var tb = (TileBehaviour)hex.GetComponent ("TileBehaviour");
				tb.updatePositionOfTile (calcWorldCoord (gridPos));
				hex.transform.parent = hexGridGO.transform;

				int landTypeId = loadedMap [landPos];
				tb.tile = new Tile ((int)x - (int)(y / 2), (int)y, TerrainType [landTypeId]);
				tb.setTileMaterial (tb.tile.landType);
				tb.tile.boardCoords = new Point ((int)gridPos.x, (int)gridPos.y);
				tempBoard.Add (tb.tile.Location, tb.tile);
				board.Add (new Point ((int)gridPos.x, (int)gridPos.y), tb);

				List<GameObject> stuffOnTile = new List<GameObject> ();
				bool rand = true;
				switch (landTypeId) {
				case 0:
					stuffOnTile.Add (createObject (tb, camp, players));
					players++;
					rand = false;
					break;
				case 1:
					int n = UnityEngine.Random.Range (3, 6);
					for (int i = 0; i < n; i++) {
						stuffOnTile.Add ((GameObject)Instantiate (tree));
					}
					break;
				case 2:
					stuffOnTile.Add ((GameObject)Instantiate (junk));
					rand = false;					
					break;
				case 3:
					int nos = UnityEngine.Random.Range (0, 3);
					for (int i = 0; i < nos; i++) {
						int stone = UnityEngine.Random.Range (0, stones.Count);
						stuffOnTile.Add ((GameObject)Instantiate (stones [stone]));
					}
					break;
				case 4:
					int placeStuff = UnityEngine.Random.Range (0, 10);
					if (placeStuff < 2) {
						int numberOfTrees = UnityEngine.Random.Range (1, 4);
						for (int i = 0; i < numberOfTrees; i++) {
							stuffOnTile.Add ((GameObject)Instantiate (tree));
						}
					}
					if (placeStuff < 8) {
						int numberOfStones = UnityEngine.Random.Range (1, 4);
						for (int i = 0; i < numberOfStones; i++) {
							int stone = UnityEngine.Random.Range (0, stones.Count);
							stuffOnTile.Add ((GameObject)Instantiate (stones [stone]));
						}
					}
					break;
				case 5:
					stuffOnTile.Add ((GameObject)Instantiate (mountain));
					tb.tile.Passable = false;
					rand = false;
					break;
				}
				for (int i = 0; i < stuffOnTile.Count; i++) {
					float randX = UnityEngine.Random.Range (-3, 3);
					float randZ = UnityEngine.Random.Range (-3, 3);
					if (!rand) {
						randX = randZ = 0;
					}

					stuffOnTile [i].transform.position = hex.transform.position;
					Vector3 temp = new Vector3 (randX, 0f, randZ);
					stuffOnTile [i].transform.position += temp;
				}
			}
		}
		if (players > 0) {
			addObjsToLists (board [new Point (0, 0)], fanatic, 0);
			addObjsToLists (board [new Point (0, 1)], fanatic, 0);
			createObject (board [new Point (0, 1)], junkyard, 0);
			addObjsToLists (board [new Point (1, 0)], fanatic, 0);
			createObject (board [new Point (1, 0)], windmill, 0);
			board [new Point (0, 1)].built = true;
			board [new Point (1, 0)].built = true;
		}
		if (players > 1) {
			int temp = players == 4 ? 3 : 1; 
			addObjsToLists (board [new Point (9, 10)], fanatic, temp);
			addObjsToLists (board [new Point (8, 10)], fanatic, temp);
			createObject (board [new Point (8, 10)], windmill, temp);
			addObjsToLists (board [new Point (8, 9)], fanatic, temp);
			createObject (board [new Point (8, 9)], junkyard, temp);
			board [new Point (8, 10)].built = true;
			board [new Point (8, 9)].built = true;
		}
		if (players > 2) {
			addObjsToLists (board [new Point (9, 0)], fanatic, 1);
			addObjsToLists (board [new Point (8, 1)], fanatic, 1);
			createObject (board [new Point (8, 1)], junkyard, 1);
			addObjsToLists (board [new Point (8, 0)], fanatic, 1);
			createObject (board [new Point (8, 0)], windmill, 1);
			board [new Point (8, 1)].built = true;
			board [new Point (8, 0)].built = true;
		}
		if (players > 3) {
			addObjsToLists (board [new Point (0, 10)], fanatic, 2);
			addObjsToLists (board [new Point (0, 9)], fanatic, 2);
			createObject (board [new Point (0, 9)], windmill, 2);
			addObjsToLists (board [new Point (1, 10)], fanatic, 2);
			createObject (board [new Point (1, 10)], junkyard, 2);
			board [new Point (0, 9)].built = true;
			board [new Point (1, 10)].built = true;
		}

		//variable to indicate if all rows have the same number of hexes in them
		//this is checked by comparing width of the first hex row plus half of the hexWidth with groundWidth
		bool equalLineLengths = (gridSize.x + 0.5) * hexWidth <= groundWidth;
		//Neighboring tile coordinates of all the tiles are calculated
		foreach (TileBehaviour tb in board.Values)
			tb.tile.FindNeighbours (tempBoard, gridSize, equalLineLengths);
	}

	private void clearSelectionMenu ()
	{
		foreach (GameObject sel in selectionMenu) {
			RawImage img = (RawImage)sel.GetComponent<RawImage> ();
			img.texture = null;
			((RawImage)sel.GetComponent<RawImage> ()).color = Color.clear;
		}
	}

	private bool onTile (TileBehaviour tb, String type)
	{
		foreach (GameObject objOnTile in tb.objsOnTile) {
			GOProperties gop = (GOProperties)objOnTile.GetComponent (typeof(GOProperties));
			if (gop.type == type) {
				gop.Quantity++;
				return true;
			}
		}
		return false;
	}

	private int addObjsToLists (TileBehaviour tb, GameObject go, int tID)
	{
		if (!onTile (tb, go.name.ToString ())) {
			GameObject ngo = createObject (tb, go, tID);
			GOProperties gop = (GOProperties)ngo.GetComponent (typeof(GOProperties));
			gameobjects.Add (gop.UniqueID, ngo);
			return gop.UniqueID;
		}
		return tb.objectTypeExists (go.name.ToString ());
	}

	private bool hasEnough (int playerId, int[] cost)
	{
		PlayerData p = playerData [playerId];
		if (p.water < cost [0]) {
			return false;
		}
		if (p.petrol < cost [1]) {
			return false;
		}
		if (p.scrap < cost [2]) {
			return false;
		}
		return true;
	}

	private bool hasEnoughAndDeduct (int playerId, int[] cost)
	{
		if (hasEnough (playerId, cost)) {
			PlayerData p = playerData [playerId];
			p.water -= cost [0];
			p.petrol -= cost [1];
			p.scrap -= cost [2];
			return true;
		} else {
			print ("Not enough resources to build!");
		}
		return false;
	}

	public void buildOnTile (GameObject selection)
	{
		bool build = false;
		foreach (GameObject unit in unitSelected) {
			GOProperties gop = (GOProperties)unit.GetComponent (typeof(GOProperties));
			if (gop.type == "ThirdPersonController")
				build = true;
		}
		if (unitSelected.Count != 0 & build == true) {
			GameObject hexGrid = retrieveTile (unitSelected.First ());
			if (hexGrid == null) {
				Debug.Log ("No Tile Found");
				return;
			}
			int tId = unitSelected.First ().GetComponent<GOProperties> ().PlayerId;
			TileBehaviour tb = (TileBehaviour)hexGrid.GetComponent ("TileBehaviour");
			if (tb.getTile ().getLandType () == LandType.Base) {
				if (selection.name.Equals ("sel0")) {
					if (hasEnoughAndDeduct (tId, fanaticCost)) {
						int guid = addObjsToLists (tb, fanatic, tId);
						show (gameobjects[guid], true);
					}
				} else if (selection.name.Equals ("sel1")) {
					if (hasEnoughAndDeduct (tId, bikeCost)) {
						int guid = addObjsToLists (tb, bike, tId);
						show (gameobjects[guid], true);
					}
				} else if (selection.name.Equals ("sel2")) {
					if (hasEnoughAndDeduct (tId, carCost)) {
						int guid = addObjsToLists (tb, car, tId);
						show (gameobjects[guid], true);
					}
				} else if (selection.name.Equals ("sel3")) {
					if (hasEnoughAndDeduct (tId, truckCost)) {
						int guid = addObjsToLists (tb, truck, tId);
						show (gameobjects[guid], true);
					}
				}
			} else if (!tb.built && !tb.upgraded) {
				switch (tb.getTile ().getLandType ()) {
				case LandType.Oasis:
					if (selection.name.Equals ("sel0")) {
						if (hasEnoughAndDeduct (tId, waterMillCost)) {
							GameObject temp = createObject (tb, windmill, tId);
							GOProperties gop = (GOProperties)temp.GetComponent (typeof(GOProperties));
							gop.initStructureShown (tId, players);
							tb.built = true;
						}
					}
					break;
				case LandType.OilField:
					if (selection.name.Equals ("sel0")) {
						if (hasEnoughAndDeduct (tId, refineryCost)) {
							GameObject temp = createObject (tb, refinery, tId);
							GOProperties gop = (GOProperties)temp.GetComponent (typeof(GOProperties));
							gop.initStructureShown (tId, players);
							tb.built = true;
						}
					}
					break;
				case LandType.Junkyard:
					if (selection.name.Equals ("sel0")) {
						if (hasEnoughAndDeduct (tId, junkYardCost)) {
							GameObject temp = createObject (tb, junkyard, tId);
							GOProperties gop = (GOProperties)temp.GetComponent (typeof(GOProperties));
							gop.initStructureShown (tId, players);
							tb.built = true;
						}
					}
					break;
				default:
					break;
				}
				clearSelectionMenu ();
			} else if (tb.built && !tb.upgraded) {
				switch (tb.getTile ().getLandType ()) {
				default:
					if (selection.name.Equals ("sel0")) {
						if (hasEnoughAndDeduct (tId, structureUpgradeCost)) {
							tb.upgraded = true;
						}
					}
					break;
				}
				clearSelectionMenu ();
			}
		}
	}

	private void updateSelectionMenu (TileBehaviour tb)
	{
		LandType type = LandType.Desert;
		if (tb != null) {
			type = tb.getTile ().landType;
		}

		clearSelectionMenu ();
		bool build = false;
		foreach (GameObject unit in unitSelected) {
			GOProperties gop = (GOProperties)unit.GetComponent (typeof(GOProperties));
			if (gop.type == "ThirdPersonController")
				build = true;
		}
		if (build == true) {
			if (type == LandType.Base) {
				((RawImage)selectionMenu [0].GetComponent<RawImage> ()).texture = textures [0];
				((RawImage)selectionMenu [0].GetComponent<RawImage> ()).color = Color.white;
				((RawImage)selectionMenu [1].GetComponent<RawImage> ()).texture = textures [1];
				((RawImage)selectionMenu [1].GetComponent<RawImage> ()).color = Color.white;
				((RawImage)selectionMenu [2].GetComponent<RawImage> ()).texture = textures [2];
				((RawImage)selectionMenu [2].GetComponent<RawImage> ()).color = Color.white;
				((RawImage)selectionMenu [3].GetComponent<RawImage> ()).texture = textures [3];
				((RawImage)selectionMenu [3].GetComponent<RawImage> ()).color = Color.white;
			} else if (!tb.built && !tb.upgraded) { 			// check if tile is built or needs an upgrade
				switch (type) {
				case LandType.Oasis:
					((RawImage)selectionMenu [0].GetComponent<RawImage> ()).texture = textures [5];
					((RawImage)selectionMenu [0].GetComponent<RawImage> ()).color = Color.white;
					break;
				case LandType.OilField:
					((RawImage)selectionMenu [0].GetComponent<RawImage> ()).texture = textures [4];
					((RawImage)selectionMenu [0].GetComponent<RawImage> ()).color = Color.white;
					break;
				case LandType.Junkyard:
					((RawImage)selectionMenu [0].GetComponent<RawImage> ()).texture = textures [6];
					((RawImage)selectionMenu [0].GetComponent<RawImage> ()).color = Color.white;
					break;
				default:
					break;
				}
			} else if (tb.built && !tb.upgraded) {
				switch (type) {
				case LandType.Oasis:
					((RawImage)selectionMenu [0].GetComponent<RawImage> ()).texture = textures [7];
					((RawImage)selectionMenu [0].GetComponent<RawImage> ()).color = Color.white;
					break;
				case LandType.OilField:
					((RawImage)selectionMenu [0].GetComponent<RawImage> ()).texture = textures [7];
					((RawImage)selectionMenu [0].GetComponent<RawImage> ()).color = Color.white;
					break;
				case LandType.Junkyard:
					((RawImage)selectionMenu [0].GetComponent<RawImage> ()).texture = textures [7];
					((RawImage)selectionMenu [0].GetComponent<RawImage> ()).color = Color.white;
					break;
				default:
					break;
				}
			}
		}
	}

	private GameObject retrieveTile (GameObject obj)
	{
		RaycastHit hitInfo = new RaycastHit ();
		int mask = 1 << LayerMask.NameToLayer ("grid");
		Vector3 start = obj.transform.position;
		start += new Vector3 (0f, 0.2f, 0f);
		Debug.DrawRay (start, Vector3.down, Color.green, 30f, false);
		if (Physics.Raycast (start, Vector3.down, out hitInfo, mask)) {
			return hitInfo.transform.gameObject;
		}
		return null;
	}

	private TileBehaviour retrieveTileBehaviourOfObject (GameObject obj)
	{
		GameObject hit = retrieveTile (obj);
		if (hit != null) {
			TileBehaviour tb = (TileBehaviour)hit.GetComponent ("TileBehaviour");
			if (tb == null) {
				Debug.Log ("TB=null: " + hit.name);
				return null;
			}
			return tb;
		}
		return null;
	}

	private LandType retrieveTileLandTypeOfObject (GameObject obj)
	{
		TileBehaviour tb = retrieveTileBehaviourOfObject (obj);
		if (tb != null) {
			return tb.getTile ().landType;
		}
		return LandType.Desert;
	}

	private GameObject createObject (TileBehaviour tb, GameObject obj, int teamId)
	{
		return createObject (tb, obj, teamId, true);
	}

	private GameObject createObject (TileBehaviour tb, GameObject obj, int teamId, bool join)
	{
		GameObject go = Instantiate (obj);
		GOProperties gop = (GOProperties)go.GetComponent (typeof(GOProperties));
		gop.setUId (this.getId ());
		gop.setPId (teamId); 
		gop.type = obj.name.ToString ();
		gop.Quantity = 1;
		originTileTB.Add (gop.UniqueID, tb);
		destTileTB.Add (gop.UniqueID, tb);
		ObjsPaths.Add (gop.UniqueID, new List<GameObject> ());
		go.transform.position = tb.getNextPosition (go, join);
		//Debug.Log ("Created Object: "+gop.type);
		switch (gop.type) {
		case "ThirdPersonController":
			gop.setAV (1);
			gop.setDV (1);
			gop.setMV (1);
			go.transform.position += new Vector3 (0f, 0.2f, 0f);
			break;
		case "Apo_Car_2015":
			gop.setAV (2);
			gop.setDV (2);
			gop.setMV (15);
			go.transform.position += new Vector3 (0f, 0.5f, 0f);
			break;
		case "f_noladder":
			gop.setAV (3);
			gop.setDV (3);
			gop.setMV (2);
			go.transform.position += new Vector3 (0f, 0.2f, 0f);
			break;
		case "bike":
			gop.setAV (2);
			gop.setDV (1);
			gop.setMV (3);
			go.transform.position += new Vector3 (0f, 0.2f, 0f);
			break;
		}
		return go;
	}

	//Distance between destination tile and some other tile in the grid
	//	double calcDistance(Tile tile)
	//	{
	//		Tile destTile = destTileTB.tile;
	//		//Formula used here can be found in Chris Schetter's article
	//		float deltaX = Mathf.Abs(destTile.X - tile.X);
	//		float deltaY = Mathf.Abs(destTile.Y - tile.Y);
	//		int z1 = -(tile.X + tile.Y);
	//		int z2 = -(destTile.X + destTile.Y);
	//		float deltaZ = Mathf.Abs(z2 - z1);
	//
	//		return Mathf.Max(deltaX, deltaY, deltaZ);
	//	}

	private void DrawPath (IEnumerable<Tile> path, int id)
	{
		if (this.ObjsPaths [id] == null)
			this.ObjsPaths [id] = new List<GameObject> ();
		
		//DestroyPath(id);
		//Lines game object is used to hold all the "Line" game objects indicating the path
		string linesName = "Lines" + getCurrentPlayerId ();
		GameObject lines = GameObject.Find (linesName);
		if (lines == null)
			lines = new GameObject (linesName);
		foreach (Tile tile in path) {
			var line = (GameObject)Instantiate (Line);
			//calcWorldCoord method uses squiggly axis coordinates so we add y / 2 to convert x coordinate from straight axis coordinate system
			Vector2 gridPos = new Vector2 (tile.X + tile.Y / 2, tile.Y);
			line.transform.position = calcWorldCoord (gridPos);
			this.ObjsPaths [id].Add (line);
			line.transform.parent = lines.transform;
		}
	}

	public void DestroyPath (int id)
	{
	
		//Destroy game objects which used to indicate the path
		ObjsPaths [id].ForEach (Destroy);
		ObjsPaths [id].Clear ();	
	}

	public void DestroyTilesPath (int id)
	{

		//Destroy game objects which used to indicate the path
		ObjsPathsTiles.Remove (id);
	}

	public void generateAndShowPath ()
	{	
		foreach (GameObject unit in GridManager.unitSelected) {
			if (unit != null) {
				GOProperties gop = (GOProperties)unit.GetComponent (typeof(GOProperties));
				//Don't do anything if origin or destination is not defined yet
				if (originTileTB [gop.UniqueID] == null || destTileTB [gop.UniqueID] == null) {
					DrawPath (new List<Tile> (), gop.UniqueID);
					deSelect ();
					return;
				}
				DestroyPath (gop.UniqueID);
				if (destTileTB [gop.UniqueID] != originTileTB [gop.UniqueID]) {
					var path = PathFinder.FindPath (originTileTB [gop.UniqueID].tile, destTileTB [gop.UniqueID].tile);
					if (path.TotalCost <= gop.MovementValue) {
						DrawPath (path, gop.UniqueID);
						ObjsPathsTiles [gop.UniqueID] = path;
					}
					if (GridManager.unitSelected.Count == 1) {
						showSplitMenu = true;
						splitMenuPos = board [path.LastStep.boardCoords].transform.position;
						splitSelection = GridManager.unitSelected.First ();
					}
				}
			}
		}
		deSelect ();
	}

	private List<int> LoadGameFile ()
	{
		List<int> map = new List<int> ();
		string fileName = "Assets/Resources/testingMap";
		string line;
		StreamReader theReader = new StreamReader (fileName, Encoding.Default);
		using (theReader) {
			do {
				line = theReader.ReadLine ();
				if (line != null) {
					string[] entries = line.Split (',');
					foreach (string entry in entries) {
						map.Add (Int32.Parse (entry));
					}
				}
			} while (line != null);
			theReader.Close ();
			return map;
		}
	}
}
