using System;
using System.Collections.Generic;
using System.Text;

using Physics;
using Physics.Primitives;
using Physics.Constraints;

namespace Physics.Composites
{
	/**
	 * Physics - 2D Dynamics Engine
	 * Release 0.1 alpha 
	 * RigidComposite class
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
	public class RigidComposite
	{
		private Particle[] particles;

		public RigidComposite( Engine engine, params Particle[] particles )
		{
			this.particles = particles;

			foreach ( Particle particle in particles )
			{
				engine.Add( particle );
			}

			for ( int index = 0; index < particles.Length; index += 2 )
			{
				int index1 = ( index + 1 < particles.Length ) ? index + 1 : 0;
				int index2 = ( index1 + 1 < particles.Length ) ? index1 + 1 : 0;

				AngularConstraint constraint = new AngularConstraint(
					particles[ index ], particles[ index1 ], particles[ index2 ] );

				engine.Add( constraint );
			}
		}

		public IList<Particle> Particles { get { return particles; } }
	}
}