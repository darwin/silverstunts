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
	 * RectangleSurface class
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
	public class RectangleSurface : AbstractSurface, ISurface
	{
		private double rectWidth;
		private double rectHeight;
		
		public RectangleSurface( double cx, double cy, double rw, double rh ) : base( cx, cy )
		{
            Init(cx, cy, rw, rh);
		}

        public void Init(double cx, double cy, double rw, double rh)
        {
            base.Init(cx, cy);
            rectWidth = rw;
            rectHeight = rh;
            CreateBoundingRect(rw, rh);
        }

		public void ResolveCircleCollision( CircleParticle p, Engine engine )
		{
			if ( IsCircleColliding( p ) )
			{
				OnContact();
                p.OnContact();
				p.ResolveCollision( normal, engine );
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

			// determine if the circle's center is in a vertex voronoi region
			bool isInVertexX = Math.Abs( depthX ) < p.Radius;
			bool isInVertexY = Math.Abs( depthY ) < p.Radius;

			if ( isInVertexX && isInVertexY )
			{	
				// get the closest vertex
				double vx = center.X + Sign( p.Curr.X - center.X ) * ( rectWidth / 2 );
				double vy = center.Y + Sign( p.Curr.Y - center.Y ) * ( rectHeight / 2 );
				
				// get the distance from the vertex to circle center
				double dx = p.Curr.X - vx;
				double dy = p.Curr.Y - vy;
    			double mag = Math.Sqrt( dx * dx + dy * dy );
				double pen = p.Radius - mag;
				
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
				
		public bool IsRectangleColliding( RectangleParticle p )
		{	
			p.GetCardXProjection();
			double depthX = TestIntervals( p.BMin, p.BMax, minX, maxX );
			if ( depthX == 0 ) return false;

			p.GetCardYProjection();
			double depthY = TestIntervals( p.BMin, p.BMax, minY, maxY );
			if ( depthY == 0 ) return false;
			
			p.SetXYMTD( depthX, depthY );
			normal.Set( p.MTD.X / Math.Abs( depthX ), p.MTD.Y / Math.Abs( depthY ) );
			return true;
		}
				
		// TBD: Put in a util class
		private double Sign( double val )
		{
			return ( val < 0 ) ? -1 : ( val > 0 ) ? 1 : 0;
		}

		public double Width { get { return rectWidth; } }
		public double Height { get { return rectHeight; } }
		public Vector CenterOffset { get { return new Vector( -rectWidth / 2, -rectHeight / 2 ); } }
	}
}