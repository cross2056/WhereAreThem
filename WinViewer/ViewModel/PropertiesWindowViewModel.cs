﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using PureLib.Common;
using PureLib.WPF;
using WhereAreThem.Model;

namespace WhereAreThem.WinViewer {
    public class PropertiesWindowViewModel : ViewModelBase {
        private List<Folder> _itemStack {  get; set; }
        private PropertyInfo _propertyInfo { get; set; }

        public FileSystemItem Item { get; private set; }

        public string FileSystemType {
            get {
                return Item.GetType().Name;
            }
        }

        public string Location {
            get {
                return System.IO.Path.Combine(_itemStack.Select(f => f.Name).ToArray());
            }
        }

        public string Size {
            get {
                return string.Format("{0} ({1} bytes)", _propertyInfo.TotalSizeFriendlyString, _propertyInfo.TotalSizeString);
            }
        }

        public string Contains {
            get {
                string strFilesCount = string.Empty;
                if (IsFolder) {
                    Folder folder = (Folder)Item;
                    strFilesCount = string.Format("{0} Files, {1} Folders", _propertyInfo.FolderCountString, _propertyInfo.FileCountString);
                }
                return strFilesCount;
            }
        }

        public bool IsFolder {
            get { return !IsFile; }
        }

        public bool IsFile {
            get { return Item is File; }
        }

        public PropertiesWindowViewModel(FileSystemItem item, List<Folder> itemStack) {
            Item = item;
            _itemStack = itemStack;
            _propertyInfo = new PropertyInfo(_itemStack.Last(), new string[] { Item.Name });
        }
    }
}
