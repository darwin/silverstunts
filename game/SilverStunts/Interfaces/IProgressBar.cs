
namespace SilverStunts
{
	public interface IProgressBar
	{
		void Changed(double percentage);
		void Failed(double percentage);
		void Completed(double percentage);
	}
}
