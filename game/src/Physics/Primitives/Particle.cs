using System;
using System.Collections.Generic;
using System.Text;

using Physics;
using Physics.Util;
using Physics.Surfaces;
using System.ComponentModel;

using Physics.Base;

namespace Physics.Primitives
{
	/**
	 * Physics - 2D Dynamics Engine
	 * Release 0.1 alpha 
	 * Particle class
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
    public class Particle : PhysicsObject, IParticle //, INotifyPropertyChanged
	{
		protected Vector curr;
		protected Vector prev;
		protected double bMin;
		protected double bMax;
		protected Vector mtd;

		protected Vector init;
		protected Vector temp;
		protected Vector extents;

		private static int nextIndex = 0;

		//private var dmc:MovieClip;
		protected bool visible;
		protected int index;

		public Particle( double posX, double posY )
		{
			index = nextIndex++;

			// store initial position, for pinning
			init = new Vector( posX, posY );
			
			// current and previous positions - for integration
			curr = new Vector( posX, posY );
			prev = new Vector( posX, posY );
			temp = new Vector( 0, 0 );
			
			// attributes for collision detection with tiles
			this.extents = new Vector( 0, 0 ); 

			bMin = 0;
			bMax = 0;
			mtd = new Vector( 0, 0 );
			
			visible = true;
		}

		public bool Visible { get { return visible; } set { visible = value; } }

		public virtual void Verlet( Engine engine )
		{
			temp.X = curr.X;
			temp.Y = curr.Y;
			
			curr.X += engine.CoeffDamp * ( curr.X - prev.X ) + engine.Gravity.X;
			curr.Y += engine.CoeffDamp * ( curr.Y - prev.Y ) + engine.Gravity.Y;

			prev.X = temp.X;
			prev.Y = temp.Y;
		}
		
		public void Pin()
		{
			curr.X = init.X;
			curr.Y = init.Y;
			prev.X = init.X;
			prev.Y = init.Y;
		}
				
		public void SetPos( double px, double py )
		{
			curr.X = px;
			curr.Y = py;
			prev.X = px;
			prev.Y = py;
		}

		/**
		 * Get projection onto a cardinal (world) axis x 
		 */
		// TODO: rename to something other than "get" 
		// TODO: there is another implementation of this in the 
		// AbstractSurface base class.
		public void GetCardXProjection()
		{
			bMin = curr.X - extents.X;
			bMax = curr.X + extents.X;
		}

		/**
		 * Get projection onto a cardinal (world) axis y
		 */	
		// TBD: there is another implementation of this in the 
		// AbstractSurface base class. see if they can be combined
		public void GetCardYProjection()
		{
			bMin = curr.Y - extents.Y;
			bMax = curr.Y + extents.Y;
		}

		/**
		 * Get projection onto arbitrary axis. Note that axis need not be unit-length. If
		 * it is not, min and max will be scaled by the length of the axis. This is fine
		 * if all we're doing is comparing relative values. If we need the 'actual' projection,
		 * the axis should be unit length.
		 */
		public void GetAxisProjection( Vector axis )
		{
			Vector absAxis = new Vector( Math.Abs( axis.X ), Math.Abs( axis.Y ) );
			double projectedCenter = curr.Dot( axis );
			double projectedRadius = extents.Dot( absAxis );

			bMin = projectedCenter - projectedRadius;
			bMax = projectedCenter + projectedRadius;
		}

		/**
		 * Find minimum depth and set mtd appropriately. mtd is the minimum translational 
		 * distance, the vector along which we must move the box to resolve the collision.
		 */
		 //TBD: this is only for right triangle surfaces - make generic
		public void SetMTD( double depthX, double depthY, double depthN, Vector surfNormal )
		{
			double absX = Math.Abs( depthX );
			double absY = Math.Abs( depthY );
			double absN = Math.Abs( depthN );

			if ( absX < absY && absX < absN ) mtd.Set( depthX, 0 );
			else if ( absY < absX && absY < absN ) mtd.Set( 0, depthY );
			else if ( absN < absX && absN < absY ) mtd = surfNormal.MultNew( depthN );
		}

		/**
		 * Set the mtd for situations where there are only the x and y axes to consider.
		 */
		public void SetXYMTD( double depthX, double depthY )
		{
			double absX = Math.Abs( depthX );
			double absY = Math.Abs( depthY );

			if ( absX < absY ) mtd.Set( depthX, 0 );
			else mtd.Set( 0, depthY );
		}		
		
		// TBD: too much passing around of the Physics object. Probably better if
		// it was static.  there is no way to individually set the kfr and friction of the
		// surfaces since they are calculated here from properties of the Physics
		// object. Also, review for too much object creation
		public virtual Vector ResolveCollision( Vector normal, Engine engine)
		{					
			// get the velocity
			Vector vel = curr.MinusNew( prev );
			double sDotV = normal.Dot( vel );

			// compute momentum of particle perpendicular to normal
			Vector velProjection = vel.MinusNew( normal.MultNew( sDotV ) );
			Vector perpMomentum = velProjection.MultNew( engine.CoeffFric );

			// compute momentum of particle in direction of normal
			Vector normMomentum = normal.MultNew( sDotV * engine.CoeffRest );
			Vector totalMomentum = normMomentum.PlusNew( perpMomentum );

			// set new velocity w/ total momentum
			Vector newVel = vel.MinusNew( totalMomentum );

			// project out of collision
			curr.Plus( mtd );

			// apply new velocity
			prev = curr.MinusNew( newVel );

			return newVel;
		}

		public virtual void CheckCollision( ISurface surface, Engine engine )
		{
		}

		public virtual void CheckCollision( IParticle particle, Engine engine, ref CollisionState state )
		{
		}

		protected virtual bool HasChanged()
		{
			return curr.X != prev.X || curr.Y != prev.Y;
		}

		#region IParticle

		public int Index { get { return index; } }

		public virtual void ResolveCircleCollision( CircleParticle p, Engine engine, ref CollisionState state )
		{
		}

		public virtual void ResolveRectangleCollision( RectangleParticle p, Engine engine, ref CollisionState state )
		{
		}

		#endregion

		public Vector Prev { get { return prev; } }
		public Vector Curr { get { return curr; } }
		public Vector MTD { get { return mtd; } set { mtd = value; } }
		public double BMin { get { return bMin; } }
		public double BMax { get { return bMax; } }

        public event EventHandler Contact;

        // empty holder for the onContact event
        public void OnContact()
        {
            if (Contact != null) Contact(this, EventArgs.Empty);
        }
	}
}