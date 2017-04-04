﻿using UnityEngine;
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

	public void changeColor(Color color)
	{
		GetComponent<Renderer> ().material.color = color;
	}

	public void setTileMaterial(LandType type)
	{
		GetComponent<Renderer> ().material = type.getMaterial();
	}

	//IMPORTANT: for methods like OnMouseEnter, OnMouseExit and so on to work, collider (Component -> Physics -> Mesh Collider) should be attached to the prefab
	void OnMouseEnter()
	{
		foreach (GameObject unit in GridManager.unitSelected) {
			
			if (unit != null) {
				GridManager.instance.selectedTile = tile;
				//when mouse is over some tile, the tile is passable and the current tile is neither destination nor origin tile, change color to orange
				if (tile.Passable && this != GridManager.instance.destTileTB
					&& this != GridManager.instance.getOriginTileTB () [unit.name]) {
					changeColor (orange);
				}
			}
		}
	}

	//changes back to fully transparent material when mouse cursor is no longer hovering over the tile
	void OnMouseExit()
	{
		foreach (GameObject unit in GridManager.unitSelected) {
			
			if (unit != null) {
				GridManager.instance.selectedTile = null;
				if (tile.Passable && this != GridManager.instance.destTileTB
				   && this != GridManager.instance.getOriginTileTB () [unit.name]) {
					changeColor (Color.white);
				}
			}
		}
	}
	//called every frame when mouse cursor is on this tile
	void OnMouseOver()
	{
		foreach (GameObject unit in GridManager.unitSelected) {
			
			if (unit != null) {
				
				//if player right-clicks on the tile, toggle passable variable and change the color accordingly
				//		if (Input.GetMouseButtonUp(1))
				//		{		
				//		}
				//if user left-clicks the tile
				bool moving = false;
				foreach (GameObject active in GameObject.FindGameObjectsWithTag("Unit")) {
					if (active != null) {
						CharacterMovement characterAction = (CharacterMovement)active.GetComponent (typeof(CharacterMovement));
						if (characterAction.IsMoving == true) {
							moving = characterAction.IsMoving;
						}
					}
				}
				if (Input.GetMouseButtonUp (0) & unit != null & moving == false) {
					tile.Passable = true;
					changeColor (Color.white);
					TileBehaviour originTileTB = GridManager.instance.getOriginTileTB () [unit.name];
					//if user clicks on origin tile or origin tile is not assigned yet
					if (this == originTileTB || originTileTB == null)
						originTileChanged ();
					else
						destTileChanged ();

					GridManager.instance.generateAndShowPath();
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
		var destTile = GridManager.instance.destTileTB;
		//deselect destination tile if user clicks on current destination tile
		//if (this == destTile)
		//{
			//GridManager.instance.destTileTB = null;
			//GetComponent<Renderer>().material.color = temptilecolour;
			//return;
		//}
		//if there was other tile marked as destination, change its material to default (fully transparent) one
		if (destTile != null)
			destTile.GetComponent<Renderer>().material = defaultMaterial;
		GridManager.instance.destTileTB = this;
		//changeColor(Color.green);
	}
}