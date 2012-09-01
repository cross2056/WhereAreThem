﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using PureLib.Common;

namespace WhereAreThem.Model.Models {
    [DataContract]
    public class Drive : Folder {
        [DataMember]
        public DriveType DriveType { get; set; }

        public string DriveLetter {
            get { return GetDriveLetter(Name); }
        }

        public static string GetDriveLetter(string path) {
            return path.Substring(0, path.IndexOf(Path.VolumeSeparatorChar));
        }

        public static string GetDrivePath(string letter) {
            return "{0}{1}{2}".FormatWith(letter, Path.VolumeSeparatorChar, Path.DirectorySeparatorChar);
        }

        public static Drive FromFolder(Folder folder, DriveType driveType) {
            return new Drive() {
                DriveType = driveType,
                Name = folder.Name,
                CreatedDateUtc = folder.CreatedDateUtc,
                Folders = folder.Folders,
                Files = folder.Files,
            };
        }
    }
}
