using System;
using System.Windows.Controls;
using System.Collections.Generic;

using Physics.Composites;
using Physics.Constraints;
using Physics.Primitives;
using Physics.Util;
using Physics.Base;
using SilverStunts.Entities;

namespace SilverStunts
{
	// actors should be implemented in script
	public class Bike : IActor
	{
		protected Physics.Engine physics;
		protected Canvas world;

		protected Wheel wheelA;
		protected Wheel wheelB;
		protected AngularConstraint sBikeAngular;
		protected double angDefault;

		protected CircleParticle personHead;
		protected CircleParticle personBody;
		protected CircleParticle personLegs;

		protected SpringConstraint sHeadToWheelA;
		protected SpringConstraint sHeadToWheelB;
		protected SpringConstraint sLegsToWheelB;
		protected SpringConstraint sLegsToWheelA;

		protected SpringConstraint sHeadToBody;
		protected SpringConstraint sBodyToLegs;
		protected AngularConstraint aPose;

		int airTimeout = 0;
		Vector jumpImpulse = new Vector(0, 0);
		double jumpCharge = 0;
		bool lastRight = true;
		bool dead = false;

		List<Generic> parts = new List<Generic>();
		Level level;

		public Bike(Level level, double x, double y)
		{
			this.level = level;
			this.physics = level.physics;

			Create(x, y);
			dead = false;
		}

		public bool IsDead()
		{
			return dead;
		}

		public Generic Add(PhysicsObject o)
		{
			return Add(o, null);
		}

		public Generic Add(PhysicsObject o, string selector)
		{
			Generic g = new Generic(o, Visual.Family.Bike, selector);
			parts.Add(g);
			return g;
		}

		public void Remove(Generic g)
		{
			g.Destroy();
			parts.Remove(g);
		}

		public void Remove(PhysicsObject o)
		{
			foreach (Generic e in parts)
			{
				if (ReferenceEquals(e.visual.source, o))
				{
					e.Destroy();
					parts.Remove(e);
					return;
				}
			}
		}

		public void Destroy()
		{
			foreach (Generic e in parts)
			{
				e.Destroy();
			}
			parts.Clear();
		}

		public void Move(double x, double y)
		{
			Destroy();
			Create(x, y);
		}

		public Vector GetPos()
		{
			return personHead.Curr;
		}

		public void ProcessInputs(bool[] keys)
		{
			if (personHead.Curr.Y >= 1000) Kill();
			double keySpeed = 0.2;
			if (IsDead())
			{
				wheelA.RP.Decelerate(keySpeed);
				wheelB.RP.Decelerate(keySpeed);
				return;
			}

			bool keyLeft = keys[30]; // A
			bool keyRight = keys[33]; // D
			bool keyJump = keys[48]; // S
			bool keyStunt = keys[52]; // W

			if (keyLeft)
			{
				if (lastRight)
				{
					wheelA.RP.Decelerate(keySpeed);
					wheelB.RP.Decelerate(keySpeed);
				}
				wheelA.RP.VS -= keySpeed;
				wheelB.RP.VS -= keySpeed;
			}
			else if (keyRight)
			{
				if (!lastRight)
				{
					wheelA.RP.Decelerate(keySpeed);
					wheelB.RP.Decelerate(keySpeed);
				}
				wheelA.RP.VS += keySpeed;
				wheelB.RP.VS += keySpeed;
			}
			else
			{
				wheelA.RP.VS /= 2;
				wheelB.RP.VS /= 2;
			}

			Wheel primaryWheel = wheelA;
			Wheel secondaryWheel = wheelB;

			double deltaX = lastRight ? 1 : -1;

			double dirX = 1.0;
			if (deltaX < 0)
			{
				dirX = -1.0;
				primaryWheel = wheelB;
				secondaryWheel = wheelA;
			}

			if (primaryWheel.lastNormal!=null)
			{
				airTimeout = 0;
				if (keyStunt)
				{
					secondaryWheel.Curr.Y -= 2.0;
					secondaryWheel.Curr.X -= dirX * 0.5;
					personBody.Curr.X -= dirX;
					personHead.Curr.X += dirX;
				}
			}
			else if (secondaryWheel.lastNormal!=null)
			{
				airTimeout = 0;
				if (keyStunt)
				{
					primaryWheel.Curr.Y -= 2.0;
					primaryWheel.Curr.X -= dirX / 2;
					personBody.Curr.X -= dirX;
					personHead.Curr.X += dirX;
				}
			}
			else
			{
				// in air
				airTimeout++;

				if (airTimeout > 10)
				{
					Vector r = wheelB.Curr.MinusNew(wheelA.Curr);

					if (keyRight)
					{
						r = r.Rotate(((2 * Math.PI) / 360) * 10);
						r.Normalize();
						r.Mult(5);

						wheelA.Curr.X -= r.X;
						wheelA.Curr.Y -= r.Y;
						wheelB.Curr.X += r.X;
						wheelB.Curr.Y += r.Y;

						//personBody.Curr.X += dirX;
						//personHead.Curr.X += 2;
					}
					if (keyLeft)
					{
						r = r.Rotate(-((2 * Math.PI) / 360) * 10);
						r.Normalize();
						r.Mult(5);

						wheelA.Curr.X -= r.X;
						wheelA.Curr.Y -= r.Y;
						wheelB.Curr.X += r.X;
						wheelB.Curr.Y += r.Y;

						//personBody.Curr.X -= dirX;
						//personHead.Curr.X -= 2;
					}
				}
			}

			if (jumpCharge > 0 && !keyJump)
			{
				if (primaryWheel.lastNormal != null)// && (physics.tick - primaryWheel.lastlastNormalTick) < 2)
				{
					Vector n = primaryWheel.lastNormal;

					jumpImpulse.X = n.X * jumpCharge;
					jumpImpulse.Y = n.Y * jumpCharge;
				}
				jumpCharge = 0;
			}

			if (keyJump)
			{
				if (primaryWheel.lastNormal != null)
				{
					if (jumpCharge == 0) jumpCharge = 8; // initial charge
					if (jumpCharge < 25) jumpCharge += 0.5;

					personBody.Curr.Y -= primaryWheel.lastNormal.Y * jumpCharge * 0.1;
					personHead.Curr.Y -= primaryWheel.lastNormal.Y * jumpCharge * 0.1;
					personBody.Curr.X -= primaryWheel.lastNormal.X * jumpCharge * 0.1;
					personHead.Curr.X -= primaryWheel.lastNormal.X * jumpCharge * 0.1;
					personHead.Curr.X += dirX;
					personBody.Curr.X -= dirX;
				}
				//primaryWheel.Curr.X += dirX * jumpCharge * 0.15;
			}

			if (jumpImpulse.X != 0 || jumpImpulse.Y != 0)
			{
				primaryWheel.Curr.X += jumpImpulse.X;
				primaryWheel.Curr.Y += jumpImpulse.Y;
				secondaryWheel.Curr.X += jumpImpulse.X;
				secondaryWheel.Curr.Y += jumpImpulse.Y;

				jumpImpulse.X = jumpImpulse.X / 2;
				jumpImpulse.Y = jumpImpulse.Y / 2;

				if (Math.Abs(jumpImpulse.X) < 1) jumpImpulse.X = 0;
				if (Math.Abs(jumpImpulse.Y) < 1) jumpImpulse.Y = 0;
			}

			if (keyLeft) lastRight = false;
			if (keyRight) lastRight = true;

			primaryWheel.lastNormal = null;
			secondaryWheel.lastNormal = null;
		}

		public void Kill()
		{
			Remove(sHeadToWheelA);
			Remove(sHeadToWheelB);
			Remove(sBikeAngular);
			Remove(sLegsToWheelA);
			Remove(sLegsToWheelB);
			Remove(sBikeAngular);

			aPose.TargetTheta = -Math.PI + 0.1;
			dead = true;
		}

		protected void Create(double x, double y)
		{
			// create the bicycle
			double leftX = x - 18;
			double rightX = x + 18;
			double widthX = rightX - leftX;
			double midX = leftX + (widthX / 2);
			double topY = y + 0;

			// wheels
			wheelA = new Wheel(leftX, topY, 12);
			Add(wheelA, "wheelA");

			wheelB = new Wheel(rightX, topY, 12);
			Add(wheelB, "wheelB");

			// body
			PhysicsObject[] objs;
			SpringBox rectA = new SpringBox(midX, topY, widthX, 15, out objs);
			foreach (PhysicsObject o in objs)
			{
				Add(o);
			}

			// wheel struts
			SpringConstraint conn1 = new SpringConstraint(wheelA, rectA.P3);
			Add(conn1);

			SpringConstraint conn2 = new SpringConstraint(wheelB, rectA.P2);
			Add(conn2);

			SpringConstraint conn1a = new SpringConstraint(wheelA, rectA.P0);
			Add(conn1a);

			SpringConstraint conn2a = new SpringConstraint(wheelB, rectA.P1);
			Add(conn2a);

			// triangle top of car
			personHead = new CircleParticle(midX, topY - 30, 5);
			Add(personHead);

			personHead.Contact += delegate(object sender, EventArgs e)
			{
				Kill();
			};

			sHeadToWheelA = new SpringConstraint(personHead, wheelA);
			Add(sHeadToWheelA);

			sHeadToWheelB = new SpringConstraint(personHead, wheelB);
			Add(sHeadToWheelB);

			// angular constraint for triangle top
			sBikeAngular = new AngularConstraint(wheelA, personHead, wheelB);
			Add(sBikeAngular);

			angDefault = sBikeAngular.TargetTheta;

			personBody = new CircleParticle(midX - 5, topY - 20, 5);
			personLegs = new CircleParticle(midX, topY - 5, 5);
			aPose = new AngularConstraint(personHead, personBody, personLegs);

			sHeadToBody = new SpringConstraint(personHead, personBody);
			sHeadToBody.RestLength = 12;
			Add(sHeadToBody);
			sBodyToLegs = new SpringConstraint(personBody, personLegs);
			sBodyToLegs.RestLength = 12;
			Add(sBodyToLegs);

			Add(personBody);
			Add(personLegs);

			sLegsToWheelB = new SpringConstraint(personLegs, wheelB);
			sLegsToWheelB.RestLength = 20;
			Add(sLegsToWheelB);

			sLegsToWheelA = new SpringConstraint(personLegs, wheelA);
			sLegsToWheelA.RestLength = 20;
			Add(sLegsToWheelA);
		}
	}
}
