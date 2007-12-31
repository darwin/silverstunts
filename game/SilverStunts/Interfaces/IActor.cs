using Physics.Util;

namespace SilverStunts
{
	public interface IActor
	{
		void ProcessInputs(bool[] keys);
		void Destroy();
		void Move(double x, double y);

		bool IsDead();
		Vector GetPos();
	}
}
