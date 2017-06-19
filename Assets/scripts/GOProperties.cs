using System;
using UnityEngine;

public class GOProperties: MonoBehaviour
{

	public int AttackValue;
	public int DefenseValue;
	public int PlayerId;
	public int MovementValue;
	public int UniqueID;
	public int quantity;
	public String type;
	public bool shown { get; set; }
	public bool[] structureShown { get; set; }

	public void initStructureShown(int playerId, int size){
		structureShown = new bool[size];
		for(int i=0;i<size;i++){
			structureShown[i] = playerId == i;
		}
	}

	public void setUId(int id){
		UniqueID=id;
	}
		
	public void setPId(int id){
		PlayerId=id;
	}
	public void setAV(int a){
		AttackValue=a;
	}
	public void setDV(int d){
		DefenseValue=d;
	}
	public void setMV(int m){
		MovementValue=m;
	}

}


