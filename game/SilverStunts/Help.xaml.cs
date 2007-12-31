using System.Windows;
using System.Windows.Controls;
using System.IO;

namespace SilverStunts
{
	public class Help : Control
	{
		public ClipCanvas content;
		public bool Visible { get { return content.Visibility == Visibility.Visible; } set { content.Visibility = value ? Visibility.Visible : Visibility.Collapsed; } }

		public Help(Canvas parent)
		{
			Stream s = this.GetType().Assembly.GetManifestResourceStream("SilverStunts.Help.xaml");
			content = (ClipCanvas)this.InitializeFromXaml(new StreamReader(s).ReadToEnd());

			content.UpdateLayout();
		}
	}
}
