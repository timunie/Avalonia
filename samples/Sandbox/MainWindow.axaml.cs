using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Win32.WinRT.Composition;
using Sandbox.Model;

namespace Sandbox
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            this.AttachDevTools();

            AddHandler(DragDrop.DropEvent, DoDrop);
        }

        private void DoDrop(object sender, DragEventArgs e)
        {
            if(e.Data.GetFileNames() is { } files && DataContext is MainViewModel viewModel)
            {
                foreach (var file in files)
                {
                    viewModel.FileItems.Add(new FileItem(file));
                }
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
