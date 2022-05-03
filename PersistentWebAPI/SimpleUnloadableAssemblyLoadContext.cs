using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.Reflection;
using System.Runtime.Loader;

namespace PersistentWebAPI
{
    public class SimpleUnloadableAssemblyLoadContext : AssemblyLoadContext
    {
        public SimpleUnloadableAssemblyLoadContext()
           : base(isCollectible: true)
        {
        }

        public Assembly? GetAssembly(string cacheAssemblyName)
        {
            Assembly? assembly = null;
            try
            {
                assembly = this.Assemblies.FirstOrDefault(asm =>  asm.FullName == cacheAssemblyName + ", Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                
            } catch{ }
            if (assembly != null)
            {
                return assembly;
            }

            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(@"
                using System;

                namespace RoslynCompileSample
                {
                    public class LocalTemp
                    {
                        public int NextTemp()
                        {
                            return System.Random.Shared.Next(-20, 55);
                        }
                    }
                }");


            string assemblyName = Path.GetFileName(cacheAssemblyName);
            MetadataReference[] references = new MetadataReference[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location)
            };

            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                );

            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);

                if (!result.Success)
                {
                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);
                    string error = string.Empty;
                    foreach (Diagnostic diagnostic in failures)
                    {
                        error += $"{diagnostic.Id} {diagnostic.GetMessage()}" + System.Environment.NewLine;
                    }
                    throw new Exception(error);
                }
                else
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    assembly = this.LoadFromStream(ms);
                }
            }
            return assembly;
        }
    }

}
