using UnityEngine;
using System.Collections;

namespace JumpPointSearach
{
	/// <summary>
	/// Point class. 
	/// Author: Radosław Bigaj
	/// POLITECHNIKA ŚLĄSKA 
	/// </summary>
	public class Point
	{
		/// <summary>
		/// The m x.
		/// </summary>
		private int mX;
		/// <summary>
		/// The m y.
		/// </summary>
		private int mY;
	
		/// <summary>
		/// Gets or sets the x.
		/// </summary>
		/// <value>
		/// The x.
		/// </value>
		public int X {
			get { return this.mX;}
			set { this.mX = value;}
		}
	
		/// <summary>
		/// Gets or sets the y.
		/// </summary>
		/// <value>
		/// The y.
		/// </value>
		public int Y {
			get { return this.mY;}
			set { this.mY = value;}
		}
	
		/// <summary>
		/// Initializes a new instance of the <see cref="JumpPointSearach.Point"/> class.
		/// </summary>
		/// <param name='x'>
		/// X.
		/// </param>
		/// <param name='y'>
		/// Y.
		/// </param>
		public Point (int x, int y)
		{
			this.mX = x;
			this.mY = y;
		}
	}
}

