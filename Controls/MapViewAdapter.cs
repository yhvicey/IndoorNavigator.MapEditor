﻿namespace IndoorNavigator.MapEditor.Controls
{
    using System.Linq;
    using System.Windows.Forms;
    using Models;
    using Models.Nodes;
    using Share;

    public class MapViewAdapter
    {
        private readonly TreeView _mapView;

        private MapElementTreeNode _root;

        public Map Map
        {
            set
            {
                _mapView.Nodes.Clear();
                if (value == null) return;
                _root = new MapElementTreeNode(value.Name, mapElement: value);
                var index = 0;
                foreach (var floor in value.Floors)
                {
                    AddFloor(index++, floor);
                }
                _mapView.Nodes.Add(_root);
                Flush();
            }
        }

        public MapViewAdapter(TreeView mapView)
        {
            Contract.EnsureArgsNonNull(mapView);
            _mapView = mapView;
        }

        public void AddFloor(int index, Floor floor)
        {
            var floorNode = new MapElementTreeNode($"Floor {index + 1}", mapElement: floor);
            var entryNodes =
                floor.EntryNodes.Select(
                    entryNode =>
                        new MapElementTreeNode(null,
                            entryNode.Links.Select(link => new MapElementTreeNode(null, mapElement: link)),
                            entryNode));
            var guideNodes =
                floor.GuideNodes.Select(
                    guideNode =>
                        new MapElementTreeNode(null,
                            guideNode.Links.Select(link => new MapElementTreeNode(null, mapElement: link)),
                            guideNode));
            var wallNodes =
                floor.WallNodes.Select(
                    wallNode =>
                        new MapElementTreeNode(null,
                            wallNode.Links.Select(link => new MapElementTreeNode(null, mapElement: link)),
                            wallNode));
            floorNode.Nodes.Add(new MapElementTreeNode("Entry nodes", entryNodes.ToArray()));
            floorNode.Nodes.Add(new MapElementTreeNode("Guide nodes", guideNodes.ToArray()));
            floorNode.Nodes.Add(new MapElementTreeNode("Wall nodes", wallNodes.ToArray()));
            _root.Nodes.Insert(index, floorNode);
            Flush();
        }

        public void AddNode(int index, NodeBase node)
        {
            _root.Nodes[index].Nodes[(int)node.Type].Nodes.Add(new MapElementTreeNode(null,
                node.Links.Select(link => new MapElementTreeNode(null, mapElement: link)), node));
            Flush();
        }

        public void Flush()
        {
            _root.Update();
        }

        public void RemoveFloor(int index)
        {
            _root.Nodes.RemoveAt(index);
            Flush();
        }

        public void RemoveNode(NodeType type, int floor, int index)
        {
            _root.Nodes[floor].Nodes[(int)type].Nodes.RemoveAt(index);
            Flush();
        }
    }
}