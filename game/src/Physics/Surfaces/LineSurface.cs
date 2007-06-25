using System;
using System.Collections.Generic;
using System.Text;

using Physics;
using Physics.Util;
using Physics.Surfaces;
using Physics.Primitives;

namespace Physics.Surfaces
{
	/**
	 * Physics - 2D Dynamics Engine
	 * Release 0.1 alpha 
	 * LineSurface class
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
	public class LineSurface : AbstractSurface, ISurface
	{
		private Vector p1;
		private Vector p2;
		private Vector p3;
		private Vector p4;
		private Vector faceNormal;
		private Vector sideNormal;
		private Vector collNormal;
        private Vector collisionDepthNormal;

        private Vector centerOffset;

        public Vector CollisionDepthNormal { get { return collisionDepthNormal; } }
        public Vector CenterOffset { get { return centerOffset; } }
        public Vector CenterOffsetInv { get { return centerOffset.MultNew(-1); } }
        public Vector OP1 { get { return p1; } }
        public Vector OP2 { get { return p2; } }
        
		private double rise;
		private double run;
		
		private double invB;
		private double sign;
		private double slope;
		
		private double minF;
		private double maxF;
		private double minS;
		private double maxS;
		private double collisionDepth;
		
		public LineSurface( double p1x, double p1y, double p2x, double p2y ) : base( 0, 0 )
		{
            Init(p1x, p1y, p2x, p2y);
		}

        public void Init(double p1x, double p1y, double p2x, double p2y)
        {
            base.Init(0, 0);

            p1 = new Vector(p1x, p1y);
            p2 = new Vector(p2x, p2y);

            CalcFaceNormal();
            collNormal = new Vector(0, 0);
            SetCollisionDepth(30);
            collisionDepthNormal = faceNormal.MultNew(-collisionDepth);
            //center.X = (p1.X + p2.X + p3.X + p4.X) / 4;
            //center.Y = (p1.Y + p2.Y + p3.Y + p4.Y) / 4;
            //double w = verts[0].X - verts[2].X;
           // double h = verts[0].Y - verts[1].Y;

            center.X = (p1.X + p2.X) / 2;
            center.Y = (p1.Y + p2.Y) / 2;
            double w = Math.Abs(p2.X - p1.X);
            double h = Math.Abs(p2.Y - p1.Y);
            centerOffset = new Vector(- w / 2, -h / 2);
        }

        public void ResolveCircleCollision(CircleParticle p, Engine engine)
		{
			if ( IsCircleColliding( p ) )
			{
				OnContact();
                p.OnContact();
				p.ResolveCollision( faceNormal, engine );
			}
        }
        
		public void ResolveRectangleCollision( RectangleParticle p, Engine engine )
		{
			if ( IsRectangleColliding( p ) )
			{
				OnContact();
                p.OnContact();
				p.ResolveCollision( collNormal, engine );
			}
        }

		public void SetCollisionDepth( double d )
		{
			collisionDepth = d;
			PreCalculate();
		}
				
		private bool IsCircleColliding( CircleParticle p )
		{
			// find the closest point on the surface to the CircleParticle
			p.ClosestPoint = FindClosestPoint( p.Curr );

			// get the normal of the circle relative to the location of the closest point
			Vector circleNormal = p.ClosestPoint.MinusNew( p.Curr );
			circleNormal.Normalize();
			
			// if the center of the circle has broken the line keep the normal from 'flipping'
			// to the opposite direction. for small circles, this prevents break-throughs
			if ( Inequality( p.Curr ) )
			{
				double absCX = Math.Abs( circleNormal.X );
				circleNormal.X = ( faceNormal.X < 0 ) ? absCX : -absCX;
				circleNormal.Y = Math.Abs( circleNormal.Y );
			}
			
			// get contact point on edge of circle
			Vector contactPoint = p.Curr.PlusNew( circleNormal.Mult( p.Radius ) );

			if ( SegmentInequality( contactPoint ) )
			{	
				if ( contactPoint.Distance( p.ClosestPoint ) > collisionDepth ) return false;

				double dx = contactPoint.X - p.ClosestPoint.X;
				double dy = contactPoint.Y - p.ClosestPoint.Y;
				p.MTD.Set( -dx, -dy );
				return true;
			}

			return false;
		}
        
		public bool IsRectangleColliding( RectangleParticle p )
		{
			p.GetCardYProjection();
			double depthY = TestIntervals( p.BMin, p.BMax, minY, maxY );
			if ( depthY == 0 ) return false;
			
			p.GetCardXProjection();
			double depthX = TestIntervals( p.BMin, p.BMax, minX, maxX );
			if ( depthX == 0 ) return false;
			
			p.GetAxisProjection( sideNormal );
			double depthS = TestIntervals( p.BMin, p.BMax, minS, maxS );
			if ( depthS == 0 ) return false;
			
			p.GetAxisProjection( faceNormal );
			double depthF = TestIntervals( p.BMin, p.BMax, minF, maxF );
			if ( depthF == 0 ) return false;
					
			double absX = Math.Abs( depthX );
			double absY = Math.Abs( depthY );
			double absS = Math.Abs( depthS );
			double absF = Math.Abs( depthF );
				
			if ( absX <= absY && absX <= absS && absX <= absF )
			{
				p.MTD.Set( depthX, 0 );
				collNormal.Set( p.MTD.X / absX, 0 );
			}
			else if ( absY <= absX && absY <= absS && absY <= absF )
			{
				p.MTD.Set( 0, depthY );
				collNormal.Set( 0, p.MTD.Y / absY );
			}
			else if ( absF <= absX && absF <= absY && absF <= absS )
			{
				p.MTD = faceNormal.MultNew( depthF );
				collNormal.Copy( faceNormal );
			}
			else if ( absS <= absX && absS <= absY && absS <= absF )
			{
				p.MTD = sideNormal.MultNew( depthS );
				collNormal.Copy( sideNormal );
			}

			return true;
		}

		private void PreCalculate()
		{
			// precalculations for circle collision
			rise = p2.Y - p1.Y;
			run = p2.X - p1.X;
			
			// TBD: sign is a quick bug fix, needs to be review
			sign = ( run >= 0 ) ? 1 :-1;
			slope = rise / run;
			invB = 1 / ( run * run + rise * rise );
				
			// precalculations for rectangle collision
			CreateRectangle();
			CalcSideNormal();
			SetCardProjections();
			SetAxisProjections();
		}
				
		private void CalcFaceNormal()
		{
			faceNormal = new Vector( 0, 0 );
			double dx = p2.X - p1.X;
			double dy = p2.Y - p1.Y;
			faceNormal.Set( dy, -dx );
			faceNormal.Normalize();
		}

		private bool SegmentInequality( Vector toPoint )
		{
			double u = FindU( toPoint );
			bool isUnder = Inequality( toPoint );
			return ( u >= 0 && u <= 1 && isUnder );
		}

		private bool Inequality( Vector toPoint )
		{	
			// TBD: sign is a quick bug fix, needs to be review
			double line = (slope * ( toPoint.X - p1.X ) + ( p1.Y - toPoint.Y ) ) * sign;
			return ( line <= 0 );
		}

		private Vector FindClosestPoint( Vector toPoint )
		{
			double u = FindU( toPoint );

			if ( u <= 0 ) return p1.Clone();
			if ( u >= 1 ) return p2.Clone();

			double x = p1.X + u * ( p2.X - p1.X );
			double y = p1.Y + u * ( p2.Y - p1.Y );

			return new Vector( x, y );
		}

		private double FindU( Vector p )
		{
			double a = ( p.X - p1.X ) * run + ( p.Y - p1.Y ) * rise;
			return a * invB;
		}
		
		
		private void CreateRectangle()
		{	
			double p3x = p2.X + -faceNormal.X * collisionDepth;
			double p3y = p2.Y + -faceNormal.Y * collisionDepth;
			
			double p4x = p1.X + -faceNormal.X * collisionDepth;
			double p4y = p1.Y + -faceNormal.Y * collisionDepth;
			
			p3 = new Vector( p3x, p3y );
			p4 = new Vector( p4x, p4y );
			
			verts.Add( p1 );
			verts.Add( p2 );
			verts.Add( p3 );
			verts.Add( p4 );
		}
		
		private void SetAxisProjections()
		{
			double temp;
			
			minF = p2.Dot( faceNormal );
			maxF = p3.Dot( faceNormal );

			if ( minF > maxF )
			{
				temp = minF;
				minF = maxF;
				maxF = temp;
			}
			
			minS = p1.Dot( sideNormal );
			maxS = p2.Dot( sideNormal );

			if ( minS > maxS )
			{
				temp = minS;
				minS = maxS;
				maxS = temp;
			}	
		}
		
		private void CalcSideNormal()
		{
			sideNormal = new Vector( 0, 0 );
			double dx = p3.X - p2.X;
			double dy = p3.Y - p2.Y;
			sideNormal.Set( dy, -dx );
			sideNormal.Normalize();
		}

        public Vector P1 { get { return p1.MinusNew(center).MinusNew(centerOffset); } }
        public Vector P2 { get { return p2.MinusNew(center).MinusNew(centerOffset); } }
	}
}