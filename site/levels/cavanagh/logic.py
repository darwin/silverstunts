# if the car touches the right rectangle then turn off the upward ramp
def patriciaContact(sender, args):
	maria.active = False

# if the car touches the right rectangle then turn on the upward ramp
def susanContact(sender, args):
	maria.active = True

# register events
patricia.onContact += patriciaContact
susan.onContact += susanContact

###################################################

def init():
	maria.active = False
    
def tick(t, e):
	1
		