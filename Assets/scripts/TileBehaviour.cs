using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileBehaviour: MonoBehaviour
{
	public Tile tile;
	//After attaching this script to hex tile prefab don't forget to initialize following materials with the ones we created earlier
	public Material OpaqueMaterial;
	public Material defaultMaterial;
	//Slightly transparent orange
	Color orange = new Color(255f / 255f, 127f / 255f, 0, 127f/255f);
	Color temptilecolour;

	public void changeColor(Color color)
	{
		//If transparency is not set already, set it to default value
		if (color.a == 1)
			color.a = 130f / 255f;
		GetComponent<Renderer>().material = OpaqueMaterial;
		GetComponent<Renderer>().material.color = color;
	}

	//IMPORTANT: for methods like OnMouseEnter, OnMouseExit and so on to work, collider (Component -> Physics -> Mesh Collider) should be attached to the prefab
	void OnMouseEnter()
	{
		GridManager.instance.selectedTile = tile;
		temptilecolour = tile.tilecolor;
		//when mouse is over some tile, the tile is passable and the current tile is neither destination nor origin tile, change color to orange
		if (tile.Passable && this != GridManager.instance.destTileTB
			&& this != GridManager.instance.originTileTB)
		{
			changeColor(orange);
		}
	}

	//changes back to fully transparent material when mouse cursor is no longer hovering over the tile
	void OnMouseExit()
	{
		GridManager.instance.selectedTile = null;
		if (tile.Passable && this != GridManager.instance.destTileTB
			&& this != GridManager.instance.originTileTB)
		{
			changeColor (temptilecolour);
		}
	}
	//called every frame when mouse cursor is on this tile
	void OnMouseOver()
	{
		//if player right-clicks on the tile, toggle passable variable and change the color accordingly
		if (Input.GetMouseButtonUp(1))
		{
			if (this == GridManager.instance.destTileTB ||
				this == GridManager.instance.originTileTB)
				return;
			tile.Passable = !tile.Passable;
			if (!tile.Passable)
				changeColor(Color.gray);
			else
				changeColor(orange);

			GridManager.instance.generateAndShowPath();
		}
		//if user left-clicks the tile
		if (Input.GetMouseButtonUp(0))
		{
			tile.Passable = true;

			TileBehaviour originTileTB = GridManager.instance.originTileTB;
			//if user clicks on origin tile or origin tile is not assigned yet
			if (this == originTileTB || originTileTB == null)
				originTileChanged();
			else
				destTileChanged();

			GridManager.instance.generateAndShowPath();
		}
	}

	void originTileChanged()
	{
		var originTileTB = GridManager.instance.originTileTB;
		//deselect origin tile if user clicks on current origin tile
		if (this == originTileTB)
		{
			GridManager.instance.originTileTB = null;
			GetComponent<Renderer>().material = defaultMaterial;
			return;
		}
		//if origin tile is not specified already mark this tile as origin
		GridManager.instance.originTileTB = this;
		changeColor(Color.green);
	}

	void destTileChanged()
	{
		var destTile = GridManager.instance.destTileTB;
		//deselect destination tile if user clicks on current destination tile
		if (this == destTile)
		{
			GridManager.instance.destTileTB = null;
			GetComponent<Renderer>().material.color = orange;
			return;
		}
		//if there was other tile marked as destination, change its material to default (fully transparent) one
		if (destTile != null)
			destTile.GetComponent<Renderer>().material = defaultMaterial;
		GridManager.instance.destTileTB = this;
		changeColor(Color.green);
	}
}