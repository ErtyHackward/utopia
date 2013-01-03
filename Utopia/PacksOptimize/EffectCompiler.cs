using S33m3Engines.Effects;
using SharpDX.D3DCompiler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacksOptimize
{
    public class EffectCompiler
    {
        public enum EntryPointType
        {
            None,
            vs,
            gs,
            ps
        }

        private struct EP
        {
            public EntryPointType Type;
            public string Name;

            public string Profile
            {
                get
                {
                    switch (Type)
                    {
                        case EntryPointType.vs:
                            return "vs_4_0";
                        case EntryPointType.gs:
                            return "gs_4_0";
                        case EntryPointType.ps:
                            return "ps_4_0";
                        default:
                            return null;
                    }
                }
            }
        }

        #region Private Variables
        private ShaderFlags _shaderFlags;
        private DefaultIncludeHandler _include;
        #endregion

        public EffectCompiler(ShaderFlags shaderFlags, string includeHandlerPath)
        {
            _shaderFlags = shaderFlags;
            _include = new PackOptimizeIncludeHandler(includeHandlerPath);
        }

        #region Public Properties
        #endregion

        #region Public Methods
        public void ProcessDirectory(string path)
        {
            int nbr = 0;
            //Get All Effects file
            List<FileInfo> files = new List<FileInfo>();
            GetAllEffectsFiles(path, files);

            foreach (var file in files)
            {
                if (CompiledEffect(file))
                {
                    nbr++;
                    file.Delete();
                }
            }

            Console.WriteLine("Compiled shadder files: {0}", nbr);
        }

        #endregion

        #region Private Methods
        private void GetAllEffectsFiles(string directory, List<FileInfo> files)
        {
            //Check files in directory
            foreach (var file in Directory.GetFiles(directory, "*.hlsl"))
            {
                files.Add(new FileInfo(file));
            }

            foreach (var subDirectories in Directory.GetDirectories(directory))
            {
                GetAllEffectsFiles(subDirectories, files);
            }
        }

        private bool CompiledEffect(FileInfo effectFile)
        {
            List<EP> entryPoints = new List<EP>();
            string rootPath = effectFile.DirectoryName;
            EntryPointType nextInstructionEntryPoint = EntryPointType.None;

            //Open file and extract all Entry points
            using (TextReader reader = effectFile.OpenText())
            {
                string line = null;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Contains("ENTRY POINT]"))
                    {
                        switch (line.Substring(3,2))
                        {
                            case "VS":
                                nextInstructionEntryPoint = EntryPointType.vs;
                                break;
                            case "GS":
                                nextInstructionEntryPoint = EntryPointType.gs;
                                break;
                            case "PS":
                                nextInstructionEntryPoint = EntryPointType.ps;
                                break;
                            default:
                                break;
                        }
                        continue;
                    }

                    if (nextInstructionEntryPoint != EntryPointType.None)
                    {
                        string entryPoint = CheckForEntryPoint(line);
                        if (entryPoint != null)
                        {
                            entryPoints.Add(new EP() { Name = entryPoint, Type = nextInstructionEntryPoint });
                            nextInstructionEntryPoint = EntryPointType.None;
                        }
                    }
                    
                }

            }

            //Do compilation
            foreach (EP entryPoint in entryPoints)
            {
                CompileEffect(effectFile, entryPoint);
            }

            return entryPoints.Count > 0;
        }

        private string CheckForEntryPoint(string line)
        {
            //Remove leading space.
            line = line.TrimStart(' ');
            line = line.TrimStart('\t');
            if (line.Length == 0) return null;

            if (line.StartsWith(@"//")) return null;
            if (line.StartsWith(@"[")) return null;

            string[] spaceSplitted = line.Split(' ', '(');

            return spaceSplitted[1];
        }

        private void CompileEffect(FileInfo effectFile, EP entryPoint)
        {
            using (CompilationResult bytecode = ShaderBytecode.CompileFromFile(effectFile.FullName, entryPoint.Name, entryPoint.Profile, _shaderFlags, EffectFlags.None, null, _include))
            {
                //dislay Compilation Warning
                if (bytecode.Message != null) Console.WriteLine("Warning compilation message. File {0}, Warning {1}", effectFile.Name, bytecode.Message);
                if (bytecode.HasErrors) throw new Exception("Compilation error !!"); 

                string fileName = String.Concat(Path.GetFileNameWithoutExtension(effectFile.Name), "_", entryPoint.Name.ToString(), ".chlsl");
                File.WriteAllBytes(string.Concat(effectFile.DirectoryName, @"\", fileName), bytecode.Bytecode.Data);
            }
        }

        #endregion

    }
}
