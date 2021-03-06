﻿namespace IndoorNavigator.MapEditor.Map
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Xml;
    using Models;
    using Models.Nodes;
    using Properties;

    public static class MapParser
    {
        private const string AttrEndIndex = "EndIndex";
        private const string AttrName = "Name";
        private const string AttrNext = "Next";
        private const string AttrPrev = "Prev";
        private const string AttrStartIndex = "StartIndex";
        private const string AttrType = "Type";
        private const string AttrVersion = "Version";
        private const string AttrX = "X";
        private const string AttrY = "Y";
        private const string ElementMap = "Map";
        private const string ElementFloor = "Floor";
        private const string ElementLink = "Link";
        private const string SupportedVersion = "1.1";

        private static Link GenerateLink(XmlElement element)
        {
            Debug.Assert(element != null);

            if (!Enum.TryParse(element.GetAttribute(AttrType), out NodeType type)) throw new Exception(Resources.UnexpectedTypeError);
            if (!int.TryParse(element.GetAttribute(AttrStartIndex), out var startIndex)) throw new Exception(Resources.InvalidElementError);
            if (!int.TryParse(element.GetAttribute(AttrEndIndex), out var endIndex)) throw new Exception(Resources.InvalidElementError);

            return new Link(type, startIndex, endIndex);
        }

        private static NodeBase GenerateNode(XmlElement element)
        {
            Debug.Assert(element != null);

            if (!Enum.TryParse(element.GetAttribute(AttrType), out NodeType type)) throw new Exception(Resources.UnexpectedTypeError);
            if (!int.TryParse(element.GetAttribute(AttrX), out var x)) throw new Exception(Resources.InvalidElementError);
            if (!int.TryParse(element.GetAttribute(AttrY), out var y)) throw new Exception(Resources.InvalidElementError);

            switch (type)
            {
                case NodeType.GuideNode:
                {
                    var name = element.GetAttribute(AttrName);
                    var prev = int.TryParse(element.GetAttribute(AttrPrev), out var prevEntry)
                        ? prevEntry
                        : (int?)null;
                    var next = int.TryParse(element.GetAttribute(AttrNext), out var nextEntry)
                        ? nextEntry
                        : (int?)null;
                    return new GuideNode(x, y, string.IsNullOrEmpty(name) ? null : name, prev, next);
                }
                case NodeType.WallNode:
                {
                    return new WallNode(x, y);
                }
                default:
                {
                    throw new Exception(Resources.InvalidElementError);
                }
            }
        }

        public static Map Parse(string filename)
        {
            Debug.Assert(filename != null);

            var doc = new XmlDocument();
            doc.Load(filename);
            var root = doc.DocumentElement;
            Debug.Assert(root != null);
            if (root.Name != ElementMap) throw new Exception(Resources.InvalidMapFileError);

            var version = root.GetAttribute(AttrVersion);
            if (version != SupportedVersion) throw new Exception(Resources.UnsupportedVersionError);
            var name = root.GetAttribute(AttrName);

            var floors = new List<Floor>();
            var floorElements = root.SelectNodes(ElementFloor)?.OfType<XmlElement>();
            if (floorElements == null) return new Map(name, floors);
            foreach (var floorElement in floorElements)
            {
                var floor = new Floor();

                var entryNodeElements = floorElement.SelectNodes($"Node[@{AttrType}='{NodeType.GuideNode}']")?.OfType<XmlElement>();
                Debug.Assert(entryNodeElements != null);
                floor.AddNodes(entryNodeElements.Select(GenerateNode));

                var wallNodeElements = floorElement.SelectNodes($"Node[@{AttrType}='{NodeType.WallNode}']")?.OfType<XmlElement>();
                Debug.Assert(wallNodeElements != null);
                floor.AddNodes(wallNodeElements.Select(GenerateNode));

                var linkElements = floorElement.SelectNodes(ElementLink)?.OfType<XmlElement>();
                Debug.Assert(linkElements != null);
                floor.AddLinks(linkElements.Select(GenerateLink));

                floors.Add(floor);
            }

            return new Map(name, floors);
        }
    }
}
