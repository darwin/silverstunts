using System;
using System.Collections.Generic;
using System.Text;

namespace Physics.Util
{
	/**
	 * Physics - 2D Dynamics Engine
	 * Release 0.1 alpha 
	 * Line class
	 * Copyright 2007 Chris Cavanagh
	 * 
	 * Based on Flade source code by Alec Cove.
	 * 
	 * Physics is free software; you can redistribute it and/or modify
	 * it under the terms of the GNU General Public License as published by
	 * the Free Software Foundation; either version 2 of the License, or
	 * (at your option) any later version.
	 *
	 * Physics is distributed in the hope that it will be useful,
	 * but WITHOUT ANY WARRANTY; without even the implied warranty of
	 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	 * GNU General Public License for more details.
	 *
	 * You should have received a copy of the GNU General Public License
	 * along with Physics; if not, write to the Free Software
	 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
	 */
	public class Line
	{
		private Vector p1;
		private Vector p2;

		public Line( Vector p1, Vector p2 )
		{
			this.p1 = p1;
			this.p2 = p2;
		}

		public Vector P1 { get { return p1; } }
		public Vector P2 { get { return p2; } }
	}
}
