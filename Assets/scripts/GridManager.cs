using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Random=UnityEngine.Random;

public class GridManager: MonoBehaviour
{
	public GameObject Hex;
	//This time instead of specifying the number of hexes you should just drop your ground game object on this public variable
	public GameObject Ground;

	public GameObject player;

	//selectedTile stores the tile mouse cursor is hovering on
	public Tile selectedTile = null;
	//TB of the tile which is the start of the path
	public Dictionary<String, TileBehaviour> originTileTB = new Dictionary<String, TileBehaviour>();

	public Dictionary<String, TileBehaviour> getOriginTileTB(){
		return originTileTB;
	}

	//public TileBehaviour originTileTB = null;
	//TB of the tile which is the end of the path
	public TileBehaviour destTileTB = null;
	public TileBehaviour tb =null;

	public static GridManager instance = null;

	//Line should be initialised to some 3d object that can fit nicely in the center of a hex tile and will be used to indicate the path. For example, it can be just a simple small sphere with some material attached to it. Initialise the variable using inspector pane.
	public GameObject Line;
	//List to hold "Lines" indicating the path
	List<GameObject> path;

	private float hexWidth;
	private float hexHeight;
	private float groundWidth;
	private float groundHeight;

	public static GameObject unitSelected=null;

	Dictionary<int, LandType> TerrainType = new Dictionary<int, LandType>()
	{
		{1,LandType.Oasis},
		{2,LandType.Junkyard},
		{3,LandType.OilField},
		{4,LandType.Desert}
	};

	void setSizes()
	{
		hexWidth = Hex.GetComponent<Renderer>().bounds.size.x;
		hexHeight = Hex.GetComponent<Renderer>().bounds.size.z;
		groundWidth = Ground.GetComponent<Renderer>().bounds.size.x;
		groundHeight = Ground.GetComponent<Renderer>().bounds.size.z;
	}

	void Update(){

		if (Input.GetMouseButtonDown(0))
		{
			Debug.Log("Mouse is down");

			RaycastHit hitInfo = new RaycastHit();
			bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);
			if (hit) 
			{
				Debug.Log("Hit " + hitInfo.transform.gameObject.name);
				if (hitInfo.transform.gameObject.tag == "Unit")
				{	
					if (GridManager.unitSelected == null) {
						GridManager.unitSelected = hitInfo.transform.gameObject;
						Debug.Log ("Unit Selected");
					} else if (GridManager.unitSelected == hitInfo.transform.gameObject) {
						GridManager.unitSelected = null;
						Debug.Log ("Unit Deselected");
					}
						
				} else {
					Debug.Log ("Not a Unit");
				}
			} else {
				Debug.Log("No hit");
			}
			Debug.Log("Mouse is down");
		} 
	}

	//The method used to calculate the number hexagons in a row and number of rows
	//Vector2.x is gridWidthInHexes and Vector2.y is gridHeightInHexes
	Vector2 calcGridSize()
	{
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
		return new Vector3(x, 0, z);
	}

	void Start()
	{
		setSizes();
		createGrid();
		generateAndShowPath();
	}

	void Awake()
	{
		instance = this;
	}

	void createGrid()
	{
		Vector2 gridSize = calcGridSize();
		GameObject hexGridGO = new GameObject("HexGrid");
		//board is used to store tile locations
		Dictionary<Point, Tile> board = new Dictionary<Point, Tile>();

		for (float y = 0; y < gridSize.y; y++)
		{
			float sizeX = gridSize.x;
			//if the offset row sticks up, reduce the number of hexes in a row
			if (y % 2 != 0 && (gridSize.x + 0.5) * hexWidth > groundWidth)
				sizeX--;
			for (float x = 0; x < sizeX; x++)
			{
				GameObject hex = (GameObject)Instantiate(Hex);
				Vector2 gridPos = new Vector2(x, y);
				hex.transform.position = calcWorldCoord(gridPos);
				hex.transform.parent = hexGridGO.transform;
				var tb = (TileBehaviour)hex.GetComponent("TileBehaviour");
				//y / 2 is subtracted from x because we are using straight axis coordinate system
				int tTypeId = UnityEngine.Random.Range(1,15);
				if (tTypeId > 3)
					tTypeId = 4;
				tb.tile = new Tile((int)x - (int)(y / 2), (int)y, TerrainType[tTypeId]);
				tb.changeColor(tb.tile.tilecolor);
				board.Add(tb.tile.Location, tb.tile);
				//Mark originTile as the tile with (0,0) coordinates
				if (x == 0 && y == 0)
				{
					tb.GetComponent<Renderer>().material = tb.OpaqueMaterial;
					Color red = Color.red;
					red.a = 158f / 255f;
					tb.GetComponent<Renderer>().material.color = red;
					GameObject player1 = Instantiate (player);
					player1.name = "player1";
					originTileTB.Add(player1.name,tb);
					player1.transform.position = tb.transform.position;
				}
				if (x == 2 && y == 3)
				{
					tb.GetComponent<Renderer>().material = tb.OpaqueMaterial;
					Color red = Color.red;
					red.a = 158f / 255f;
					tb.GetComponent<Renderer>().material.color = red;
					//originTileTB = tb;
					GameObject player2 = Instantiate (player);
					player2.name = "player2";
					originTileTB.Add(player2.name,tb);
					player2.transform.position = tb.transform.position;
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

	//Distance between destination tile and some other tile in the grid
	double calcDistance(Tile tile)
	{
		Tile destTile = destTileTB.tile;
		//Formula used here can be found in Chris Schetter's article
		float deltaX = Mathf.Abs(destTile.X - tile.X);
		float deltaY = Mathf.Abs(destTile.Y - tile.Y);
		int z1 = -(tile.X + tile.Y);
		int z2 = -(destTile.X + destTile.Y);
		float deltaZ = Mathf.Abs(z2 - z1);

		return Mathf.Max(deltaX, deltaY, deltaZ);
	}

	private void DrawPath(IEnumerable<Tile> path)
	{
		if (this.path == null)
			this.path = new List<GameObject>();
		//Destroy game objects which used to indicate the path
		this.path.ForEach(Destroy);
		this.path.Clear();

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
			this.path.Add(line);
			line.transform.parent = lines.transform;
		}
	}

	public void generateAndShowPath()
	{
		if (GridManager.unitSelected != null) {
			//Don't do anything if origin or destination is not defined yet
			if (originTileTB [GridManager.unitSelected.name] == null || destTileTB == null) {
				DrawPath (new List<Tile> ());
				return;
			}

			var path = PathFinder.FindPath (originTileTB [GridManager.unitSelected.name].tile, destTileTB.tile);
			DrawPath (path);
			CharacterMovement characterAction = (CharacterMovement)GridManager.unitSelected.GetComponent (typeof(CharacterMovement));
			characterAction.StartMoving (path.ToList ());
		}
	}
}