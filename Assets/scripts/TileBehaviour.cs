using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class TileBehaviour: MonoBehaviour
{
	public Tile tile;
	//After attaching this script to hex tile prefab don't forget to initialize following materials with the ones we created earlier
	public Material OpaqueMaterial;
	public Material defaultMaterial;
	//Slightly transparent orange
	Color orange = new Color (255f / 255f, 127f / 255f, 0, 127f / 255f);

	public List<Vector3> separatePositions = new List<Vector3> ();
	private static float radius = 2.5f;
	private int maxSpaces = 6;
	private int incrementAmount = 1;

	public List<GameObject> objsOnTile = new List<GameObject> ();
	public Dictionary<int,int> objPos = new Dictionary<int,int> ();

	public bool built{ get; set; }

	public bool upgraded{ get; set; }

	public int getPlayerOwner ()
	{
		if (objsOnTile.Count > 0) {
			GOProperties gop = (GOProperties)objsOnTile [0].GetComponent (typeof(GOProperties));
			return gop.PlayerId;
		}
		return -1;
	}

	public int getFanaticsOnTile ()
	{
		int f = 0;
		foreach (GameObject objOnTile in objsOnTile) {
			GOProperties gop = (GOProperties)objOnTile.GetComponent (typeof(GOProperties));
			if (gop.type.Equals ("ThirdPersonController")) {
				f += gop.Quantity;
			}
		}
		return f;
	}

	public void updatePositionOfTile (Vector3 position)
	{
		transform.position = position;
		createSeparatePositions ();
	}

	public Vector3 getNextPosition (GameObject obj)
	{
		return getNextPosition (obj, true);
	}

	public Vector3 getNextPosition (GameObject obj, bool join)
	{
		GOProperties gop = (GOProperties)obj.GetComponent (typeof(GOProperties));
		int index = objectTypeExists (gop.type);
		printTileText ("Adding", gop);
		if (index != -1 && join) {
			printTileText ("Already represented", gop);
			return separatePositions [objPos [index]];
		} else {
			objPos.Add (gop.UniqueID, getFirstAvailablePos ());
			objsOnTile.Add (obj);
			printTileText ("New position " + objPos [gop.UniqueID], gop);
			if (objsOnTile.Count == maxSpaces - 1) {
				//increase number of spaces on tile
				maxSpaces += incrementAmount;
				createSeparatePositions ();
				foreach (GameObject o in objsOnTile) {
					GOProperties gopTemp = (GOProperties)o.GetComponent (typeof(GOProperties));
					o.transform.position = separatePositions [objPos [gopTemp.UniqueID]] + new Vector3 (0f, 0.3f, 0f); 
				}
			}
			Vector3 result = separatePositions [objPos [gop.UniqueID]];
			return result;
		}
	}

	private bool isPosUsed (int pos)
	{
		foreach (int guid in objPos.Keys) {
			if (pos == objPos [guid]) {
				return true;
			}
		}
		return false;
	}

	private int getFirstAvailablePos ()
	{
		for (int i = 0; i < maxSpaces; i++) {
			if (!isPosUsed (i)) {
				return i;
			}
		}
		return -1;
	}

	private void createSeparatePositions ()
	{
		float ang = 360 / maxSpaces;
		separatePositions.Clear ();
		for (float inc = 0; inc < 360; inc += ang) {
			Vector3 pos;
			pos.x = this.transform.position.x + radius * Mathf.Sin (inc * Mathf.Deg2Rad);
			pos.y = this.transform.position.y;
			pos.z = this.transform.position.z + radius * Mathf.Cos (inc * Mathf.Deg2Rad);
			separatePositions.Add (pos);
		}
	}

	public void removeObjectFromTile (int uniqueId)
	{
		GOProperties gop = (GOProperties)GridManager.instance.gameobjects [uniqueId].GetComponent (typeof(GOProperties));
		printTileText ("Deleting", gop);
		if (objPos.ContainsKey (uniqueId)) {
			int posToDelete = objPos [uniqueId];
			objsOnTile.RemoveAt (posToDelete);
			objPos.Remove (uniqueId);
			//sync position of other objects after deletion
			List<int> keys = new List<int> (objPos.Keys);
			foreach(int key in keys){
				if(posToDelete <objPos[key]){
					objPos [key] -= 1;
				}
			}
		} else {
			printTileText ("Failed to delete properly", gop);
		}
	}

	private void printTileText(string text, GOProperties gop){
		Debug.Log (text+" id " + gop.UniqueID + "("+gop.type+", team "+gop.PlayerId+") on tile (" + this.tile.boardCoords.X + "," + this.tile.boardCoords.Y+ ")");
	}

	public int objectTypeExists (string type)
	{
		
		foreach (GameObject objOnTile in objsOnTile) {
			GOProperties gop = (GOProperties)objOnTile.GetComponent (typeof(GOProperties));
			if (gop.type == type) {
				return gop.UniqueID;
			}
		}
		return -1;
	}

	public void changeColor (Color color)
	{
		GetComponent<Renderer> ().material.color = color;
	}

	public void setTileMaterial (LandType type)
	{
		GetComponent<Renderer> ().material = type.getMaterial ();
	}

	public Tile getTile ()
	{
		return tile;
	}

	//IMPORTANT: for methods like OnMouseEnter, OnMouseExit and so on to work, collider (Component -> Physics -> Mesh Collider) should be attached to the prefab
	void OnMouseEnter ()
	{
		if (!EventSystem.current.IsPointerOverGameObject ()) {
			foreach (GameObject unit in GridManager.unitSelected) {
				if (unit != null) {
					GOProperties gop = (GOProperties)unit.GetComponent (typeof(GOProperties));
					GridManager.instance.selectedTile = tile;
					//when mouse is over some tile, the tile is passable and the current tile is neither destination nor origin tile, change color to orange
					if (tile.Passable && this != GridManager.instance.destTileTB [gop.UniqueID]
					    && this != GridManager.instance.getOriginTileTB () [gop.UniqueID]) {
						changeColor (orange);
					}
				}
			}
		}
	}

	public void decolour ()
	{
		changeColor (Color.white);
	}

	public void highlightMovementPossible ()
	{		
		if (GetComponent<Renderer> ().material.color.Equals (Color.white)) {			
			changeColor (Color.grey);
		}
	}

	//changes back to fully transparent material when mouse cursor is no longer hovering over the tile
	void OnMouseExit ()
	{
		foreach (GameObject unit in GridManager.unitSelected) {
			if (unit != null) {
				GOProperties gop = (GOProperties)unit.GetComponent (typeof(GOProperties));
				GridManager.instance.selectedTile = null;
				if (tile.Passable && this != GridManager.instance.destTileTB [gop.UniqueID]
				    && this != GridManager.instance.getOriginTileTB () [gop.UniqueID]) {
					changeColor (Color.white);
				}
			}
		}
	}
	//called every frame when mouse cursor is on this tile
	void OnMouseOver ()
	{
		if (!EventSystem.current.IsPointerOverGameObject ()) {
			foreach (GameObject unit in GridManager.unitSelected) {
				if (unit != null) {
					GOProperties gop = (GOProperties)unit.GetComponent (typeof(GOProperties));
					//if player right-clicks on the tile, toggle passable variable and change the color accordingly
					//		if (Input.GetMouseButtonUp(1))
					//		{		
					//		}
					//if user left-clicks the tile
					bool moving = GridManager.instance.isAnyMoving ();
					if (Input.GetMouseButtonUp (0) & unit != null & moving == false) {
						if (tile.Passable) {
							changeColor (Color.white);
							TileBehaviour originTileTB = GridManager.instance.getOriginTileTB () [gop.UniqueID];
							//if user clicks on origin tile or origin tile is not assigned yet
							if (this == originTileTB || originTileTB == null) {
								originTileChanged ();
							} else {
								destTileChanged ();
							}
							GridManager.instance.generateAndShowPath ();
						}
					} 
				}
			}
		}
	}

	void Update ()
	{

		if (Input.GetKeyDown (KeyCode.Escape)) {
			changeColor (Color.white);
		}
	}

	void originTileChanged ()
	{	
		foreach (GameObject unit in GridManager.unitSelected) {
			

		}
	}

	void destTileChanged ()
	{
		foreach (GameObject unit in GridManager.unitSelected) {

			if (unit != null) {
				GOProperties gop = (GOProperties)unit.GetComponent (typeof(GOProperties));
				//var destTile = GridManager.instance.destTileTB;
				//deselect destination tile if user clicks on current destination tile
				//if (this == destTile)
				//{
				//GridManager.instance.destTileTB = null;
				//GetComponent<Renderer>().material.color = temptilecolour;
				//return;
				//}
				//if there was other tile marked as destination, change its material to default (fully transparent) one
				//if (destTile != null)
				//	destTile.GetComponent<Renderer>().material = defaultMaterial;
				GridManager.instance.destTileTB [gop.UniqueID] = this;
				//changeColor(Color.green);
			}
		}
	}
}