using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public class Tile: GridObject, IHasNeighbours<Tile>
{
	public bool Passable;

	public Tile(int x, int y)
		: base(x, y)
	{
		Passable = true;
	}

	public IEnumerable<Tile> AllNeighbours { get; set; }
	public IEnumerable<Tile> Neighbours
	{
		get { return AllNeighbours.Where(o => o.Passable); }
	}

	public void FindNeighbours(Dictionary<Point, Tile> Board,
		Vector2 BoardSize, bool EqualLineLengths)
	{
		List<Tile> neighbours = new List<Tile>();

		foreach (Point point in NeighbourShift)
		{
			int neighbourX = X + point.X;
			int neighbourY = Y + point.Y;
			//x coordinate offset specific to straight axis coordinates
			int xOffset = neighbourY / 2;

			//If every second hexagon row has less hexagons than the first one, just skip the last one when we come to it
			if (neighbourY % 2 != 0 && !EqualLineLengths &&
				neighbourX + xOffset == BoardSize.x - 1)
				continue;
			//Check to determine if currently processed coordinate is still inside the board limits
			if (neighbourX >= 0 - xOffset &&
				neighbourX < (int)BoardSize.x - xOffset &&
				neighbourY >= 0 && neighbourY < (int)BoardSize.y)
				neighbours.Add(Board[new Point(neighbourX, neighbourY)]);
		}

		AllNeighbours = neighbours;
	}

	public static List<Point> NeighbourShift
	{
		get
		{
			return new List<Point>
			{
				new Point(0, 1),
				new Point(1, 0),
				new Point(1, -1),
				new Point(0, -1),
				new Point(-1, 0),
				new Point(-1, 1),
			};
		}
	}
}