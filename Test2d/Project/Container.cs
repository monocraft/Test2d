﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Test2d
{
    /// <summary>
    /// 
    /// </summary>
    public class Container : ObservableObject
    {
        private string _name;
        private double _width;
        private double _height;
        private IList<KeyValuePair<string, ShapeProperty>> _database;
        private IList<Layer> _layers;
        private Container _template;
        private Layer _currentLayer;
        private Layer _workingLayer;
        private BaseShape _currentShape;

        /// <summary>
        /// 
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            {
                if (value != _name)
                {
                    _name = value;
                    Notify("Name");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public double Width
        {
            get { return _width; }
            set
            {
                if (value != _width)
                {
                    _width = value;
                    Notify("Width");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public double Height
        {
            get { return _height; }
            set
            {
                if (value != _height)
                {
                    _height = value;
                    Notify("Height");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public IList<KeyValuePair<string, ShapeProperty>> Database
        {
            get { return _database; }
            set
            {
                if (value != _database)
                {
                    _database = value;
                    Notify("Database");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public IList<Layer> Layers
        {
            get { return _layers; }
            set
            {
                if (value != _layers)
                {
                    _layers = value;
                    Notify("Layers");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Container Template
        {
            get { return _template; }
            set
            {
                if (value != _template)
                {
                    _template = value;
                    Notify("Template");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Layer CurrentLayer
        {
            get { return _currentLayer; }
            set
            {
                if (value != _currentLayer)
                {
                    _currentLayer = value;
                    Notify("CurrentLayer");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Layer WorkingLayer
        {
            get { return _workingLayer; }
            set
            {
                if (value != _workingLayer)
                {
                    _workingLayer = value;
                    Notify("WorkingLayer");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public BaseShape CurrentShape
        {
            get { return _currentShape; }
            set
            {
                if (value != _currentShape)
                {
                    _currentShape = value;
                    Notify("CurrentShape");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            foreach (var layer in Layers)
            {
                layer.Shapes.Clear();
            }
            WorkingLayer.Shapes.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Invalidate()
        {
            if (Template != null)
            {
                Template.Invalidate();
            }
            
            foreach (var layer in Layers)
            {
                layer.Invalidate();
            }
            WorkingLayer.Invalidate();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Container Create(string name = "Container", double width = 810, double height = 600)
        {
            var c = new Container()
            {
                Name = name,
                Width = width,
                Height = height,
                Layers = new ObservableCollection<Layer>()
            };

            c.Layers.Add(Layer.Create("Layer1"));
            c.Layers.Add(Layer.Create("Layer2"));
            c.Layers.Add(Layer.Create("Layer3"));
            c.Layers.Add(Layer.Create("Layer4"));

            c.CurrentLayer = c.Layers.FirstOrDefault();

            c.WorkingLayer = Layer.Create("Working");

            return c;
        }
    }
}