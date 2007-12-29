using System;
using System.Collections.Generic;
using System.Text;

using Physics;
using Physics.Primitives;
using Physics.Constraints;
using Physics.Base;

namespace Physics.Composites
{
    /**
     * Physics - 2D Dynamics Engine
     * Release 0.1 alpha 
     * SpringBox class
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
    public class SpringBox
    {
        private RectangleParticle p0;
        private RectangleParticle p1;
        private RectangleParticle p2;
        private RectangleParticle p3;

        public SpringBox(
            double px,
            double py,
            double w,
            double h,
            out PhysicsObject[] objs)
        {

            // top left
            p0 = new RectangleParticle(px - w / 2, py - h / 2, 1, 1);
            // top right
            p1 = new RectangleParticle(px + w / 2, py - h / 2, 1, 1);
            // bottom right
            p2 = new RectangleParticle(px + w / 2, py + h / 2, 1, 1);
            // bottom left
            p3 = new RectangleParticle(px - w / 2, py + h / 2, 1, 1);

            p0.Visible = false;
            p1.Visible = false;
            p2.Visible = false;
            p3.Visible = false;

            objs = new PhysicsObject[10];
            objs[0] = p0;
            objs[1] = p1;
            objs[2] = p2;
            objs[3] = p3;

            // edges
            objs[4] = new SpringConstraint(p0, p1);
            objs[5] = new SpringConstraint(p1, p2);
            objs[6] = new SpringConstraint(p2, p3);
            objs[7] = new SpringConstraint(p3, p0);

            // crossing braces
            objs[8] = new SpringConstraint(p0, p2);
            objs[9] = new SpringConstraint(p1, p3);
        }

        public RectangleParticle P0 { get { return p0; } }
        public RectangleParticle P1 { get { return p1; } }
        public RectangleParticle P2 { get { return p2; } }
        public RectangleParticle P3 { get { return p3; } }
    }
}