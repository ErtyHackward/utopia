﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3CoreComponents.Meshes.Factories
{
    public interface IMeshFactory
    {
        bool LoadMesh(string Path, out Mesh mesh, int indiceOffset);
    }
}
