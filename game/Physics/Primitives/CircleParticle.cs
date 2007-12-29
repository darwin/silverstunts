using System;
using System.Collections.Generic;
using System.Text;

using Physics;
using Physics.Util;
using Physics.Surfaces;
using Physics.Primitives;

namespace Physics.Primitives
{
	/**
	 * Physics - 2D Dynamics Engine
	 * Release 0.1 alpha 
	 * CircleParticle class
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
	public class CircleParticle : Particle
	{
		private double radius;
		private Vector closestPoint;
		private double contactRadius;

		public CircleParticle( double px, double py, double r ) : base( px, py )
		{
			radius = r;
			contactRadius = r;
			
			extents = new Vector( r, r );
			closestPoint = new Vector( 0, 0 );
		}

		public override void CheckCollision( ISurface surface, Engine engine )
		{
			surface.ResolveCircleCollision( this, engine );
		}

		public override void CheckCollision( IParticle particle, Engine engine, ref CollisionState state )
		{
            // HACK: here i need not to collide wheels with body particles
            if (this is Wheel && !(particle is Wheel)) return;
            if (particle is Wheel && !(this is Wheel)) return;
            particle.ResolveCircleCollision(this, engine, ref state);
		}

		public Vector ClosestPoint { get { return closestPoint; } set { closestPoint = value; } }
		public double Radius { get { return radius; } set { radius = value; } }
		public double Diameter { get { return radius * 2; } set { radius = value / 2; } }
		public Vector Size { get { double diameter = Diameter; return new Vector( diameter, diameter ); } }

		public Vector CenterOffset { get { return new Vector( -radius, -radius ); } }

		#region IParticle Members

		public override void ResolveCircleCollision( CircleParticle p, Engine engine, ref CollisionState state )
		{
			if ( Collision.IsEllipseCollision( curr, Size, 0, p.curr, p.Size, 0, out state ) )
			{
				Vector normal = state.Depth.Clone().Normalize();

				Vector vel1 = curr.MinusNew( prev );
				Vector vel2 = p.curr.MinusNew( p.prev );

				p.prev = p.curr.PlusNew( state.Depth );
				prev = curr.MinusNew( state.Depth );
			}
		}

		public override void ResolveRectangleCollision( RectangleParticle p, Engine engine, ref CollisionState state )
		{
			throw new Exception( "The method or operation is not implemented." );
		}

		#endregion
	}
}