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

	public GOProperties ()
	{
		quantity = 1;
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


