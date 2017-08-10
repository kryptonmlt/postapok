using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.Characters.ThirdPerson;

public class CharacterMovement: MonoBehaviour
{
	//speed in meters per second
	public float speed = 10F;
	public float rotationSpeed = 1F;
	//distance between character and tile position when we assume we reached it and start looking for the next. Explained in detail later on
	public static float MinNextTileDist = 2f;

	private MovingObject m_character;
	//position of the tile we are heading to
	Vector3 curTilePos;
	Tile curTile;
	List<Tile> path;
	Tile previousTile;
	public bool IsMoving { get; private set; }

	public int unitInterval = 0;

	Transform myTransform;
	private Vector3? closeToDest = null;
	bool waiting = false;

	void Awake ()
	{
		IsMoving = false;
	}

	void Start ()
	{
		m_character = this.GetComponent<MovingObject> ();
		//caching the transform for better performance
		myTransform = transform;
		//all the animations by default should loop
		//this.GetComponent<Animation>().wrapMode = WrapMode.Loop;
	}

	//gets tile position in world space
	Vector3 calcTilePos (Tile tile)
	{
		//y / 2 is added to convert coordinates from straight axis coordinate system to squiggly axis system
		Vector2 tileGridPos = new Vector2 (tile.X + tile.Y / 2, tile.Y);
		Vector3 tilePos = GridManager.instance.calcWorldCoord (tileGridPos);
		//y coordinate is disregarded
		tilePos.y = myTransform.position.y;
		return tilePos;
	}

	//method argument is a list of tiles we got from the path finding algorithm
	public void StartMoving (List<Tile> path)
	{
		if (path.Count == 0)
			return;
		//the first tile we need to reach is actually in the end of the list just before the one the character is currently on
		curTile = path [path.Count - 2];
		curTilePos = calcTilePos (curTile);
		IsMoving = true;
		this.path = path;
	}

	//Method used to switch destination and origin tiles after the destination is reached
	void switchOriginAndDestinationTiles ()
	{
		GOProperties gop = (GOProperties)this.GetComponent (typeof(GOProperties));
		GridManager GM = GridManager.instance;
		GM.DestroyPath (gop.UniqueID);
		GM.DestroyTilesPath (gop.UniqueID);
		GM.getOriginTileTB () [gop.UniqueID] = GM.destTileTB [gop.UniqueID];
		GM.destTileTB [gop.UniqueID] = null;
	}

	bool isEnemyOnTile(){
		Debug.Log ("checking isEnemyOnTile??");
		GridManager GM = GridManager.instance;
		GOProperties gop1 = (GOProperties)this.GetComponent (typeof(GOProperties));
		/*TileBehaviour tb = GM.board [previousTile.boardCoords];
		Debug.Log (previousTile.X +", "+  previousTile.Y +" - "+tb.tile.Location.X+" , "+tb.tile.Location.Y);
		foreach (GameObject o in tb.objsOnTile) {
			GOProperties gop2 = (GOProperties)o.GetComponent (typeof(GOProperties));
			if (gop1.PlayerId!=gop2.PlayerId) {
				return true;
			}
		}*/
		Collider[] collisions = Physics.OverlapSphere (transform.position,5f);
		foreach (Collider col in collisions) {
			if(col.gameObject.tag == "Unit"){
				GOProperties gop2 = (GOProperties)col.gameObject.GetComponent (typeof(GOProperties));
				if (gop1.PlayerId!=gop2.PlayerId) {
					return true;
				}
			}
		}
		return false;
	}

	void Update ()
	{
		GridManager GM = GridManager.instance;
		if (unitInterval >= GM.globalInterval) {
			m_character.Move (Vector3.zero, false, false);
			return;
		} else if(waiting){
			waiting = false;
			if(isEnemyOnTile()){
				Debug.Log ("ENEMY ON TILE!!");
				path = new List<Tile>() ;
				path.Add (previousTile);
			}
		}

		if (!IsMoving) {
			m_character.Move (Vector3.zero, false, false);
			return;
		}
		if (closeToDest == null) {
			GOProperties gop = (GOProperties)this.GetComponent (typeof(GOProperties));
			closeToDest = GM.destTileTB [gop.UniqueID].getNextPosition (this.gameObject);
		}
		if (path.IndexOf (curTile) == 0) {
			curTilePos = closeToDest.Value;
			curTilePos.y = myTransform.position.y;
		}

		//if the distance between the character and the center of the next tile is short enough
		if ((curTilePos - myTransform.position).sqrMagnitude < MinNextTileDist * MinNextTileDist) {
			unitInterval++;
			waiting = true;
			//set custom destinitation
			//if we reached the destination tile
			if (path.IndexOf (curTile) == 0) {
				IsMoving = false; 
				unitInterval = 0;
				switchOriginAndDestinationTiles ();
				closeToDest = null;
				previousTile = curTile;
				return;
			}
			//curTile becomes the next one
			previousTile=curTile;
			curTile = path [path.IndexOf (curTile) - 1];
			curTilePos = calcTilePos (curTile);
		}
		MoveTowards (curTilePos);
	}

	void MoveTowards (Vector3 position)
	{
		//mevement direction
		Vector3 dir = position - myTransform.position;

		// Rotate towards the target
		myTransform.rotation = Quaternion.Slerp (myTransform.rotation,
			Quaternion.LookRotation (dir), rotationSpeed * Time.deltaTime);

		Vector3 forwardDir = myTransform.forward;
		forwardDir = forwardDir * speed;
		float speedModifier = Vector3.Dot (dir.normalized, myTransform.forward);
		forwardDir *= speedModifier;
		if (speedModifier > 0.95f) {
			//controller.SimpleMove(forwardDir);
			m_character.Move (forwardDir, false, false);
		}
	}
}