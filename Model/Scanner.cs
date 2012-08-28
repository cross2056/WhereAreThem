﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PureLib.Common;
using WhereAreThem.Model.Models;
using WhereAreThem.Model.Persistences;
using IO = System.IO;

namespace WhereAreThem.Model {
    public class Scanner {
        private const FileAttributes filter = FileAttributes.Hidden | FileAttributes.System;
        private readonly string driveSuffix = "{0}{1}".FormatWith(Path.VolumeSeparatorChar, Path.DirectorySeparatorChar);
        private string _machinePath;
        private IPersistence _persistence;

        public event StringEventHandler PrintLine;

        public Scanner(string machinePath, IPersistence persistence) {
            _machinePath = machinePath;
            _persistence = persistence;
        }

        public void Scan(string drive) {
            string driveLetter = drive.EndsWith(driveSuffix) ? GetDriveLetter(drive) : drive;
            Folder driveFolder = GetFolder(new DirectoryInfo("{0}{1}".FormatWith(driveLetter, driveSuffix)));
            Save(GetListPath(driveLetter), driveFolder);
        }

        public void ScanUpdate(string path) {
            if (path.IsNullOrEmpty())
                throw new ArgumentNullException("ScanUpdate path is null.");
            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException("Path '{0}' cannot be found.".FormatWith(path));
            string[] parts = path.Split(new char[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            parts[0] += Path.DirectorySeparatorChar;
            string listPath = GetListPath(GetDriveLetter(parts[0]));
            if (!IO.File.Exists(listPath))
                throw new FileNotFoundException("List '{0}' cannot be found.".FormatWith(listPath));
            Folder driveFolder = _persistence.Load(listPath);
            UpdateFolder(driveFolder, parts);
            Save(listPath, driveFolder);
        }

        private void OnPrintLine(string s) {
            if (PrintLine != null)
                PrintLine(this, new StringEventArgs() { String = s });
        }

        private string GetDriveLetter(string path) {
            return path.Substring(0, path.IndexOf(Path.VolumeSeparatorChar));
        }

        private string GetListPath(string driveLetter) {
            return Path.Combine(_machinePath, Path.ChangeExtension(driveLetter, Constant.ListExt));
        }

        private void Save(string listPath, Folder drive) {
            drive.CreatedDateUtc = DateTime.UtcNow;
            _persistence.Save(drive, listPath);
        }

        private void UpdateFolder(Folder root, string[] pathParts) {
            Folder folder = root;
            for (int i = 1; i < pathParts.Length; i++) {
                if (folder.Folders == null)
                    folder.Folders = new List<Folder>();
                Folder current = folder.Folders.SingleOrDefault(f => f.Name.Equals(pathParts[i], StringComparison.OrdinalIgnoreCase));
                if (current == null) {
                    current = GetFolder(new DirectoryInfo(Path.Combine(pathParts.Take(i + 1).ToArray())));
                    folder.Folders.Add(current);
                    folder.Folders.Sort();
                    return;
                }
                else
                    folder = current;
            }
            Folder newFolder = GetFolder(new DirectoryInfo(Path.Combine(pathParts)));
            folder.CreatedDateUtc = newFolder.CreatedDateUtc;
            folder.Folders = newFolder.Folders;
            folder.Files = newFolder.Files;
        }

        private Folder GetFolder(DirectoryInfo directory) {
            OnPrintLine(directory.FullName);
            Folder folder = new Folder() {
                Name = directory.Name,
                CreatedDateUtc = directory.CreationTimeUtc,
            };
            try {
                folder.Files = directory.GetFiles()
                     .Where(f => !f.Attributes.HasFlag(filter))
                     .Select(f => new Models.File() {
                         Name = f.Name,
                         FileSize = f.Length,
                         CreatedDateUtc = f.CreationTimeUtc,
                         ModifiedDateUtc = f.LastWriteTimeUtc
                     }).ToList();
                folder.Folders = directory.GetDirectories()
                        .Where(d => !d.Attributes.HasFlag(filter))
                        .Select(d => GetFolder(d)).ToList();
                folder.Files.Sort();
                folder.Folders.Sort();
            }
            catch (Exception) { }
            return folder;
        }
    }

    public class StringEventArgs : EventArgs {
        public string String { get; set; }
    }

    public delegate void StringEventHandler(object sender, StringEventArgs e);
}