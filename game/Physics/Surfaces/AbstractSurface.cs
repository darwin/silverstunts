using System;
using System.Collections.Generic;
using System.Text;

using Physics.Base;
using Physics.Util;

namespace Physics.Surfaces
{
	/**
	 * Physics - 2D Dynamics Engine
	 * Release 0.1 alpha 
	 * AbstractSurface class
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
    public class AbstractSurface : PhysicsObject
	{
		protected double minX;
		protected double minY;
		protected double maxX;
		protected double maxY;
		protected List<Vector> verts;

        public List<Vector> Verts { get { return verts;  } }

		protected Vector center;
		protected Vector normal;

		protected bool active;

		public AbstractSurface( double cx, double cy )
		{
            Init(cx, cy);
		}

        public void Init(double cx, double cy)
        {
	 		center = new Vector( cx, cy );
	 		verts = new List<Vector>();
	 		normal = new Vector( 0, 0 );
		 	
	 		active = true;
            SetDirty();
        }

        public bool Active { get { return active; } set { active = value; SetDirty();  } }		
		
		public void CreateBoundingRect( double rw, double rh )
		{ 			
			double t = center.Y - rh / 2;
			double b = center.Y + rh / 2;
			double l = center.X - rw / 2;
			double r = center.X + rw / 2;
			
			verts.Add( new Vector( r, b ) );
			verts.Add( new Vector( r, t ) );
			verts.Add( new Vector( l, t ) );
			verts.Add( new Vector( l, b ) );
			SetCardProjections();
		}

		public double TestIntervals(
				double boxMin, 
				double boxMax, 
				double tileMin, 
				double tileMax )
		{
			// returns 0 if intervals do not overlap. Returns depth if they do overlap
			if ( boxMax < tileMin ) return 0;
			if ( tileMax < boxMin ) return 0;

			// return the smallest translation
			double depth1 = tileMax - boxMin;
			double depth2 = tileMin - boxMax;

			return ( Math.Abs( depth1 ) < Math.Abs( depth2 ) ) ? depth1 : depth2;
		}

		public void SetCardProjections()
		{
			GetCardXProjection();
			GetCardYProjection();
		}

		// get projection onto a cardinal (world) axis x 
		// TBD: duplicate methods (with different implementation) in 
		// in the Particle base class. 
		public void GetCardXProjection()
		{
			minX = verts[0].X;
			maxX = verts[0].X;

			foreach ( Vector vert in verts )
			{
				if ( vert.X < minX ) minX = vert.X;
				if ( vert.X > maxX ) maxX = vert.X;
			}
		}

		// get projection onto a cardinal (world) axis y 
		// TBD: duplicate methods (with different implementation) in 
		// in the Particle base class. 
		public void GetCardYProjection()
		{
			minY = verts[ 0 ].Y;
			maxY = verts[ 0 ].Y;

			foreach ( Vector vert in verts )
			{
				if ( vert.Y < minY ) minY = vert.Y;
				if ( vert.Y > maxY ) maxY = vert.Y;
			}
		}

		public event EventHandler Contact;

		// empty holder for the onContact event
		public void OnContact()
		{
			if ( Contact != null ) Contact( this, EventArgs.Empty );
		}

		public Vector Center { get { return center; } }

        public bool TestBoundingBox(double x1, double y1, double x2, double y2)
        {
            GetCardXProjection();
            if (TestIntervals(minX, maxX, x1, x2)==0) return false;
            GetCardYProjection();
            return TestIntervals(minY, maxY, y1, y2)!=0;
        }
	}
}