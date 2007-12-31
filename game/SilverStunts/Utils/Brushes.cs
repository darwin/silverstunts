// taken from MS SilverLight SDK 1.1
using System.Windows.Media;

namespace SilverStunts 
{
	public class Brushes 
	{
		public static SolidColorBrush White
		{
			get
			{
				SolidColorBrush sb = new SolidColorBrush();
				sb.Color = Colors.White;
				return sb;
			}
		}

		public static SolidColorBrush Black
		{
			get {
				SolidColorBrush sb = new SolidColorBrush();
				sb.Color = Colors.Black;
				return sb;
			}
		}

		public static SolidColorBrush Blue {
			get {
				SolidColorBrush sb = new SolidColorBrush();
				sb.Color = Colors.Blue;
				return sb;
			}
		}

		public static SolidColorBrush Yellow
		{
			get
			{
				SolidColorBrush sb = new SolidColorBrush();
				sb.Color = Colors.Yellow;
				return sb;
			}
		}

		public static SolidColorBrush Orange
		{
			get
			{
				SolidColorBrush sb = new SolidColorBrush();
				sb.Color = Colors.Orange;
				return sb;
			}
		}

		public static SolidColorBrush DarkGray
		{
			get {
				SolidColorBrush sb = new SolidColorBrush();
				sb.Color = Colors.DarkGray;
				return sb;
			}
		}

		public static SolidColorBrush Gray {
			get {
				SolidColorBrush sb = new SolidColorBrush();
				sb.Color = Colors.Gray;
				return sb;
			}
		}

		public static SolidColorBrush Green {
			get {
				SolidColorBrush sb = new SolidColorBrush();
				sb.Color = Colors.Green;
				return sb;
			}
		}

		public static SolidColorBrush LightGray {
			get {
				SolidColorBrush sb = new SolidColorBrush();
				sb.Color = Colors.LightGray;
				return sb;
			}
		}

		public static SolidColorBrush Red {
			get {
				SolidColorBrush sb = new SolidColorBrush();
				sb.Color = Colors.Red;
				return sb;
			}
		}

		public static SolidColorBrush Transparent {
			get {
				SolidColorBrush sb = new SolidColorBrush();
				sb.Color = Colors.Transparent;
				return sb;
			}
		}

		public static SolidColorBrush FromColor(Color color) {
			SolidColorBrush sb = new SolidColorBrush();
			sb.Color = color;
			return sb;
		}
	}
}
