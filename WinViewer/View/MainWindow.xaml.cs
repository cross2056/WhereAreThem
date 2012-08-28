﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WhereAreThem.Model.Models;
using WhereAreThem.WinViewer.Event;
using WhereAreThem.WinViewer.Model;
using WhereAreThem.WinViewer.ViewModel;

namespace WhereAreThem.WinViewer.View {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private TreeViewItem _selectedTreeViewItem;
        private SearchWindow _searchWindow;

        public MainWindowViewModel VM { get; private set; }
        public SearchWindow SearchWindow {
            get {
                if (_searchWindow == null) {
                    _searchWindow = new SearchWindow();
                    _searchWindow.Owner = this;
                    _searchWindow.VM.LocatingItem += OnLocatingItem;
                    _searchWindow.Closing += (s, e) => {
                        ((Window)s).Hide();
                        KeyDown += Window_KeyDown;
                        e.Cancel = true;
                    };
                }
                return _searchWindow;
            }
        }

        public MainWindow() {
            InitializeComponent();

            KeyDown += Window_KeyDown;

            VM = new MainWindowViewModel();
            VM.View = this;
            VM.OpeningProperties += new OpeningPropertiesEventHandler(OnOpeningProperties);
            DataContext = VM;
        }

        private void OnOpeningProperties(object sender, OpeningPropertiesEventArgs e) {
            List<Folder> stack = new List<Folder>(VM.SelectedFolderStack);
            stack.Add(VM.SelectedFolder);
            PropertiesWindow window = new PropertiesWindow(VM.SelectedItem, stack);
            window.Owner = this;
            window.Show();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e) {
            if ((e.Key == Key.F) && (Keyboard.Modifiers == ModifierKeys.Control)) {
                if ((VM.SelectedFolder != null) && !(VM.SelectedFolder is Computer)) {
                    SearchWindow.VM.Root = VM.SelectedFolder;
                    SearchWindow.VM.RootStack = VM.SelectedFolderStack;
                    SearchWindow.Show();
                    KeyDown -= Window_KeyDown;
                }
                e.Handled = true;
            }
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            DataGrid dataGrid = (DataGrid)sender;
            FileSystemItem item = (FileSystemItem)dataGrid.SelectedItem;

            if (item is Folder) {
                _selectedTreeViewItem.IsExpanded = true;
                _selectedTreeViewItem.UpdateLayout();
                _selectedTreeViewItem = (TreeViewItem)_selectedTreeViewItem.ItemContainerGenerator.ContainerFromItem(item);
                _selectedTreeViewItem.IsSelected = true;
            }
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (VM.SelectedItem != null)
                ((DataGrid)sender).ScrollIntoView(VM.SelectedItem);
        }

        private void FolderTree_Selected(object sender, RoutedEventArgs e) {
            _selectedTreeViewItem = (TreeViewItem)e.OriginalSource;

            TreeView treeView = (TreeView)sender;
            LoadDrive(treeView.SelectedItem);
            VM.SelectedFolder = (Folder)treeView.SelectedItem;
            List<Folder> stack = new List<Folder>();
            GetFolderStack(_selectedTreeViewItem, stack);
            VM.SelectedFolderStack = stack;
        }

        private void FolderTree_Expanded(object sender, RoutedEventArgs e) {
            TreeViewItem treeViewItem = (TreeViewItem)e.OriginalSource;
            LoadDrive(treeViewItem.Header);
        }

        private void OnLocatingItem(object sender, LocatingItemEventArgs e) {
            TreeViewItem treeViewItem = GetRootTreeViewItem(_selectedTreeViewItem);
            for (int i = 1; i < e.Result.Stack.Count; i++) {
                treeViewItem.IsExpanded = true;
                treeViewItem.UpdateLayout();
                treeViewItem = (TreeViewItem)treeViewItem.ItemContainerGenerator.ContainerFromItem(e.Result.Stack[i]);
            }
            treeViewItem.IsSelected = true;
            VM.SelectedItem = e.Result.Item;
        }

        private void LoadDrive(object item) {
            if (item is Drive)
                ((Drive)item).Load();
        }

        private void GetFolderStack(TreeViewItem item, List<Folder> stack) {
            TreeViewItem parent = GetParentTreeNode(item);
            if (parent != null) {
                stack.Insert(0, (Folder)parent.Header);
                GetFolderStack(parent, stack);
            }
        }

        private TreeViewItem GetRootTreeViewItem(TreeViewItem item) {
            TreeViewItem parent = GetParentTreeNode(item);
            if (parent != null)
                return GetRootTreeViewItem(parent);
            else // parent is TreeView
                return item;
        }

        private static TreeViewItem GetParentTreeNode(TreeViewItem item) {
            DependencyObject parent = item;
            do {
                parent = VisualTreeHelper.GetParent(parent);
            }
            while (!(parent is TreeViewItem || parent is TreeView));
            return parent as TreeViewItem;
        }
    }
}
