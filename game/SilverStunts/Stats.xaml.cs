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
using System.Windows.Browser;

namespace SilverStunts
{
    public class Stats : Control
    {
        public ClipCanvas content;
        HtmlTimer htmlTimer;
        
        public bool Visible
        { 
            get {
                if (htmlTimer == null) timer.Begin(); else if (!htmlTimer.Enabled) htmlTimer.Start();
                return content.Visibility == Visibility.Visible; 
            } 
            set {
                content.Visibility = value ? Visibility.Visible : Visibility.Collapsed; 
            } 
        }

        Storyboard timer;
        TextBlock pageStats;

        public Stats()
        {
            Stream s = this.GetType().Assembly.GetManifestResourceStream("SilverStunts.Stats.xaml");
            content = (ClipCanvas)this.InitializeFromXaml(new StreamReader(s).ReadToEnd());

            UseHTMLTimer();

            timer = content.FindName("timer") as Storyboard;
            pageStats = content.FindName("pageStats") as TextBlock;

            content.UpdateLayout();
        }

        void UseHTMLTimer()
        {
            htmlTimer = new HtmlTimer();
            htmlTimer.Interval = 1000;
            htmlTimer.Tick += new EventHandler(Tick);
        }

        public void Tick(object sender, EventArgs e)
        {
            pageStats.Text = Page.Current.GetStats();
            if (htmlTimer==null) timer.Begin();
            else if (!htmlTimer.Enabled) htmlTimer.Start();
        }
    }
}
