using System;
using System.Collections.Generic;
using System.Text;

using Physics;
using Physics.Util;

namespace Physics.Primitives
{
	/**
	 * Physics - 2D Dynamics Engine
	 * Release 0.1 alpha 
	 * RimParticle class
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
	public class RimParticle
	{
		protected Vector curr;
		protected Vector prev;
		protected double speed;
		protected double vs;

		protected double wr;
		protected double maxTorque;
		
		/**
		 * The RimParticle is really just a second component of the wheel model.
		 * The rim particle is simulated in a coordsystem relative to the wheel's 
		 * center, not in worldspace
		 */
		public RimParticle( double r, double mt )
		{
			curr = new Vector( r, 0 );
			prev = new Vector( 0, 0 );

			vs = 0;			// variable speed
			speed = 0; 		// initial speed
			maxTorque = mt; 	
			wr = r;		
		}

		// TBD: provide a way to get the worldspace position of the rimparticle
		// either here, or in the wheel class, so it can be used to move other
		// primitives / constraints
		public void Verlet( Engine engine )
		{
			//clamp torques to valid range
			speed = Math.Max( -maxTorque, Math.Min( maxTorque, speed + vs ) );

			//apply torque
			//this is the tangent vector at the rim particle
			double dx = -curr.Y;
			double dy = curr.X;

			//normalize so we can scale by the rotational speed
			double len = Math.Sqrt( dx * dx + dy * dy );
			dx /= len;
			dy /= len;

			curr.X += speed * dx;
			curr.Y += speed * dy;		

			double ox = prev.X;
			double oy = prev.Y;
			double px = prev.X = curr.X;		
			double py = prev.Y = curr.Y;		

			curr.X += engine.CoeffDamp * ( px - ox );
			curr.Y += engine.CoeffDamp * ( py - oy );

			// hold the rim particle in place
			double clen = Math.Sqrt( curr.X * curr.X + curr.Y * curr.Y );
			double diff = ( clen - wr ) / clen;

			if ( Math.Abs( diff ) < 0.0001 ) diff = 0;

			curr.X -= curr.X * diff;
			curr.Y -= curr.Y * diff;
		}

		public void Decelerate( double speed )
		{
			vs = 0;
//			if ( vs < 0 ) vs = Math.Min( 0, vs + speed );
//			else if ( vs > 0 ) vs = Math.Max( 0, vs - speed );
		}

		public Vector Prev { get { return prev; } }
		public Vector Curr { get { return curr; } }
		public double Speed { get { return speed; } set { speed = value; } }
		public double VS { get { return vs; } set { vs = value; } }
	}
}