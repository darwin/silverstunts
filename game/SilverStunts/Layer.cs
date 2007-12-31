using System;
using System.Windows.Controls;

namespace SilverStunts
{
	public class Layer : Control
	{
		public Layer(string source, string type)
		{
			try
			{
				this.InitializeFromXaml(source);
			}
			catch (Exception e)
			{
				Page.Current.PrintConsole("Bad "+type+" XAML: "+e.ToString()+"\n", Page.ConsoleOutputKind.Exception);
			}
		}
	}
}
