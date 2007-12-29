import sys
import clr
ss = clr.LoadAssemblyByName("SilverStunts, Version=1.0.0.0")

Line = ss.SilverStunts.Entities.Line
Circle = ss.SilverStunts.Entities.Circle
Rectangle = ss.SilverStunts.Entities.Rectangle

del ss
del clr
del sys