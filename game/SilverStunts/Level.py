# purpose of this file is to define shortcuts for some game objects to be visible in Python
import sys
import clr
ss = clr.LoadAssemblyByName("SilverStunts, Version=1.0.0.0")

# define shortcuts
Line = ss.SilverStunts.Entities.Line
Circle = ss.SilverStunts.Entities.Circle
Rectangle = ss.SilverStunts.Entities.Rectangle

# cleanup
del ss
del clr
del sys