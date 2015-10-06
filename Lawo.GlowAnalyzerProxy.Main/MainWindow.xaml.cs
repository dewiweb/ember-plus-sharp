﻿////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.GlowAnalyzerProxy.Main
{
    using System.Collections.Specialized;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>Represents the main window of the application.</summary>
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated through reflection.")]
    internal sealed partial class MainWindow : Window
    {
        //// [SetDataContext]
        public MainWindow()
        {
            this.InitializeComponent();
            this.DataContext = new MainWindowViewModel(Properties.Settings.Default);
            this.ViewModel.ScrollEventIntoView += this.OnScrollEventIntoView;
        }
        //// [SetDataContext]

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private MainWindowViewModel ViewModel
        {
            get { return (MainWindowViewModel)this.DataContext; }
        }

        private void OnScrollEventIntoView(object sender, ScrollEventIntoViewEventArgs e)
        {
            this.EventsDataGrid.ScrollIntoView(e.NewEvent);
        }

        private void OnSelectLogFolderButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.RootFolder = System.Environment.SpecialFolder.Desktop;
                dialog.SelectedPath = this.ViewModel.LogFolder;

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    this.ViewModel.LogFolder = dialog.SelectedPath;
                }
            }
        }

        private void OnDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            var column = (DataGridBoundColumn)e.Column;

            if ((e.PropertyType == typeof(int)) || (e.PropertyType == typeof(int?)) || (e.PropertyType == typeof(double)))
            {
                var setter = new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Right);
                column.CellStyle = new Style() { Setters = { setter } };
            }

            if ((e.PropertyType == typeof(int)) || (e.PropertyType == typeof(int?)))
            {
                column.Binding.StringFormat = "#,#";
            }
            else if (e.PropertyType == typeof(double))
            {
                column.Binding.StringFormat = "0.00s";
            }
        }
    }
}