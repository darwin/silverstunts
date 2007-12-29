using System;
using System.Collections.Generic;
using System.Text;

using Physics;
using Physics.Util;
using Physics.Primitives;

namespace Physics.Primitives
{
	/**
	 * Physics - 2D Dynamics Engine
	 * Release 0.1 alpha 
	 * Wheel class
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
	public class Wheel : CircleParticle
	{
		private RimParticle rp;
		private double coeffSlip;
        public Vector jumpNormal;
        public int lastJumpNormalTick = 0;

		public Wheel( double x, double y, double r ) : base( x, y, r )
		{
			// TODO: set max torque?
			// rim particle (radius, max torque)
			rp = new RimParticle( r, 5 ); 		

			// TBD:Review this for a higher level of friction
			// 1 = totally slippery, 0 = full friction
			coeffSlip = 0.2;
		}

		public override void Verlet( Engine engine )
		{
			rp.Verlet( engine );
			base.Verlet( engine );
		}


		public override Vector ResolveCollision( Vector normal, Engine engine )
		{
			Vector velocity = base.ResolveCollision( normal, engine );
			Resolve( normal );
            lastJumpNormalTick = engine.tick;
            jumpNormal = normal;
			return velocity;
		}

		public void SetTraction( double t )
		{
			coeffSlip = t;
		}

		/**
		 * simulates torque/wheel-ground interaction - n is the surface normal
		 */
		private void Resolve( Vector n )
		{
			// this is the tangent vector at the rim particle
			double rx = -rp.Curr.Y;
			double ry = rp.Curr.X;

			// normalize so we can scale by the rotational speed
			double len = Math.Sqrt( rx * rx + ry * ry );
			rx /= len;
			ry /= len;

			// sx,sy is the velocity of the wheel's surface relative to the wheel
			double sx = rx * rp.Speed;
			double sy = ry * rp.Speed;

			// tx,ty is the velocity of the wheel relative to the world
			double tx = curr.X - prev.X;
			double ty = curr.Y - prev.Y;

			// vx,vy is the velocity of the wheel's surface relative to the ground
			double vx = tx + sx;
			double vy = ty + sy;

			// dp is the the wheel's surfacevel projected onto the ground's tangent
			double dp = -n.Y * vx + n.X * vy;

			// set the wheel's spinspeed to track the ground
			rp.Prev.X = rp.Curr.X - dp * rx;
			rp.Prev.Y = rp.Curr.Y - dp * ry;

			// some of the wheel's torque is removed and converted into linear displacement
			double w0 = 1 - coeffSlip;
			curr.X += w0 * rp.Speed * -n.Y;
			curr.Y += w0 * rp.Speed * n.X;
			rp.Speed *= coeffSlip;
		}

		protected override bool HasChanged()
		{
			bool angleChanged = ( rp.Curr.X != rp.Prev.X || rp.Curr.Y != rp.Prev.Y );

			//if ( angleChanged ) OnPropertyChanged( "Angle" );

			return angleChanged || base.HasChanged();
		}

		public double Angle { get { return rp.Curr.Angle / ( Math.PI * 2 ) * 360; } }
		public RimParticle RP { get { return rp; } }
	}
}