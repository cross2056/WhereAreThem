﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using PureLib.Common;
using PureLib.WPF;
using WhereAreThem.Model;
using IO = System.IO;

namespace WhereAreThem.WinViewer {
    public class LocateItemEventArgs : EventArgs {
        public SearchResult Result { get; private set; }

        public LocateItemEventArgs(SearchResult result) {
            Result = result;
        }
    }
    public delegate void LocateItemEventHandler(object sender, LocateItemEventArgs e);

    public class SearchWindowViewModel : ViewModelBase {
        private RelayCommand _searchCommand;
        private RelayCommand _locateCommand;
        private SearchResult _selectedSearchResult;
        private ObservableCollection<SearchResult> _results;
        private string _searchPattern;

        public SearchWindowViewModel() {
            Results = new ObservableCollection<SearchResult>();
        }

        public event LocateItemEventHandler LocatingItem;

        public Folder Root { get; set; }
        public List<Folder> RootStack { get; set; }
        public RelayCommand SearchCommand {
            get {
                if (_searchCommand == null)
                    _searchCommand = new RelayCommand((p) => {
                        Results.Clear();
                        SearchInFolder(Root, RootStack);
                    }, (p) => { return !SearchPattern.IsNullOrEmpty(); });
                return _searchCommand;
            }
        }
        public RelayCommand LocateCommand {
            get {
                if (_locateCommand == null)
                    _locateCommand = new RelayCommand((p) => {
                        if (LocatingItem != null)
                            LocatingItem(this, new LocateItemEventArgs(SelectedSearchResult));
                        SelectedSearchResult = null;
                        View.Close();
                    });
                return _locateCommand;
            }
        }
        public SearchResult SelectedSearchResult {
            get { return _selectedSearchResult; }
            set {
                _selectedSearchResult = value;
                RaiseChange("SelectedSearchResult");
            }
        }
        public ObservableCollection<SearchResult> Results {
            get { return _results; }
            set {
                _results = value;
                RaiseChange("Results");
            }
        }
        public string SearchPattern {
            get { return _searchPattern; }
            set {
                _searchPattern = value;
                RaiseChange("SearchPattern");
            }
        }
        public string WindowTitle {
            get {
                return "Search {0} in {1}".FormatWith(Root.Name, IO.Path.Combine(RootStack.Select(f => f.Name).ToArray()));
            }
        }

        public void RefreshWindowTitle() {
            RaiseChange("WindowTitle");
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