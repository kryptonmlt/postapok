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
	Color orange = new Color(255f / 255f, 127f / 255f, 0, 127f/255f);

	public int itemsOnTile =0;
	public List<Vector3> separatePositions = new List<Vector3>();

	private static float outerRadius =2.5f;
	private static float innerRadius =1.5f;
	private static Vector3[] corners = {
		new Vector3(0f, 0f, outerRadius),
		new Vector3(innerRadius, 0f, 0.5f * outerRadius),
		new Vector3(innerRadius, 0f, -0.5f * outerRadius),
		new Vector3(0f, 0f, -outerRadius),
		new Vector3(-innerRadius, 0f, -0.5f * outerRadius),
		new Vector3(-innerRadius, 0f, 0.5f * outerRadius)
	};
	public List<GameObject> objsOnTile = new List<GameObject> ();
	public List<int> objPosition = new List<int>();

	public bool built = false;

	public void Builded(){
		built = true;
	}

	public void updatePositionOfTile(Vector3 position){
		separatePositions.Clear();
		transform.position = position;
		foreach(Vector3 v in corners){
			Vector3 updated = v + position;
			separatePositions.Add(updated);
		}
	}

	public Vector3 getNextPosition(GameObject obj){
		GOProperties gop = (GOProperties)obj.GetComponent (typeof(GOProperties));
		int index = objectTypeExists (gop.type);
		if(index!=-1){
			return separatePositions[objPosition [index]];
		}else{
			objsOnTile.Add (obj);
			objPosition.Add (itemsOnTile);
			Vector3 result = separatePositions[itemsOnTile];
			itemsOnTile++;
			return result;
		}
	}

	public void removeObjectFromTile(int gId){
		int pos = -1;
		for(int i=0;i<objsOnTile.Count;i++){
			GOProperties gop = (GOProperties)objsOnTile[i].GetComponent (typeof(GOProperties));

			if(gop.UniqueID==gId){
				pos = i;
				break;
			}
		}
		if(pos!=-1){
			itemsOnTile--;
			objsOnTile.RemoveAt (pos);
			objPosition.RemoveAt (pos);
		}
	}

	public int objectTypeExists(string type){
		for(int i=0;i<objsOnTile.Count;i++){
			GOProperties gop = (GOProperties)objsOnTile[i].GetComponent (typeof(GOProperties));
			if(gop.type==type){
				return i;
			}
		}
		return -1;
	}

	public void changeColor(Color color)
	{
		GetComponent<Renderer> ().material.color = color;
	}

	public void setTileMaterial(LandType type)
	{
		GetComponent<Renderer> ().material = type.getMaterial();
	}

	public Tile getTile(){
		return tile;
	}

	//IMPORTANT: for methods like OnMouseEnter, OnMouseExit and so on to work, collider (Component -> Physics -> Mesh Collider) should be attached to the prefab
	void OnMouseEnter()
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

	public void decolour(){
		changeColor (Color.white);
	}

	//changes back to fully transparent material when mouse cursor is no longer hovering over the tile
	void OnMouseExit()
	{
		foreach (GameObject unit in GridManager.unitSelected) {
			if (unit != null) {
				GOProperties gop = (GOProperties) unit.GetComponent (typeof(GOProperties));
				GridManager.instance.selectedTile = null;
				if (tile.Passable && this != GridManager.instance.destTileTB[gop.UniqueID]
					&& this != GridManager.instance.getOriginTileTB () [gop.UniqueID]) {
					changeColor (Color.white);
				}
			}
		}
	}
	//called every frame when mouse cursor is on this tile
	void OnMouseOver()
	{
		if(!EventSystem.current.IsPointerOverGameObject()){
			foreach (GameObject unit in GridManager.unitSelected) {
				if (unit != null) {
					GOProperties gop = (GOProperties) unit.GetComponent (typeof(GOProperties));
					//if player right-clicks on the tile, toggle passable variable and change the color accordingly
					//		if (Input.GetMouseButtonUp(1))
					//		{		
					//		}
					//if user left-clicks the tile
					bool moving = GridManager.instance.isAnyMoving();
					if (Input.GetMouseButtonUp (0) & unit != null & moving == false) {
						if (tile.Passable) {
							changeColor (Color.white);
							TileBehaviour originTileTB = GridManager.instance.getOriginTileTB () [gop.UniqueID];
							//if user clicks on origin tile or origin tile is not assigned yet
							if (this == originTileTB || originTileTB == null) {
								originTileChanged ();
							}else {
								destTileChanged ();
							}
							GridManager.instance.generateAndShowPath ();
						}
					} 
				}
			}
		}
	}

	void Update(){

		if (Input.GetKeyDown (KeyCode.Escape)) {
			changeColor (Color.white);
		}
	}

	void originTileChanged()
	{	
		foreach (GameObject unit in GridManager.unitSelected) {
			

		}
	}

	void destTileChanged()
	{
		foreach (GameObject unit in GridManager.unitSelected) {

			if (unit != null) {
				GOProperties gop = (GOProperties) unit.GetComponent (typeof(GOProperties));
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