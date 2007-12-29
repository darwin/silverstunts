using System;
using System.Collections.Generic;
using System.Text;

using Physics.Util;
using Physics.Primitives;
using Physics.Surfaces;

namespace Physics.Constraints
{
	/**
	 * Physics - 2D Dynamics Engine
	 * Release 0.1 alpha 
	 * SpringConstraint class
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
    public class SpringConstraint : Constraint, IConstraint
	{
		private Particle p1;
		private Particle p2;
		private double restLength;
		
		private double stiffness;
		private bool visible;

		public SpringConstraint( Particle p1, Particle p2 )
		{
			this.p1 = p1;
			this.p2 = p2;
			restLength = p1.Curr.Distance( p2.Curr );

			stiffness = 0.5;
			visible = true;
		}		
		
		public void Resolve()
		{
			Vector delta = p1.Curr.MinusNew( p2.Curr );
			double deltaLength = p1.Curr.Distance( p2.Curr );

			double diff = ( deltaLength - restLength ) / deltaLength;
			Vector dmd = delta.Mult( diff * stiffness );

			p1.Curr.Minus( dmd );
			p2.Curr.Plus( dmd );
		}

		public double RestLength { get { return restLength; } set { restLength = value; } }
		public double Stiffness { get { return stiffness; } set { stiffness = value; } }
		public bool Visible { get { return visible; } set { visible = value; } }
		public Particle P1 { get { return p1; } }
		public Particle P2 { get { return p2; } }
	}
}