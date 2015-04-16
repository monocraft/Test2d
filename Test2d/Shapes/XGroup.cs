﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test2d
{
    public class XGroup : BaseShape
    {
        private string _name;
        private IList<BaseShape> _shapes;

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

        public IList<BaseShape> Shapes
        {
            get { return _shapes; }
            set
            {
                if (value != _shapes)
                {
                    _shapes = value;
                    Notify("Shapes");
                }
            }
        }

        public override void Draw(object dc, IRenderer renderer, double dx, double dy)
        {
            foreach (var shape in Shapes)
            {
                shape.Draw(dc, renderer, dx, dy);
            }
        }

        public static XGroup Create(string name)
        {
            return new XGroup()
            {
                Name = name,
                Shapes = new ObservableCollection<BaseShape>()
            };
        }
    }
}