﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WhereAreThem.Model.Models;

namespace WhereAreThem.Model.Persistences {
    public abstract class PersistenceBase : IPersistence, IStreamPersistence {
        public abstract void Save(Folder folder, Stream stream);
        public abstract Folder Load(Stream stream);

        public virtual void Save(Folder folder, string path) {
            using (FileStream stream = new FileStream(path, FileMode.Create)) {
                Save(folder, stream);
            }
        }

        public virtual Folder Load(string path) {
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read)) {
                return Load(stream);
            }
        }
    }
}
