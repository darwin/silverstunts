using Physics.Base;

namespace SilverStunts.Entities
{
	public class Generic : Entity
	{
		public Visual visual { get { return _visual; } }

		public Generic(PhysicsObject source, Visual.Family family, string selector)
		{
			_visual = new Visual(source, family, selector);

			Born();
		}
	}
}
