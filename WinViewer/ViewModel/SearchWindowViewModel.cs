﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Input;
using PureLib.Common;
using PureLib.WPF;
using WhereAreThem.Model;
using IO = System.IO;

namespace WhereAreThem.WinViewer {
    public class SearchWindowViewModel : ViewModelBase {
        private SearchResult _selectedSearchResult;
        private ObservableCollection<SearchResult> _results;
        private string _searchPattern;
        private ICommand _searchCommand;

        public Folder Root { get; set; }
        public List<Folder> RootStack { get; set; }
        public SearchResult SelectedSearchResult {
            get { return _selectedSearchResult; }
            set {
                _selectedSearchResult = value;
                RaiseChange(() => SelectedSearchResult);
            }
        }
        public ObservableCollection<SearchResult> Results {
            get { return _results; }
            set {
                _results = value;
                RaiseChange(() => Results);
            }
        }
        public string SearchPattern {
            get { return _searchPattern; }
            set {
                _searchPattern = value;
                RaiseChange(() => SearchPattern);
            }
        }
        public string WindowTitle {
            get {
                return "Search in {0} of {1}".FormatWith(
                    IO.Path.Combine(RootStack.Select(f => f.Name).Union(new string[] { Root.Name }).ToArray()),
                    RootStack.First().Name);
            }
        }
        public ICommand SearchCommand {
            get {
                if (_searchCommand == null)
                    _searchCommand = new RelayCommand((p) => {
                        Results.Clear();
                        SearchInFolder(Root, RootStack);
                    }, (p) => { return !SearchPattern.IsNullOrEmpty(); });
                return _searchCommand;
            }
        }

        public SearchWindowViewModel() {
            Results = new ObservableCollection<SearchResult>();
        }

        public event LocatingItemEventHandler LocatingItem;

        public void RefreshWindowTitle() {
            RaiseChange(() => WindowTitle);
        }

        public void OnLocatingItem() {
            if (LocatingItem != null)
                LocatingItem(this, new LocatingItemEventArgs(SelectedSearchResult));
            View.Close();
        }

        private void SearchInFolder(Folder folder, List<Folder> folderStack) {
            if (folder.Files != null) {
                List<Folder> stack = new List<Folder>(folderStack);
                stack.Add(folder);
                foreach (File f in folder.Files) {
                    if (Regex.IsMatch(f.Name, SearchPattern.WildcardToRegex(), RegexOptions.IgnoreCase))
                        Results.Add(new SearchResult(f, stack));
                }
            }
            if (folder.Folders != null) {
                List<Folder> stack = new List<Folder>(folderStack);
                stack.Add(folder);
                foreach (Folder f in folder.Folders) {
                    if (Regex.IsMatch(f.Name, SearchPattern.WildcardToRegex(), RegexOptions.IgnoreCase))
                        Results.Add(new SearchResult(f, stack));
                    SearchInFolder(f, stack);
                }
            }
        }
    }
}
