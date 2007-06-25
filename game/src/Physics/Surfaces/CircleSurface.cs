using System;
using System.Collections.Generic;
using System.Text;

using Physics;
using Physics.Surfaces;
using Physics.Primitives;
using Physics.Util;

namespace Physics.Surfaces
{
	/**
	 * Physics - 2D Dynamics Engine
	 * Release 0.1 alpha 
	 * CircleSurface class
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
	public class CircleSurface : AbstractSurface, ISurface
	{
		private double radius;

		public CircleSurface( double cx, double cy, double r ) : base( cx, cy )
		{		
            Init(cx, cy, r);
		}

        public void Init(double cx, double cy, double r)
        {
            base.Init(cx, cy);
			CreateBoundingRect( r * 2, r * 2 );
			radius = r;
        }

        public void ResolveCircleCollision( CircleParticle p, Engine engine )
		{
            if (IsCircleColliding(p))
            {
                OnContact();
                p.OnContact();
                p.ResolveCollision(normal, engine);
            }
		}

		public void ResolveRectangleCollision( RectangleParticle p, Engine engine )
		{
			if ( IsRectangleColliding( p ) )
			{
				OnContact();
                p.OnContact();
				p.ResolveCollision( normal, engine );
			}
        }
				
		private bool IsCircleColliding( CircleParticle p )
		{
			p.GetCardXProjection();
			double depthX = TestIntervals( p.BMin, p.BMax, minX, maxX );
			if ( depthX == 0 ) return false;

			p.GetCardYProjection();
			double depthY = TestIntervals( p.BMin, p.BMax, minY, maxY );
			if ( depthY == 0 ) return false;
			
			double dx = center.X - p.Curr.X;
			double dy = center.Y - p.Curr.Y;
			double len = Math.Sqrt( dx * dx + dy * dy );
			double pen = ( p.Radius + radius ) - len;
			
			if ( pen > 0 )
			{
				dx /= len;
				dy /= len;
				p.MTD.Set( -dx * pen, -dy * pen );
				normal.Set( -dx, -dy );
				return true;
			}

			return false;
		}
				
		// TBD: This method is basically identical to the isCircleColliding of the
		// RectangleSurface class. Need some type of CollisionResolver class to handle
		// all collisions and move responsibility away from the Surface classes. 
		public bool IsRectangleColliding( RectangleParticle p )
		{
			p.GetCardXProjection();
			double depthX = TestIntervals( p.BMin, p.BMax, minX, maxX );
			if ( depthX == 0 ) return false;
					
			p.GetCardYProjection();
			double depthY = TestIntervals( p.BMin, p.BMax, minY, maxY );
			if ( depthY == 0 ) return false;
			
			// determine if the circle's center is in a vertex voronoi region
			bool isInVertexX = Math.Abs( depthX ) < radius;
			bool isInVertexY = Math.Abs( depthY ) < radius;

			if ( isInVertexX && isInVertexY )
			{
				// get the closest vertex
				double vx = p.Curr.X + Sign( center.X - p.Curr.X ) * ( p.Width / 2 );
				double vy = p.Curr.Y + Sign( center.Y - p.Curr.Y ) * ( p.Height / 2 );
				p.Vertex.Set( vx, vy );

				// get the distance from the vertex to circle center
				double dx = p.Vertex.X - center.X;
				double dy = p.Vertex.Y - center.Y;
				double mag = Math.Sqrt( dx * dx + dy * dy );
				double pen = radius - mag;

				// if there is a collision in one of the vertex regions
				if ( pen > 0 )
				{
					dx /= mag;
					dy /= mag;
					p.MTD.Set( dx * pen, dy * pen );
					normal.Set( dx, dy );
					return true;
				}
				return false;
			}
			else
			{
				// collision on one of the 4 edges
				p.SetXYMTD( depthX, depthY );
				normal.Set( p.MTD.X / Math.Abs( depthX ), p.MTD.Y / Math.Abs( depthY ) );
				return true;
			}
		}
		
		// TBD: Put in a util class
		private double Sign( double val )
		{
			return ( val < 0 ) ? -1 : ( val > 0 ) ? 1 : 0;
		}

		public double Radius { get { return radius; } set { radius = value; } }
		public double Diameter { get { return radius * 2; } set { radius = value / 2; } }
		public Vector CenterOffset { get { return new Vector( -radius, -radius ); } }
	}
}