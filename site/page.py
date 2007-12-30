# this is python code driving our silverlight control
# it acts as a scripting DLR bootstrapper (I was unable to intialize python scripting engine from managed code)
# we load managed assembly and route all relevant actions to it

import sys, clr

SilverStunts = clr.LoadAssemblyByName("SilverStunts, Version=1.0.0.0")

####################################################################
# class suitable for output redirect
class Redirect:
    def __init__(self, kind):
        self.method = SilverStunts.SilverStunts.Page.Current.PrintConsole
        self.kind = kind

    def write(self, s):
        self.method(s, self.kind)

####################################################################

def onLoaded(sender, args):
    # bootstrap page
    global page
    page = SilverStunts.CreateInstance("SilverStunts.Page")
    page.Init(sender.Parent)
    # redirect standard outputs
    sys.stdout = Redirect(SilverStunts.SilverStunts.Page.ConsoleOutputKind.Output)
    sys.stderr = Redirect(SilverStunts.SilverStunts.Page.ConsoleOutputKind.Error)