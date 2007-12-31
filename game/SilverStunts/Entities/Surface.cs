using System;

using Physics.Surfaces;

namespace SilverStunts.Entities
{
	public class Surface : Entity
	{
		public event EventHandler onContact
		{
			add { (_visual.source as AbstractSurface).Contact += value; }
			remove { (_visual.source as AbstractSurface).Contact -= value; }
		}

		public bool active
		{
			get { return (_visual.source as ISurface).Active; }
			set { (_visual.source as ISurface).Active = value; }
		}
	}
}
