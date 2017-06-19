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
	public bool split=false;

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
	public void setSplit(bool s){
		split = s;
	}

}


