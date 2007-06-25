using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO;

namespace SilverStunts
{
    public class Help : Control
    {
        public ClipCanvas content;
        public bool Visible { get { return content.Visibility == Visibility.Visible; } set { content.Visibility = value ? Visibility.Visible : Visibility.Hidden; } }

        TextBlock pageHelp;

        public Help()
        {
            Stream s = this.GetType().Assembly.GetManifestResourceStream("SilverStunts.Help.xaml");
            content = (ClipCanvas)this.InitializeFromXaml(new StreamReader(s).ReadToEnd());

            pageHelp = content.FindName("pageHelp") as TextBlock;
            content.UpdateLayout();
        }
    }
}
