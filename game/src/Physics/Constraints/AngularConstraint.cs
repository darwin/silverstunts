using System;
using System.Collections.Generic;
using System.Text;

using Physics.Util;
using Physics.Primitives;
using Physics.Constraints;

namespace Physics.Constraints
{
	/**
	 * Physics - 2D Dynamics Engine
	 * Release 0.1 alpha 
	 * AngularConstraint class
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
	public class AngularConstraint : Constraint, IConstraint
	{
		private double targetTheta;

		private Vector pA;
		private Vector pB;
		private Vector pC;
		private Vector pD;

		private Line lineA;
		private Line lineB;
		private Line lineC;

		private double stiffness;
		
		public AngularConstraint( Particle p1, Particle p2, Particle p3 )
		{
			pA = p1.Curr;
			pB = p2.Curr;
			pC = p3.Curr;

			lineA = new Line( pA, pB );
			lineB = new Line( pB, pC );

			// lineC is the reference line for getting the angle of the line segments
			pD = new Vector( pB.X + 0, pB.Y - 1 );
			lineC = new Line( pB, pD );

			// theta to constrain to -- domain is -Math.PI to Math.PI
			targetTheta = CalcTheta( pA, pB, pC );

			// coefficient of stiffness
			stiffness = 1;
		}

		public void Resolve()
		{
			Vector center = GetCentroid();

			// make sure the reference line position gets updated
			lineC.P2.X = lineC.P1.X + 0;
			lineC.P2.Y = lineC.P1.Y - 1;

			double abRadius = pA.Distance( pB );
			double bcRadius = pB.Distance( pC );

			double thetaABC = CalcTheta( pA, pB, pC );
			double thetaABD = CalcTheta( pA, pB, pD );
			double thetaCBD = CalcTheta( pC, pB, pD );

			double halfTheta = ( targetTheta - thetaABC ) / 2;
			double paTheta = thetaABD + halfTheta * stiffness;
			double pcTheta = thetaCBD - halfTheta * stiffness;

			pA.X = abRadius * Math.Sin( paTheta ) + pB.X;
			pA.Y = abRadius * Math.Cos( paTheta ) + pB.Y;
			pC.X = bcRadius * Math.Sin( pcTheta ) + pB.X;
			pC.Y = bcRadius * Math.Cos( pcTheta ) + pB.Y;

			// move corrected angle to pre corrected center
			Vector newCenter = GetCentroid();
			double dfx = newCenter.X - center.X;
			double dfy = newCenter.Y - center.Y;

			pA.X -= dfx; 
			pA.Y -= dfy;
			pB.X -= dfx;  
			pB.Y -= dfy;
			pC.X -= dfx;  
			pC.Y -= dfy; 
		}

		public double Stiffness { get { return stiffness; } set { stiffness = value; } }

		private double CalcTheta( Vector pa, Vector pb, Vector pc )
		{
			Vector AB = new Vector( pb.X - pa.X, pb.Y - pa.Y );
			Vector BC = new Vector( pc.X - pb.X, pc.Y - pb.Y );

			double dotProd = AB.Dot( BC );
			double crossProd = AB.Cross( BC );
			return Math.Atan2( crossProd, dotProd );
		}

		private Vector GetCentroid()
		{
			double avgX = ( pA.X + pB.X + pC.X ) / 3;
			double avgY = ( pA.Y + pB.Y + pC.Y ) / 3;
			return new Vector( avgX, avgY );
		}

		public double TargetTheta { get { return targetTheta; } set { targetTheta = value; } }
	}
}