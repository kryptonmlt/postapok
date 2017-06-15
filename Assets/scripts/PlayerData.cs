using System;

public class PlayerData
{
	public int water{ get; set; }
	public int petrol{ get; set; }
	public int scrap{ get; set; }

	public PlayerData (int w,int p, int s)
	{
		water = w;
		petrol = p;
		scrap = s;
	}
}

