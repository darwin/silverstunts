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
    public class Stats : Control
    {
        public ClipCanvas content;
        public bool Visible { get { timer.Begin(); return content.Visibility == Visibility.Visible; } set { content.Visibility = value ? Visibility.Visible : Visibility.Hidden; } }
        Storyboard timer;

        TextBlock pageStats;

        public Stats()
        {
            Stream s = this.GetType().Assembly.GetManifestResourceStream("SilverStunts.Stats.xaml");
            content = (ClipCanvas)this.InitializeFromXaml(new StreamReader(s).ReadToEnd());

            timer = content.FindName("timer") as Storyboard;
            pageStats = content.FindName("pageStats") as TextBlock;

            content.UpdateLayout();
        }

        public void Tick(object sender, EventArgs e)
        {
            pageStats.Text = Page.Current.GetStats();
            timer.Begin();
        }
    }
}
