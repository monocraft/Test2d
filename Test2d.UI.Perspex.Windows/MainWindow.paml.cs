﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Perspex.Controls;
using Perspex.Markup.Xaml;
using Test2d;

namespace TestPerspex
{
    /// <summary>
    /// 
    /// </summary>
    public class MainWindow : Window, IView
    {
        private EditorContext _context;

        /// <summary>
        /// 
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();
            App.AttachDevTools(this);
            
            this.InitializeContext();

            this.Closed += (sender, e) => _context.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitializeComponent()
        {
            PerspexXamlLoader.Load(this);
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitializeContext()
        {
            _context = new EditorContext()
            {
                View = this,
                Renderers = new IRenderer[] { new PerspexRenderer() },
                ProjectFactory = new ProjectFactory(),
                TextClipboard = new TextClipboard(),
                Serializer = new NewtonsoftSerializer(),
                PdfWriter = new PdfWriter(),
                DxfWriter = new DxfWriter(),
                CsvReader = new CsvHelperReader(),
                CsvWriter = new CsvHelperWriter()
            };

            _context.InitializeEditor(new TraceLog());

            _context.Editor.Renderers[0].State.DrawShapeState = ShapeState.Visible;
            _context.Editor.GetImageKey = async () => await OnGetImageKey();

            _context.Commands.OpenCommand =
                Command<object>.Create(
                    async (parameter) => await OnOpen(parameter),
                    (parameter) => _context.IsEditMode());

            _context.Commands.SaveCommand =
                Command.Create(
                    async () => await OnSave(),
                    () => _context.IsEditMode());

            _context.Commands.SaveAsCommand =
                Command.Create(
                    async () => await OnSaveAs(),
                    () => _context.IsEditMode());

            _context.Commands.ExportCommand =
                Command<object>.Create(
                    async (item) => await OnExport(),
                    (item) => _context.IsEditMode());

            // TODO: Initialize other commands.

            DataContext = _context;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task<string> OnGetImageKey()
        {
            var dlg = new OpenFileDialog();
            dlg.Filters.Add(new FileDialogFilter() { Name = "All", Extensions = { "*" } });
            var result = await dlg.ShowAsync(this);
            if (result != null)
            {
                var path = result.FirstOrDefault();
                var bytes = System.IO.File.ReadAllBytes(path);
                var key = _context.Editor.Project.AddImageFromFile(path, bytes);
                return key;
            }
            return null;
        }
        
        /// <summary>
        /// 
        /// </summary>
        private async Task OnOpen(object parameter)
        {
            if (parameter == null)
            {
                var dlg = new OpenFileDialog();
                dlg.Filters.Add(new FileDialogFilter() { Name = "Project", Extensions = { "project" } });
                dlg.Filters.Add(new FileDialogFilter() { Name = "All", Extensions = { "*" } });
                var result = await dlg.ShowAsync(this);
                if (result != null)
                {
                    var path = result.FirstOrDefault();
                    _context.Open(path);
                    if (_context.Invalidate != null)
                    {
                        _context.Invalidate();
                    }
                }
            }
            else
            {
                string path = parameter as string;
                if (path != null && System.IO.File.Exists(path))
                {
                    _context.Open(path);
                }
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task OnSave()
        {
            if (!string.IsNullOrEmpty(_context.Editor.ProjectPath))
            {
                _context.Save(_context.Editor.ProjectPath);
            }
            else
            {
                await OnSaveAs();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task OnSaveAs()
        {
            var dlg = new SaveFileDialog();
            dlg.Filters.Add(new FileDialogFilter() { Name = "Project", Extensions = { "project" } });
            dlg.Filters.Add(new FileDialogFilter() { Name = "All", Extensions = { "*" } });
            dlg.InitialFileName = _context.Editor.Project.Name + ".project";
            var result = await dlg.ShowAsync(this);
            if (result != null)
            {
                _context.Save(result);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task OnExport()
        {
            var dlg = new SaveFileDialog();
            dlg.Filters.Add(new FileDialogFilter() { Name = "Pdf", Extensions = { "pdf" } });
            dlg.Filters.Add(new FileDialogFilter() { Name = "Dxf", Extensions = { "dxf" } });
            dlg.Filters.Add(new FileDialogFilter() { Name = "All", Extensions = { "*" } });
            dlg.InitialFileName = _context.Editor.Project.Name + ".pdf";
            var result = await dlg.ShowAsync(this);
            if (result != null)
            {
                var ext = System.IO.Path.GetExtension(result).ToLower();

                if (ext == ".pdf")
                {
                    _context.ExportAsPdf(result, _context.Editor.Project);
                    Process.Start(result);
                }

                if (ext == ".dxf")
                {
                    _context.ExportAsDxf(result);
                    Process.Start(result);
                }
            }
        }
    }
}
