﻿namespace IndoorNavigator.MapEditor.Models.Nodes
{
    using System;
    using System.Diagnostics;

    [DebuggerDisplay("{" + nameof(X) + "}, {" + nameof(Y) + "}")]
    public abstract class NodeBase :
        IMapModel
    {
        public abstract NodeType Type { get; }

        public double X { get; set; }

        public double Y { get; set; }

        protected NodeBase(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double GetDistance(NodeBase other)
        {
            Debug.Assert(other != null);

            return Math.Sqrt(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2));
        }

        public override string ToString()
        {
            return $"{Type}({X}, {Y})";
        }
    }
}
