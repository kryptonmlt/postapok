﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Random=UnityEngine.Random;
using System.Text;
using System.IO;
using UnityEngine.UI;

public class GridManager: MonoBehaviour
{
	public int ID=0;
	public Button endTurnButton;

	public int getId() {
		return ID++;
	}

	public LinkedList<GameObject> gameobjects = new LinkedList<GameObject> ();
	public Dictionary<int, List<GameObject>> ObjsPaths = new Dictionary<int, List<GameObject>> ();
	public Dictionary<int, Path<Tile>> ObjsPathsTiles = new Dictionary<int, Path<Tile>> ();

	//selectedTile stores the tile mouse cursor is hovering on
	public Tile selectedTile = null;
	//TB of the tile which is the start of the path
	public Dictionary<int, TileBehaviour> originTileTB = new Dictionary<int, TileBehaviour>();
	public Dictionary<int, TileBehaviour> destTileTB = new Dictionary<int, TileBehaviour>();

	public Dictionary<int, TileBehaviour> getOriginTileTB(){
		return originTileTB;
	}
	//TB of the tile which is the end of the path
	public TileBehaviour tb =null;

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
	static bool draw=false;

	private Texture2D rectangleTexture;
	public float fAlpha=0.25f;

	private GameObject camp;
	private GameObject mountain;
	private GameObject tree;
	private GameObject Hex;
	private GameObject Ground;
	private GameObject fanatic;
	private GameObject truck;
	private GameObject car;
	private GameObject bike;
	private List<GameObject> stones = new List<GameObject>();

	public static LinkedList<GameObject> unitSelected=new LinkedList<GameObject>();

	Dictionary<int, LandType> TerrainType = new Dictionary<int, LandType>()
	{
		{0,LandType.Base},
		{1,LandType.Oasis},
		{2,LandType.Junkyard},
		{3,LandType.OilField},
		{4,LandType.Desert},
		{5,LandType.Mountain}
	};

	public void deSelect(){
		GridManager.unitSelected = new LinkedList<GameObject> ();
	}


	void setSizes()
	{
		hexWidth = Hex.GetComponent<Renderer>().bounds.size.x;
		hexHeight = Hex.GetComponent<Renderer>().bounds.size.z;
		groundWidth = Ground.GetComponent<Renderer>().bounds.size.x;
		groundOffset = Ground.GetComponent<Renderer> ().transform.position.y;
		groundHeight = Ground.GetComponent<Renderer>().bounds.size.z;
	}

	void OnGUI() {
		if (GridManager.draw == true) {
			Color colPreviousGUIColor = GUI.color;
			GUI.color = new Color(colPreviousGUIColor.r, colPreviousGUIColor.g, colPreviousGUIColor.b, fAlpha);
			GUI.DrawTexture (new Rect (downmouseposition.x, Screen.height-downmouseposition.y, Input.mousePosition.x - downmouseposition.x, downmouseposition.y-Input.mousePosition.y), rectangleTexture);
			GUI.color = colPreviousGUIColor;
		}
		Vector2 targetPos;
		foreach (GameObject go in gameobjects) {
			if (go != null) {
				targetPos = Camera.main.WorldToScreenPoint (go.transform.position);
				CharacterMovement characterAction = (CharacterMovement)go.GetComponent(typeof(CharacterMovement));
				GUI.Box (new Rect (targetPos.x, Screen.height - targetPos.y, 20, 20), characterAction.quantity.ToString());
			}
		}

	}

	void Update(){

		bool moving = false;
		foreach (GameObject active in GameObject.FindGameObjectsWithTag("Unit")) {
			if (active != null) {
				CharacterMovement characterAction = (CharacterMovement)active.GetComponent (typeof(CharacterMovement));
				if (characterAction.IsMoving == true) {
					moving = characterAction.IsMoving;
				}
			}
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

		if (Input.GetMouseButtonDown (0) & !unitSelected.Any() & moving==false) {
			GridManager.downmouseposition = Input.mousePosition;
			GridManager.draw = true;
			
		} else if (Input.GetMouseButtonUp (0) & !unitSelected.Any() & moving==false) {

			// Single hit 
			RaycastHit hitInfo = new RaycastHit();
			bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);
			GameObject selected = hitInfo.transform.gameObject;;
			if (hit) {
				if (hitInfo.transform.gameObject.tag == "Unit") {
					unitSelected.AddLast (hitInfo.transform.gameObject);
					Renderer[] renderers= hitInfo.transform.gameObject.GetComponentsInChildren<Renderer> ();
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
					if (unit != selected) {
						unitSelected.AddLast (unit);
						Renderer[] renderers = unit.GetComponentsInChildren<Renderer> ();
						foreach (Renderer renderer in renderers) {
							renderer.material.shader = selfIllumShader;
						}
					}
				}
			}
		}
	}

	//The method used to calculate the number hexagons in a row and number of rows
	//Vector2.x is gridWidthInHexes and Vector2.y is gridHeightInHexes
	Vector2 calcGridSize(){
		//According to the math textbook hexagon's side length is half of the height
		float sideLength = hexHeight / 2;
		//the number of whole hex sides that fit inside inside ground height
		int nrOfSides = (int)(groundHeight / sideLength);
		//I will not try to explain the following calculation because I made some assumptions, which might not be correct in all cases, to come up with the formula. So you'll have to trust me or figure it out yourselves.
		int gridHeightInHexes = (int)( nrOfSides * 2 / 3);
		//When the number of hexes is even the tip of the last hex in the offset column might stick up.
		//The number of hexes in that case is reduced.
		if (gridHeightInHexes % 2 == 0
			&& (nrOfSides + 0.5f) * sideLength > groundHeight)
			gridHeightInHexes--;
		//gridWidth in hexes is calculated by simply dividing ground width by hex width
		return new Vector2((int)(groundWidth / hexWidth), gridHeightInHexes);
	}
	//Method to calculate the position of the first hexagon tile
	//The center of the hex grid is (0,0,0)
	Vector3 calcInitPos()
	{
		Vector3 initPos;
		initPos = new Vector3(-groundWidth / 2 + hexWidth / 2, 0,
			groundHeight / 2 - hexWidth / 2);

		return initPos;
	}

	public Vector3 calcWorldCoord(Vector2 gridPos)
	{
		Vector3 initPos = calcInitPos();
		float offset = 0;
		if (gridPos.y % 2 != 0)
			offset = hexWidth / 2;

		float x =  initPos.x + offset + gridPos.x * hexWidth;
		float z = initPos.z - gridPos.y * hexHeight * 0.75f;
		//If your ground is not a plane but a cube you might set the y coordinate to sth like groundDepth/2 + hexDepth/2
		return new Vector3(x, groundOffset, z);
	}

	void endTurnTask()
	{
		Debug.Log("Finished Turn!");
		foreach (GameObject unit in GameObject.FindGameObjectsWithTag("Unit")) {
			GOProperties gop = (GOProperties) unit.GetComponent (typeof(GOProperties));
			if (unit != null & ObjsPathsTiles.ContainsKey(gop.UniqueID)) {
				Debug.Log ("found one");
				CharacterMovement characterAction = (CharacterMovement)unit.GetComponent (typeof(CharacterMovement));
				characterAction.StartMoving (ObjsPathsTiles[gop.UniqueID].ToList ());
			}
		}
	}

	void Start()
	{
		LoadResources();
		Button btn = endTurnButton.GetComponent<Button>();
		btn.onClick.AddListener(endTurnTask);

		rectangleTexture= new Texture2D (1, 1);
		rectangleTexture.SetPixel (0, 0, Color.black);
		rectangleTexture.Apply();
		selfIllumShader = Shader.Find("Outlined/Silhouetted Bumped Diffuse");
		standardShader = Shader.Find("Standard");
		setSizes();
		createGrid();
		generateAndShowPath();
	}

	void Awake()
	{
		instance = this;
	}

	void LoadResources(){
		camp = Resources.Load ("Models/structures/barracks/prefabs/barracks", typeof(GameObject)) as GameObject;
		mountain = Resources.Load ("Models/extras/mountain/prefab/mountain", typeof(GameObject)) as GameObject;
		tree = Resources.Load ("Models/extras/trees/Prefabs/tree", typeof(GameObject)) as GameObject;
		Hex = Resources.Load ("Models/Grid/HexGrid/prefab/HexGrid", typeof(GameObject)) as GameObject;
		Ground = Resources.Load ("Models/Grid/GroundSize/prefab/Ground", typeof(GameObject)) as GameObject;
		fanatic = Resources.Load ("Models/ThirdCharacterController/Prefabs/ThirdPersonController", typeof(GameObject)) as GameObject;
		truck = Resources.Load ("Models/Vehicles/F04/Prefabs/f_noladder", typeof(GameObject)) as GameObject;
		car = Resources.Load ("Models/Vehicles/car/Prefabs/Apo_car_2015", typeof(GameObject)) as GameObject;
		bike = Resources.Load ("Models/Vehicles/bike/prefabs/bike", typeof(GameObject)) as GameObject;
		stones.Add(Resources.Load ("Models/extras/stone/Prefabs/rockUp0", typeof(GameObject)) as GameObject);
		stones.Add(Resources.Load ("Models/extras/stone/Prefabs/rockUp1", typeof(GameObject)) as GameObject);
		stones.Add(Resources.Load ("Models/extras/stone/Prefabs/rockUp2", typeof(GameObject)) as GameObject);
		stones.Add(Resources.Load ("Models/extras/stone/Prefabs/rockUp3", typeof(GameObject)) as GameObject);
	}

	void createGrid()
	{
		List<int> loadedMap = LoadGameFile();
		Vector2 gridSize = calcGridSize();
		GameObject hexGridGO = new GameObject("HexGrid");
		//board is used to store tile locations
		Dictionary<Point, Tile> board = new Dictionary<Point, Tile>();
		int landPos = 0;
		for (float y = 0; y < gridSize.y; y++)
		{
			float sizeX = gridSize.x;
			//if the offset row sticks up, reduce the number of hexes in a row
			if (y % 2 != 0 && (gridSize.x + 0.5) * hexWidth > groundWidth)
				sizeX--;
			for (float x = 0; x < sizeX; x++,landPos++)
			{
				GameObject hex = (GameObject)Instantiate(Hex);
				Vector2 gridPos = new Vector2(x, y);
				hex.transform.position = calcWorldCoord(gridPos);
				hex.transform.parent = hexGridGO.transform;
				var tb = (TileBehaviour)hex.GetComponent("TileBehaviour");

				int landTypeId = loadedMap [landPos];
				tb.tile = new Tile((int)x - (int)(y / 2), (int)y, TerrainType[landTypeId]);
				tb.setTileMaterial(tb.tile.landType);
				board.Add(tb.tile.Location, tb.tile);

				List<GameObject> stuffOnTile = new List<GameObject>();
				bool rand = true;
				switch(landTypeId){
				case 0:
					stuffOnTile.Add ((GameObject)Instantiate (camp));
					rand = false;
					break;
				case 1:
					int n = UnityEngine.Random.Range(3,6);
					for(int i=0;i<n;i++){
						stuffOnTile.Add((GameObject)Instantiate (tree));
					}
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
							stuffOnTile.Add ((GameObject)Instantiate (stones[stone]));
						}
					}
					break;
				case 5:
					stuffOnTile.Add ((GameObject)Instantiate (mountain));
					tb.tile.Passable = false;
					rand = false;
					break;
				}
				for(int i = 0;i <stuffOnTile.Count; i++){
					float randX = UnityEngine.Random.Range(-3,3);
					float randZ = UnityEngine.Random.Range(-3,3);
					if (!rand) {
						randX = randZ = 0;
					}

					stuffOnTile[i].transform.position = hex.transform.position;
					Vector3 temp = new Vector3 (randX,0f,randZ);
					stuffOnTile[i].transform.position += temp;
				}

				if (x == 0 && y == 0)
				{
					gameobjects.AddLast(createObject(tb,fanatic,1));
				}
				if (x == 2 && y == 3)
				{
					gameobjects.AddLast(createObject(tb,fanatic,1));
				}
				if (x == 4 && y == 5)
				{
					gameobjects.AddLast(createObject (tb, car,2));
				}
				if (x == 6 && y == 7)
				{
					gameobjects.AddLast(createObject (tb, truck,1));
				}
			}
		}
		//variable to indicate if all rows have the same number of hexes in them
		//this is checked by comparing width of the first hex row plus half of the hexWidth with groundWidth
		bool equalLineLengths = (gridSize.x + 0.5) * hexWidth <= groundWidth;
		//Neighboring tile coordinates of all the tiles are calculated
		foreach(Tile tile in board.Values)
			tile.FindNeighbours(board, gridSize, equalLineLengths);
	}

	private GameObject createObject(TileBehaviour tb,GameObject obj,int id){
		GameObject go = Instantiate (obj);
		go.transform.position = tb.transform.position;
		GOProperties gop = (GOProperties) go.GetComponent (typeof(GOProperties));
		gop.setUId (this.getId());
		gop.setPId (id);
		String type = obj.ToString ();
		originTileTB.Add(gop.UniqueID,tb);
		destTileTB.Add(gop.UniqueID,tb);
		ObjsPaths.Add(gop.UniqueID,new List<GameObject>());
		switch (type)
		{
		case "fanatic":
			gop.setAV(1);
			gop.setDV(1);
			gop.setMV(1);
			break;
		case "car":
			gop.setAV(2);
			gop.setDV(2);
			gop.setMV(2);
			break;
		case "truck":
			gop.setAV(3);
			gop.setDV(3);
			gop.setMV(2);
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

	private void DrawPath(IEnumerable<Tile> path, int id)
	{
		if (this.ObjsPaths[id] == null)
			this.ObjsPaths[id] = new List<GameObject>();
		
		//DestroyPath(id);
		//Lines game object is used to hold all the "Line" game objects indicating the path
		GameObject lines = GameObject.Find("Lines");
		if (lines == null)
			lines = new GameObject("Lines");
		foreach (Tile tile in path)
		{
			var line = (GameObject)Instantiate(Line);
			//calcWorldCoord method uses squiggly axis coordinates so we add y / 2 to convert x coordinate from straight axis coordinate system
			Vector2 gridPos = new Vector2(tile.X + tile.Y / 2, tile.Y);
			line.transform.position = calcWorldCoord(gridPos);
			this.ObjsPaths[id].Add(line);
			line.transform.parent = lines.transform;
		}
	}

	public void DestroyPath(int id){
	
		//Destroy game objects which used to indicate the path
		ObjsPaths[id].ForEach(Destroy);
		ObjsPaths[id].Clear();	
	}

//	public bool isEmptyPath(){
//		if (this.path == null)
//			return true;
//		else return false;
//	}

	public void generateAndShowPath()
	{	
		foreach(GameObject unit in GridManager.unitSelected) {
			if (unit!=null) {
				GOProperties gop = (GOProperties) unit.GetComponent (typeof(GOProperties));
				//Don't do anything if origin or destination is not defined yet
				if (originTileTB [gop.UniqueID] == null || destTileTB [gop.UniqueID] == null) {
					DrawPath (new List<Tile> (),gop.UniqueID);
					return;
				}
				DestroyPath (gop.UniqueID);

				var path = PathFinder.FindPath (originTileTB [gop.UniqueID].tile, destTileTB[gop.UniqueID].tile);
				DrawPath (path,gop.UniqueID);
				ObjsPathsTiles [gop.UniqueID] = path;

				//remove highlight
				Renderer[] renderers = unit.GetComponentsInChildren<Renderer> ();
				foreach (Renderer renderer in renderers) {
					renderer.material.shader = standardShader;
				}
			}
		}
		deSelect ();
	}

	private List<int> LoadGameFile()
	{
			List<int> map = new List<int>();
			string fileName = "Assets/Resources/testingMap";
			string line;
			StreamReader theReader = new StreamReader(fileName, Encoding.Default);
			using (theReader)
			{
				do
				{
					line = theReader.ReadLine();
					if (line != null)
					{
						string[] entries = line.Split(',');
						foreach(string entry in entries){
							map.Add(Int32.Parse(entry));
						}
					}
				}
				while (line != null);
				theReader.Close();
				return map;
			}
	}
}
