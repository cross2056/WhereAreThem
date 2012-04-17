﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using PureLib.Common;

namespace WhereAreThem.Model {
    public class XmlPersistence : IPersistence, IStreamPersistence {
        public void Save(Folder folder, string path) {
            using (FileStream stream = new FileStream(path, FileMode.Create)) {
                Save(folder, stream);
            }
        }

        public void Save(Folder folder, Stream stream) {
            XmlSerializer serializer = new XmlSerializer(typeof(Folder));
            using (XmlWriter writer = XmlWriter.Create(stream)) {
                serializer.Serialize(writer, folder);
            }
        }

        public Folder Load(string path) {
            using (FileStream stream = new FileStream(path, FileMode.Create)) {
                return Load(stream);
            }
        }

        public Folder Load(Stream stream) {
            using (XmlReader reader = XmlReader.Create(stream)) {
                XmlSerializer serializer = new XmlSerializer(typeof(Folder));
                return (Folder)serializer.Deserialize(reader);
            }
        }
    }
}