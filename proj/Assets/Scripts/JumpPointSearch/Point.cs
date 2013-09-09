using UnityEngine;
using System.Collections;

namespace JumpPointSearach
{
	public class Point
	{
	
		private int mX;
		private int mY;
	
		public int X {
			get { return this.mX;}
			set { this.mX = value;}
		}
	
		public int Y {
			get { return this.mY;}
			set { this.mY = value;}
		}
	
		public Point (int x, int y)
		{
			this.mX = x;
			this.mY = y;
		}
	}
}

