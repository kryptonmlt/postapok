using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethods
{
	
	private static Material sandMaterial = Resources.Load ("Materials/SandMaterial", typeof(Material)) as Material;
	private static Material OilMaterial = Resources.Load ("Materials/OilMaterial", typeof(Material)) as Material;
	private static Material OasisMaterial = Resources.Load ("Materials/OasisMaterial", typeof(Material)) as Material;
	private static Material JunkMaterial = Resources.Load ("Materials/JunkMaterial", typeof(Material)) as Material;

	public static Material getMaterial (this LandType type)
	{
		switch (type) {
		case LandType.Desert:
			return sandMaterial;
		case LandType.Oasis:
			return OasisMaterial;
		case LandType.OilField:
			return OilMaterial;
		case LandType.Junkyard:
			return JunkMaterial;
		case LandType.Base:
			return sandMaterial;
		case LandType.Mountain:
			return sandMaterial;
		default:
			return null;
		}
	}
}
