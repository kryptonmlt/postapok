using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.Characters.ThirdPerson;

public class CharacterMovement: MonoBehaviour
{
	//speed in meters per second
	public float speed = 0.0025F;
	public float rotationSpeed = 0.004F;
	//distance between character and tile position when we assume we reached it and start looking for the next. Explained in detail later on
	public static float MinNextTileDist = 0.07f;

	private ThirdPersonCharacter m_character;
	//position of the tile we are heading to
	Vector3 curTilePos;
	Tile curTile;
	List<Tile> path;
	public bool IsMoving { get; private set; }
	Transform myTransform;

	void Awake()
	{
		IsMoving = false;
	}

	void Start()
	{
		m_character = this.GetComponent<ThirdPersonCharacter>();
		//caching the transform for better performance
		myTransform = transform;
		//all the animations by default should loop
		//this.GetComponent<Animation>().wrapMode = WrapMode.Loop;
	}

	//gets tile position in world space
	Vector3 calcTilePos(Tile tile)
	{
		//y / 2 is added to convert coordinates from straight axis coordinate system to squiggly axis system
		Vector2 tileGridPos = new Vector2(tile.X + tile.Y / 2, tile.Y);
		Vector3 tilePos = GridManager.instance.calcWorldCoord(tileGridPos);
		//y coordinate is disregarded
		tilePos.y = myTransform.position.y;
		return tilePos;
	}

	//method argument is a list of tiles we got from the path finding algorithm
	public void StartMoving(List<Tile> path)
	{
		if (path.Count == 0)
			return;
		//the first tile we need to reach is actually in the end of the list just before the one the character is currently on
		curTile = path[path.Count - 2];
		curTilePos = calcTilePos(curTile);
		IsMoving = true;
		this.path = path;
	}

	//Method used to switch destination and origin tiles after the destination is reached
	void switchOriginAndDestinationTiles()
	{
		GridManager GM = GridManager.instance;
		Material originMaterial = GM.getOriginTileTB()[m_character.name].GetComponent<Renderer>().material;
		GM.getOriginTileTB()[m_character.name].GetComponent<Renderer>().material = GM.destTileTB.defaultMaterial;
		GM.getOriginTileTB()[m_character.name] = GM.destTileTB;
		GM.getOriginTileTB()[m_character.name].GetComponent<Renderer>().material = originMaterial;
		GM.destTileTB = null;
		GM.generateAndShowPath();
	}

	void Update()
	{
		if (!IsMoving) {
			m_character.Move (Vector3.zero, false, false);
			return;
		}
		//if the distance between the character and the center of the next tile is short enough
		if ((curTilePos - myTransform.position).sqrMagnitude < MinNextTileDist * MinNextTileDist)
		{
			//if we reached the destination tile
			if (path.IndexOf(curTile) == 0)
			{
				IsMoving = false;
				switchOriginAndDestinationTiles();
				return;
			}
			//curTile becomes the next one
			curTile = path[path.IndexOf(curTile) - 1];
			curTilePos = calcTilePos(curTile);
		}

		MoveTowards(curTilePos);
	}

	void MoveTowards(Vector3 position)
	{
		//mevement direction
		Vector3 dir = position - myTransform.position;

		// Rotate towards the target
		myTransform.rotation = Quaternion.Slerp(myTransform.rotation,
			Quaternion.LookRotation(dir), rotationSpeed * Time.deltaTime);

		Vector3 forwardDir = myTransform.forward;
		forwardDir = forwardDir * speed;
		float speedModifier = Vector3.Dot(dir.normalized, myTransform.forward);
		forwardDir *= speedModifier;
		if (speedModifier > 0.95f)
		{
			//controller.SimpleMove(forwardDir);
			m_character.Move(forwardDir,false,false);
		}
	}
}