using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;

using Physics.Util;
using Physics.Surfaces;
using Physics.Primitives;
using Physics.Constraints;
using Physics.Base;

namespace Physics
{
	/**
	 * Physics - 2D Dynamics Engine
	 * Release 0.1 alpha 
	 * Physics class
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
	public class Engine // : INotifyPropertyChanged
	{
		private Vector gravity = new Vector( 0, 1 );
		private double coeffRest = 1f + 0.5f;
		private double coeffFric = 0.01;		// Surface friction
		private double coeffDamp = 0.99;		// Global damping
        private int tick = 0;

        //private IEnumerator<ISurface> surfaceEnumerator;
		private Collection<IParticle> particles = new Collection<IParticle>();
		private Collection<ISurface> surfaces = new Collection<ISurface>();
		private Collection<IConstraint> constraints = new Collection<IConstraint>();
		private Collection<CollisionState> collisions = new Collection<CollisionState>();

		public Engine()
		{
		}

        //private void OnPropertyChanged( string propertyName )
        //{
        //    if ( PropertyChanged != null ) PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
        //}

        public void Remove(PhysicsObject o)
        {
            if (o is IParticle) { particles.Remove(o as IParticle); return; }
            if (o is ISurface) { surfaces.Remove(o as ISurface); return; }
            if (o is IConstraint) { constraints.Remove(o as IConstraint); return; }
            throw new Exception("unknown physics object");
        }

        public void Add(PhysicsObject o)
        {
            if (o is IParticle) { particles.Add(o as IParticle); return; }
            if (o is ISurface) { surfaces.Add(o as ISurface); return; }
            if (o is IConstraint) { constraints.Add(o as IConstraint); return; }
            throw new Exception("unknown physics object");
        }

        public void Clear()
        {
            particles.Clear();
            surfaces.Clear();
            constraints.Clear();
        }

		public void TimeStep()
		{
            tick++;
			Verlet();
			SatisfyConstraints();
			CheckCollisions();
		}
	
	
		// TODO: Property of surface, not system
		public void SetSurfaceBounce( double kfr )
		{
			coeffRest = 1 + kfr;
		}
	
		// TODO: Property of surface, not system
		public void SetSurfaceFriction( double f )
		{
			coeffFric = f;
		}

		public void SetDamping( double d )
		{
			coeffDamp = d;
		}

		public void SetGravity( double gx, double gy )
		{
			gravity.X = gx;
			gravity.Y = gy;
		}

        public ISurface PickSurface(double x, double y, double tx, double ty)
        {
            RectangleParticle particle = new RectangleParticle(x - tx, y - ty, tx * 2, ty * 2);
            foreach (ISurface surface in Surfaces)
            {
                if (surface.IsRectangleColliding(particle))
                {
                    return surface;
                }
            }
            return null;
        }

		private void Verlet()
		{
			foreach ( Particle particle in particles )
			{
				particle.Verlet( this );
                particle.SetDirty();
			}
		}

		private void SatisfyConstraints()
		{
			foreach ( IConstraint constraint in constraints )
			{
				constraint.Resolve();
			}
		}

		private void CheckCollisions()
		{
			collisions.Clear();

			foreach ( ISurface surface in surfaces )
			{
				if ( surface.Active )
				{
					foreach ( Particle particle in particles )
					{
						particle.CheckCollision( surface, this );
					}
				}
			}

			Dictionary<string, object> compared = new Dictionary<string, object>();

			foreach ( Particle particle1 in particles )
			{
				foreach ( Particle particle2 in particles )
				{
					if ( particle1 != particle2 )
					{
						string key = ( particle1.Index < particle2.Index )
							? particle1.Index.ToString() + "," + particle2.Index.ToString()
							: particle2.Index.ToString() + "," + particle1.Index.ToString();

						if ( !compared.ContainsKey( key ) )
						{
							compared.Add( key, null );
							CollisionState state = new CollisionState();
							particle2.CheckCollision( particle1, this, ref state );

							if ( state.Colliding ) collisions.Add( state );
						}
					}
				}
			}
		}

		public Collection<IParticle> Particles { get { return particles; } }
		public Collection<ISurface> Surfaces { get { return surfaces; } }
		public Collection<IConstraint> Constraints { get { return constraints; } }
		public Collection<CollisionState> Collisions { get { return collisions; } }

		public Vector Gravity { get { return gravity; } }
		public double CoeffRest { get { return coeffRest; } }
		public double CoeffFric { get { return coeffFric; } }
		public double CoeffDamp { get { return coeffDamp; } }

        public string GetStats()
        {
            return String.Format("Physics: Gravity=<{5:00.0}, {6:00.0}> Total#={0:0000} [Partices#={1:0000} Surfaces#={2:0000} Constraints#={3:0000} Collisions#={4:0000}]",
                particles.Count+surfaces.Count+constraints.Count+collisions.Count,
                particles.Count,
                surfaces.Count,
                constraints.Count,
                collisions.Count,
                Gravity.X,
                Gravity.Y
                );
        }


        //#region INotifyPropertyChanged Members

        //public event PropertyChangedEventHandler PropertyChanged;

        //#endregion
	}
}