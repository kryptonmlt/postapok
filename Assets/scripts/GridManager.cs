using UnityEngine;
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

	public List<GameObject> gameobjects = new List<GameObject> ();
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
	private bool clicked =false;

	private Texture2D rectangleTexture;
	public float fAlpha=0.25f;

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
	private List<GameObject> stones = new List<GameObject>();
	private List<GameObject> selectionMenu = new List<GameObject>();
	//0=fanatic,1=bike,2=car,3=truck,4=refinery,5=windmill,6=junkyard
	private List<Texture> textures = new List<Texture>();

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
		clearSelectionMenu ();
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
				GOProperties gop = (GOProperties)go.GetComponent (typeof(GOProperties));
				CharacterMovement characterAction = (CharacterMovement)go.GetComponent(typeof(CharacterMovement));
				GUI.Box (new Rect (targetPos.x, Screen.height - targetPos.y, 20, 20), gop.quantity.ToString());
			}
		}

	}

	void Update(){

		bool moving = isAnyMoving ();

		if (!moving) {
			resolution ();
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
			clicked = true;
			
		} else if (Input.GetMouseButtonUp (0) & !unitSelected.Any() & moving==false & clicked==true) {

			// Single hit 
			RaycastHit hitInfo = new RaycastHit();
			GameObject selected = null;
			if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo)) {
				selected = hitInfo.transform.gameObject;
				if (hitInfo.transform.gameObject.tag == "Unit" & !unitSelected.Contains(hitInfo.transform.gameObject)) {
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
					if(selected!=null && unit !=selected){
						unitSelected.AddLast (unit);
						Renderer[] renderers = unit.GetComponentsInChildren<Renderer> ();
						foreach (Renderer renderer in renderers) {
							renderer.material.shader = selfIllumShader;
						}
					}
				}
			}
			if(unitSelected.Count==1){
				LandType landHit = retrieveTileOfObject (unitSelected.First());
				Debug.Log(landHit);
				updateSelectionMenu(landHit);
			}
			clicked = false;
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

	public bool isAnyMoving(){
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

	void endTurnTask()
	{
		bool moving = isAnyMoving ();
		if (!moving) {
			GridManager.draw = false;
			foreach (GameObject unit in GameObject.FindGameObjectsWithTag("Unit")) {
				GOProperties gop = (GOProperties)unit.GetComponent (typeof(GOProperties));
				if (unit != null & ObjsPathsTiles.ContainsKey (gop.UniqueID)) {
					CharacterMovement characterAction = (CharacterMovement)unit.GetComponent (typeof(CharacterMovement));
					characterAction.StartMoving (ObjsPathsTiles [gop.UniqueID].ToList ());
				}
			}
		}
	}

	void resolution(){
		List<GameObject> attackers = new List<GameObject> ();
		List<GameObject> defenders = new List<GameObject> ();

		for (int i = gameobjects.Count-1;i>=0;i--){
			GOProperties gop1 = (GOProperties)gameobjects[i].GetComponent (typeof(GOProperties));
			for (int j = gameobjects.Count-1;j>=0;j--){
				int pi1 = (int) gameobjects [i].transform.position [0];
				int pi2 = (int) gameobjects [i].transform.position [1];
				int pi3 = (int) gameobjects [i].transform.position [2];
				int pj1 = (int) gameobjects [j].transform.position [0];
				int pj2 = (int) gameobjects [j].transform.position [1];
				int pj3 = (int) gameobjects [j].transform.position [2];
				GOProperties gop2 = (GOProperties)gameobjects[j].GetComponent (typeof(GOProperties));
				if (gop1.UniqueID != gop2.UniqueID & originTileTB [gop1.UniqueID].tile == originTileTB [gop2.UniqueID].tile & gop1.PlayerId == gop2.PlayerId) {
					if (gop1.type == gop2.type) {
						gop1.quantity += gop2.quantity;
						Destroy (gameobjects [j]);
						gameobjects.Remove (gameobjects [j]);
						i--;
					} else if (gop1.type != gop2.type & pi1==pj1 & pi2==pj2 & pi3==pj3) {
						gameobjects [i].transform.Translate (originTileTB [gop1.UniqueID].getNextPosition());
						originTileTB [gop1.UniqueID].getNextPosition ();
					}
				} else if (gop1.UniqueID != gop2.UniqueID & originTileTB [gop1.UniqueID].tile == originTileTB [gop2.UniqueID].tile & gop1.PlayerId != gop2.PlayerId) {
					if (!attackers.Contains (gameobjects [i]) & !defenders.Contains (gameobjects [i])) {
						attackers.Add (gameobjects [i]);
					}
					if (!attackers.Contains (gameobjects [j]) & !defenders.Contains (gameobjects [j])) {
						defenders.Add (gameobjects [j]);
					}
				}

			}
		}
		int attackersvalue = 0;
		int defendersvalue = 0;
		foreach (GameObject attacker in attackers) {
			GOProperties gop = (GOProperties)attacker.GetComponent (typeof(GOProperties));
			attackersvalue += gop.AttackValue * gop.quantity;
		}
		foreach (GameObject defender in defenders) {
			GOProperties gop = (GOProperties)defender.GetComponent (typeof(GOProperties));
			defendersvalue += gop.AttackValue * gop.quantity;
		}

		if (attackersvalue >= defendersvalue) {
			foreach (GameObject defender in defenders) {
				Destroy (defender);
				gameobjects.Remove (defender);
			}
		} else {
			foreach (GameObject attacker in attackers) {
				Debug.Log (attackersvalue);
				Debug.Log (defendersvalue);
				Destroy (attacker);
				gameobjects.Remove (attacker);
			}
		}
	}

	void Start()
	{
		LoadResources();
		for(int i=0;i<4;i++){
			selectionMenu.Add(GameObject.Find("sel"+i));
		}
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
		refinery = Resources.Load ("Models/structures/petrolfactory/Prefabs/refinery", typeof(GameObject)) as GameObject;
		windmill = Resources.Load ("Models/structures/windmill/prefabs/windmIll", typeof(GameObject)) as GameObject;
		junkyard = Resources.Load ("Models/structures/petrolfactory/Prefabs/refinery", typeof(GameObject)) as GameObject;
		mountain = Resources.Load ("Models/extras/mountain/prefab/mountain", typeof(GameObject)) as GameObject;
		junk = Resources.Load ("Models/extras/junkLand/Prefabs/junkmount", typeof(GameObject)) as GameObject;
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
		textures.Add(Resources.Load("Textures/fanatic") as Texture);
		textures.Add(Resources.Load("Textures/bike") as Texture);
		textures.Add(Resources.Load("Textures/car") as Texture);
		textures.Add(Resources.Load("Textures/truck") as Texture);
		textures.Add(Resources.Load("Textures/refinery") as Texture);
		textures.Add(Resources.Load("Textures/windmill") as Texture);
		textures.Add(Resources.Load("Textures/junkyard") as Texture);
	}

	void createGrid()
	{
		List<int> loadedMap = LoadGameFile();
		Vector2 gridSize = calcGridSize();
		GameObject hexGridGO = new GameObject("HexGrid");
		//board is used to store tile locations
		Dictionary<Point, Tile> board = new Dictionary<Point, Tile>();
		int landPos = 0;
		int tId = 1;
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
				var tb = (TileBehaviour)hex.GetComponent("TileBehaviour");
				tb.updatePositionOfTile(calcWorldCoord(gridPos));
				hex.transform.parent = hexGridGO.transform;

				int landTypeId = loadedMap [landPos];
				tb.tile = new Tile((int)x - (int)(y / 2), (int)y, TerrainType[landTypeId]);
				tb.setTileMaterial(tb.tile.landType);
				board.Add(tb.tile.Location, tb.tile);

				List<GameObject> stuffOnTile = new List<GameObject>();
				bool rand = true;
				switch(landTypeId){
				case 0:
					stuffOnTile.Add (createObject (tb, camp, tId));
					tId++;
					rand = false;
					break;
				case 1:
					int n = UnityEngine.Random.Range(3,6);
					for(int i=0;i<n;i++){
						stuffOnTile.Add((GameObject)Instantiate (tree));
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
					gameobjects.Add(createObject(tb,fanatic,1));
				}
				if (x == 0 && y == 1)
				{
					gameobjects.Add(createObject(tb,car,1));
				}
				if (x == 8 && y == 0)
				{
					gameobjects.Add(createObject (tb,fanatic,2));
				}
				if (x == 8 && y == 1)
				{
					gameobjects.Add(createObject (tb,truck,2));
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
	private void clearSelectionMenu(){
		foreach(GameObject sel in selectionMenu){
			RawImage img = (RawImage)sel.GetComponent<RawImage>();
			img.texture = null;
			((RawImage)sel.GetComponent<RawImage> ()).color = Color.clear;
		}
	}

	public void buildOnTile(GameObject selection){
		if(unitSelected.Count!=0){
			GameObject hexGrid = retrieveTile(unitSelected.First());
			if(hexGrid == null){
				Debug.Log ("No Tile Found");
				return;
			}
			int tId = unitSelected.First ().GetComponent<GOProperties> ().PlayerId;
			TileBehaviour tb = (TileBehaviour)hexGrid.GetComponent("TileBehaviour");
			switch (tb.getTile ().getLandType()) {
			case LandType.Base:
				if(selection.name.Equals("sel0")){
					gameobjects.Add(createObject (tb,fanatic,tId));
				}else if(selection.name.Equals("sel1")){
					gameobjects.Add(createObject (tb,bike,tId));
				}else if(selection.name.Equals("sel2")){
					gameobjects.Add(createObject (tb,car,tId));
				}else if(selection.name.Equals("sel3")){
					gameobjects.Add(createObject (tb,truck,tId));
				}
				break;
			case LandType.Oasis:
				if(selection.name.Equals("sel0")){
					createObject (tb,windmill,tId);
				}
				break;
			case LandType.OilField:
				if(selection.name.Equals("sel0")){
					createObject (tb,refinery,tId);
				}
				break;
			case LandType.Junkyard:
				if(selection.name.Equals("sel0")){
					createObject (tb,junkyard,tId);
				}
				break;
			default:
				break;
			}
		}
	}

	private void updateSelectionMenu(LandType type){
		clearSelectionMenu();
		switch(type){
		case LandType.Base:
			((RawImage)selectionMenu[0].GetComponent<RawImage>()).texture = textures[0];
			((RawImage)selectionMenu[0].GetComponent<RawImage> ()).color = Color.white;
			((RawImage)selectionMenu[1].GetComponent<RawImage>()).texture = textures[1];
			((RawImage)selectionMenu[1].GetComponent<RawImage> ()).color = Color.white;
			((RawImage)selectionMenu[2].GetComponent<RawImage>()).texture = textures[2];
			((RawImage)selectionMenu[2].GetComponent<RawImage> ()).color = Color.white;
			((RawImage)selectionMenu[3].GetComponent<RawImage>()).texture = textures[3];
			((RawImage)selectionMenu[3].GetComponent<RawImage> ()).color = Color.white;
			break;
		case LandType.Oasis:
			((RawImage)selectionMenu[0].GetComponent<RawImage>()).texture = textures[5];
			((RawImage)selectionMenu[0].GetComponent<RawImage> ()).color = Color.white;
			break;
		case LandType.OilField:
			((RawImage)selectionMenu[0].GetComponent<RawImage>()).texture = textures[4];
			((RawImage)selectionMenu[0].GetComponent<RawImage> ()).color = Color.white;
			break;
		case LandType.Junkyard:
			((RawImage)selectionMenu[0].GetComponent<RawImage>()).texture = textures[6];
			((RawImage)selectionMenu[0].GetComponent<RawImage> ()).color = Color.white;
			break;
		default:
			break;
		}
	}

	private GameObject retrieveTile(GameObject obj){
		RaycastHit hitInfo = new RaycastHit();
		int mask = 1<<LayerMask.NameToLayer ("grid");
		Vector3 start = obj.transform.position;
		start+=new Vector3(0f,0.2f,0f);
		Debug.DrawRay (start, Vector3.down, Color.green, 30f, false);
		if (Physics.Raycast (start, Vector3.down, out hitInfo, mask)) {
			return hitInfo.transform.gameObject;
		}
		return null;
	}

	private LandType retrieveTileOfObject(GameObject obj){
		GameObject hit = retrieveTile (obj);
		if(hit !=null){
			TileBehaviour tb = (TileBehaviour)hit.GetComponent("TileBehaviour");
			if(tb==null){
				Debug.Log (hit.name);
				return LandType.Desert;
			}
			return tb.getTile().landType;
		}
		return LandType.Desert;
	}

	private GameObject createObject(TileBehaviour tb,GameObject obj,int teamId){
		GameObject go = Instantiate (obj);
		go.transform.position = tb.getNextPosition ();
		GOProperties gop = (GOProperties) go.GetComponent (typeof(GOProperties));
		gop.setUId (this.getId());
		gop.setPId (teamId); 
		gop.type = obj.name.ToString ();
		gop.quantity = 1;
		originTileTB.Add(gop.UniqueID,tb);
		destTileTB.Add(gop.UniqueID,tb);
		ObjsPaths.Add(gop.UniqueID,new List<GameObject>());
		Debug.Log (gop.type);
		switch (gop.type)
		{
		case "ThirdPersonController":
			gop.setAV(1);
			gop.setDV(1);
			gop.setMV(1);
			break;
		case "Apo_Car_2015":
			gop.setAV(2);
			gop.setDV(2);
			gop.setMV(2);
			break;
		case "f_noladder":
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

	public void DestroyTilesPath(int id){

		//Destroy game objects which used to indicate the path
		ObjsPathsTiles.Remove(id);
	}
		
	public void generateAndShowPath()
	{	
		foreach(GameObject unit in GridManager.unitSelected) {
			if (unit!=null) {
				GOProperties gop = (GOProperties) unit.GetComponent (typeof(GOProperties));
				//Don't do anything if origin or destination is not defined yet
				if (originTileTB [gop.UniqueID] == null || destTileTB [gop.UniqueID] == null) {
					DrawPath (new List<Tile> (),gop.UniqueID);
					deSelect ();
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
