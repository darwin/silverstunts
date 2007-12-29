using System;
using System.Collections.Generic;
using System.Text;

namespace Physics.Util
{
	/// <summary>
	/// Collision state
	/// </summary>
	public struct CollisionState
	{
		public Vector Depth;
		public Vector Point1;
		public Vector Point2;
		public bool Colliding;

		public Vector P1 { get { return Point1; } }
		public Vector P2 { get { return Point2; } }
	}

	public class Collision
	{
		/// <summary>
		/// Determine point on ellipse where ray intersects
		/// </summary>
		/// <param name="v">Center point</param>
		/// <param name="size">Size</param>
		/// <param name="angle">Rotation angle (radians)</param>
		/// <param name="rayAngle">Angle to project ray (radians)</param>
		/// <returns>Point of intersection</returns>
		public static Vector EllipsePoint( Vector v, Vector size, double angle, double rayAngle )
		{
			// Normalize the angle (remove the ellipse angle)
			double normalAngle = rayAngle - angle;

			// Determine intersection point, then rotate by the ellipse angle
			return new Vector( size.X / 2 * Math.Cos( normalAngle ), size.Y / 2 * Math.Sin( normalAngle ) ).Rotate( angle ).Plus( v );
		}

		/// <summary>
		/// Are ellipses colliding?
		/// </summary>
		/// <param name="v1">First ellipse center point</param>
		/// <param name="size1">First ellipse size</param>
		/// <param name="angle1">First ellipse angle</param>
		/// <param name="v2">Second ellipse center point</param>
		/// <param name="size2">Second ellipse size</param>
		/// <param name="angle2">Second ellipse angle</param>
		/// <param name="depth">Penetration depth (out)</param>
		/// <param name="point1">First ellipse intersection point</param>
		/// <param name="point2">Second ellipse intersection point</param>
		/// <returns>True if colliding</returns>
		public static bool IsEllipseCollision(
			Vector v1, Vector size1, double angle1,
			Vector v2, Vector size2, double angle2,
			out CollisionState state )
		{
			// Direction vector from first ellipse center to second ellipse center
			Vector direction = v2.MinusNew( v1 );
			double angle = direction.Angle;

			// Point of collision on first ellipse
			state.Point1 = EllipsePoint( v1, size1, angle1, angle );

			// Point of collision on second ellipse
			state.Point2 = EllipsePoint( v2, size2, angle2, angle + Math.PI );

			// Penetration depth
			state.Depth = state.Point2.MinusNew( state.Point1 );

			// If first ellipse radius (in direction of ray) + second ellipse radius is greater than the distance
			// between the two ellipses, we have a collision
			state.Colliding = ( v1.Distance( state.Point1 ) + v2.Distance( state.Point2 ) > v1.Distance( v2 ) );

			return state.Colliding;
		}
	}
}