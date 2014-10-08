using SharpDX.D3DCompiler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacksOptimize
{
    class Program
    {
        private static string rootPath = null;
        private static ShaderFlags compilationFlag;
        private static string includeHandlerPath = null;
        private static string action = null;
         
        static void Main(string[] args)
        {
            Console.WriteLine("PacksOptimize starting ...");
            foreach (string param in args)
            {
                Console.WriteLine("Arguments received : {0}", param);
            }

            //First agument = Pack Root Path
            if (args.Length == 0)
            {
                Console.WriteLine("Utopia Pack Optimizer");
                Console.WriteLine("Syntax : PackOptimize action=Compilation path=\"rootPath\" includePath=\"includePath\" debugcompil=0");
                Console.WriteLine("Syntax : PackOptimize action=CreateTextureArray path=\"rootPath\" ");
                return;
            }
            else
            {
                foreach (string param in args)
                {
                    string p;
                    p = param.Replace("\"", "");
                    p = p.Replace("\'", "");
                    string[] paramData = p.Split('=');
                    if(paramData.Length != 2) return;
                    switch (paramData[0])
                    {
                        case "action" :
                            action = paramData[1];
                            break;
                        case "path":
                            rootPath = paramData[1].Replace("\"", "");
                            break;
                        case "includePath":
                            includeHandlerPath = paramData[1].Replace("\"", "");
                            break;
                        case "debugcompil":
                            if (paramData[1] == "1")
                            {
                                compilationFlag = ShaderFlags.Debug | ShaderFlags.SkipOptimization;
                            }
                            else
                            {
                                compilationFlag = ShaderFlags.OptimizationLevel3;
                            }
                            break;
                        default:
                            break;
                    }
                }
                StartPackOptimazing();
            }
        }

        private static void StartPackOptimazing()
        {
            switch (action.ToLower())
            {
                case "cleanup":
                    var filePaths = Directory.GetFiles(rootPath, "*.chlsl", SearchOption.AllDirectories);
                    foreach(string filePath in filePaths)
                    {
                        File.Delete(filePath);
                    }
                    break;
                case "compilation":
                    if (rootPath == null || includeHandlerPath == null)
                    {
                        Console.WriteLine("Missing parameters for compilation");
                        return;
                    }
                    //Will Compile all effect files from a given Directory
                    EffectCompiler effectCompiler = new EffectCompiler(compilationFlag, includeHandlerPath);
                    effectCompiler.ProcessDirectory(rootPath);
                    break;
                case "createtexturearray":

                    if (rootPath == null)
                    {
                        Console.WriteLine("Missing parameters for Array texture creation");
                        return;
                    }
                    TextureArrayCreation textureArrays = new TextureArrayCreation(rootPath);
                    textureArrays.CreateTextureArrays();
                    textureArrays.Dispose();

                    break;
                default:
                    Console.WriteLine("Unknown Action value parameters : {0}", action.ToLower());
                    return;
            }
        }
    }
}
