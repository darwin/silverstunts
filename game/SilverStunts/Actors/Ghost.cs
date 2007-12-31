using Physics.Util;

namespace SilverStunts
{
	public class Ghost : IActor
	{
		public Ghost()
		{
		}

		public bool IsDead()
		{
			return true;
		}

		public void Destroy()
		{
		}

		public void Move(double x, double y)
		{
		}

		public Vector GetPos()
		{
			return new Vector(0,0);
		}

		public void ProcessInputs(bool[] keys)
		{

		}
	}
}
