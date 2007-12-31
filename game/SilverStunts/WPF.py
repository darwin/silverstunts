#///////////////////////////////////////////////////////////////////////////////
#//
#//  wpf.py
#//
#// 
#// © 2007 Microsoft Corporation. All Rights Reserved.
#//
#// This file is licensed as part of the Silverlight 1.1 SDK, for details look here: http://go.microsoft.com/fwlink/?LinkID=89145&clcid=0x409
#//
#///////////////////////////////////////////////////////////////////////////////

from System import *
from System.Windows import *
from System.Windows.Controls import *
from System.Windows.Media import *
from System.Windows.Documents import *
from System.Windows.Shapes import *
from System.Windows.Input import *
from System.Windows.Media.Animation import *
from System.Windows.Interop import *


#work-around for generic method perf bug
SetIntValue = DependencyObject.SetValue.Template[int]
def SetPosition(o, x, y):
	SetIntValue(o, Canvas.TopProperty, y)
	SetIntValue(o, Canvas.LeftProperty, x)
	
def GetPosition(o):
	return o.GetValue(Canvas.LeftProperty), o.GetValue(Canvas.TopProperty)


def LoadXaml(url):
	uri = MakeUri(url)
	request = System.Windows.Browser.Net.BrowserHttpWebRequest(uri)
	response = request.GetResponse()
	reader = System.IO.StreamReader(response.GetResponseStream())
	xaml = reader.ReadToEnd ()
	reader.Close()
	return XamlReader.Load(xaml)

def MakeUri(url):
	return System.Uri(System.Windows.Browser.HtmlPage.DocumentUri, url)
	
def __Setup():
	for name in dir(Colors):
		c = getattr(Colors, name)
		if isinstance(c, Color):
			bname = name+"Brush"
			globals()[bname] = SolidColorBrush(c)
	
#__Setup()