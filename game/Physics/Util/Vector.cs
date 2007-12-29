using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace Physics.Util
{
	/**
	 * Physics - 2D Dynamics Engine
	 * Release 0.1 alpha 
	 * Vector class
	 * Copyright 2007 Chris Cavanagh
	 * 
	 * Based on Flade source code by Alec Cove.
	 * 
	 * Physics is free software; you can redistribute it and/or modify
	 * it under the terms of the GNU General Public License as published by
	 * the Free Software Foundation; either version 2 of the License, or
	 * (at your option) any later version.
	 *
	 * Physics is distributed in the hope that it will be useful,
	 * but WITHOUT ANY WARRANTY; without even the implied warranty of
	 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	 * GNU General Public License for more details.
	 *
	 * You should have received a copy of the GNU General Public License
	 * along with Physics; if not, write to the Free Software
	 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
	 */
	public class Vector //: INotifyPropertyChanged
	{
		private static double atanZero = Math.Atan2( 0, 0 );
		private double x;
		private double y;

		public Vector( double px, double py )
		{
			x = px;
			y = py;
		}

		public Vector Clone()
		{
			return new Vector( x, y );
		}

		public void Set( double px, double py )
		{
			x = px;
			y = py;
		}

		public void Copy( Vector v )
		{
			x = v.x;
			y = v.y;
		}

		public double Dot( Vector v )
		{
			return x * v.x + y * v.y;
		}		

		public double Cross( Vector v )
		{
			return x * v.y - y * v.x;
		}

		public Vector Plus( Vector v )
		{
			x += v.x;
			y += v.y;
			return this;
		}

		public Vector PlusNew( Vector v )
		{
			return new Vector( x + v.x, y + v.y ); 
		}		

		public Vector Minus( Vector v )
		{
			x -= v.x;
			y -= v.y;
			return this;
		}

		public Vector MinusNew( Vector v )
		{
			return new Vector( x - v.x, y - v.y );
		}

		public Vector Mult( double s )
		{
			x *= s;
			y *= s;
			return this;
		}

		public Vector MultNew( double s )
		{
			return new Vector( x * s, y * s );
		}

		public double Distance( Vector v )
		{
			double dx = x - v.x;
			double dy = y - v.y;
			return Math.Sqrt( dx * dx + dy * dy );
		}

		public Vector Normalize()
		{
		   double mag = Math.Sqrt( x * x + y * y );
		   x /= mag;
		   y /= mag;
		   return this;
		}
		
		public double Magnitude()
		{
			return Math.Sqrt( x * x + y * y );
		}

		/**
		 * projects this vector onto b
		 */
		public Vector Project( Vector b )
		{
			double adotb = this.Dot( b );
			double len = ( b.x * b.x + b.y * b.y );

			Vector proj = new Vector( 0, 0 );
			proj.x = ( adotb / len ) * b.x;
			proj.y = ( adotb / len ) * b.y;
			return proj;
		}

		public Vector Rotate( double angle )
		{
			double sin = Math.Sin( angle );
			double cos = Math.Cos( angle );

			return new Vector( x * cos - y * sin, x * sin + y * cos );
		}

		public double Angle
		{
			get { return Math.Atan2( y, x ) - atanZero; }
		}

        //public void OnPropertyChanged( string propertyName )
        //{
        //    if ( PropertyChanged != null ) PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
        //}

		public double X { get { return x; } set { x = value; } }
		public double Y { get { return y; } set { y = value; } }

        //#region INotifyPropertyChanged Members

        //public event PropertyChangedEventHandler PropertyChanged;

        //#endregion
	}
}