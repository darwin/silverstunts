using System;
using System.Collections.Generic;
using System.Text;

using Physics;
using Physics.Util;
using Physics.Surfaces;
using Physics.Primitives;
using System.Windows.Media;

namespace Physics.Primitives
{
	/**
	 * Physics - 2D Dynamics Engine
	 * Release 0.1 alpha 
	 * RectangleParticle class
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
	public class RectangleParticle : Particle
	{
		private double width;
		private double height;
		private Vector vertex;
		
		public RectangleParticle( double px, double py, double w, double h ) : base( px, py )
		{	
			width = w;
			height = h;
			
			vertex = new Vector( 0, 0 );
			extents = new Vector( w / 2, h / 2 );
		}
		
		public override void CheckCollision( ISurface surface, Engine engine )
		{
			surface.ResolveRectangleCollision( this, engine );
		}

		public override void CheckCollision( IParticle particle, Engine engine, ref CollisionState state )
		{
			//particle.ResolveRectangleCollision( this, engine );
		}

		public double Width { get { return width; } }
		public double Height { get { return height; } }
		public Vector Vertex { get { return vertex; } }

		public Vector CenterOffset { get { return new Vector( -width / 2, -height / 2 ); } }

		public Matrix Matrix
		{
			get { return new Matrix(1, 0, 0, 1, curr.X, curr.Y); }
		}
	}
}