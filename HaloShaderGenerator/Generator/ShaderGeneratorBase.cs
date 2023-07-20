using HaloShaderGenerator.DirectX;
using HaloShaderGenerator.Globals;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HaloShaderGenerator.Generator
{
    static class ShaderGeneratorBase
    {
        private class IncludeManager : Include
        {

            public IncludeManager(string root_directory = "") : base(root_directory)
            {

            }

            public string ReadResource(string filepath, string _parent_directory = null)
            {
                string parent_directory = _parent_directory ?? base.DirectoryMap[IntPtr.Zero];
                var assembly = Assembly.GetExecutingAssembly();

                string relative_path = Path.Combine(parent_directory, filepath);

                Uri uri2 = new Uri(Path.GetFullPath("halo_online_shaders\\" + relative_path));

                FileInfo file = new FileInfo(uri2.AbsolutePath);

                if (!file.Exists)
                    throw new Exception($"Couldn't find file {relative_path}");

                using (Stream stream = file.OpenRead())
                {
                    if(stream == null)
                    {
                        throw new Exception($"Couldn't find file {relative_path}");
                    }
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }

            protected override int Open(D3D.INCLUDE_TYPE includeType, string filepath, string directory, ref string hlsl_source)
            {
                switch (includeType)
                {
                    case D3D.INCLUDE_TYPE.D3D_INCLUDE_LOCAL:
                        hlsl_source = ReadResource(filepath, directory);
                        return 0;
                    case D3D.INCLUDE_TYPE.D3D_INCLUDE_SYSTEM:
                        hlsl_source = ReadResource(filepath, base.DirectoryMap[IntPtr.Zero]);
                        return 0;
                    default:
                        throw new Exception("Unimplemented include type");
                }
            }
        }

        public static string GetSourceFile(string template)
        {
            string fileName = template.Split('\\').Last();

            IncludeManager include = new IncludeManager(template.Replace(fileName, ""));

            return include.ReadResource(fileName);
        }

        public static byte[] GenerateSource(string template, IEnumerable<D3D.SHADER_MACRO> macros, string entry, string version)
        { 
            // Macros should never be duplicated
            for (var i = 0; i < macros.Count(); i++)
            {
                for (var j = 0; j < macros.Count(); j++)
                {
                    if (i == j) continue;
                    if (macros.ElementAt(i).Name == macros.ElementAt(j).Name)
                    {
                        throw new Exception($"Macro {macros.ElementAt(i).Name} is defined multiple times");
                    }
                }
            }

            string fileName = template.Split('\\').Last();

            IncludeManager include = new IncludeManager(template.Replace(fileName, ""));

            string shader_source = include.ReadResource(fileName);

            D3DCompiler.D3DCOMPILE flags = 0;
#if DEBUG
            //flags |= D3DCompiler.D3DCOMPILE.D3DCOMPILE_WARNINGS_ARE_ERRORS;
#endif
            //flags |= D3DCompiler.D3DCOMPILE.D3DCOMPILE_SKIP_VALIDATION;
            //flags |= D3DCompiler.D3DCOMPILE.D3DCOMPILE_DEBUG;
            flags |= D3DCompiler.D3DCOMPILE.D3DCOMPILE_OPTIMIZATION_LEVEL3; // if can't get shader to compile 1-1 add or remove this line
            byte[] shader_code = D3DCompiler.Compile(
                shader_source,
                entry,
                version,
                macros.ToArray(),
                flags,
                0,
                template,
                include
            );

            GC.KeepAlive(include?.NativePointer);
            return shader_code;
        }

        public static string CreateMethodDefinition(object method, string prefix = "", string suffix = "")
        {
            var method_type_name = method.GetType().Name.ToLower();
            var method_name = method.ToString().ToLower();

            return $"{prefix.ToLower()}{method_name}{suffix.ToLower()}";
        }

        public static string CreateMethodDefinitionFromArgs<MethodType>(IEnumerable<object> args, string prefix = "", string suffix = "") where MethodType : struct, IConvertible
        {
            var method = args.Where(arg => arg.GetType() == typeof(MethodType)).Cast<MethodType>().FirstOrDefault();
            var method_type_name = method.GetType().Name.ToLower();
            var method_name = method.ToString().ToLower();

            return $"{prefix.ToLower()}{method_name}{suffix.ToLower()}";
        }

        public static D3D.SHADER_MACRO CreateMacro(string name, string definition)
        {
            return new D3D.SHADER_MACRO { Name = name, Definition = definition };
        }

        public static D3D.SHADER_MACRO CreateAutoMacro(string name, string definition)
        {
            return new D3D.SHADER_MACRO { Name = "category_" + name, Definition = "category_" + name + "_option_" + definition };
        }

        public static D3D.SHADER_MACRO CreateMacro(string name, object method, string prefix = "", string suffix = "")
        {
            return new D3D.SHADER_MACRO { Name = name, Definition = CreateMethodDefinition(method, prefix, suffix) };
        }

        public static D3D.SHADER_MACRO CreateVertexMacro(string name, VertexType type)
        {
            return new D3D.SHADER_MACRO { Name = name, Definition = $"{type.ToString().ToUpper()}_VERTEX" };
        }

        public static D3D.SHADER_MACRO CreateMacroFromArgs<MethodType>(string name, IEnumerable<object> args, string prefix = "", string suffix = "") where MethodType : struct, IConvertible
        {
            return new D3D.SHADER_MACRO { Name = name, Definition = CreateMethodDefinitionFromArgs<MethodType>(args, prefix, suffix) };
        }

        public static IEnumerable<D3D.SHADER_MACRO> CreateMethodEnumDefinitions<MethodType>() where MethodType : struct, IConvertible
        {
            List<D3D.SHADER_MACRO> macros = new List<D3D.SHADER_MACRO>();

            var method_type_name = typeof(MethodType).Name.ToLower();

            var values = Enum.GetValues(typeof(MethodType));
            foreach (var method in values)
            {
                var method_name = method.ToString().ToLower();

                macros.Add(new D3D.SHADER_MACRO { Name = $"k_{method_type_name}_{method_name}", Definition = ((int)method).ToString() });
            }

            return macros;
        }

        public static IEnumerable<D3D.SHADER_MACRO> CreateAutoMacroMethodEnumDefinitions<MethodType>() where MethodType : struct, IConvertible
        {
            List<D3D.SHADER_MACRO> macros = new List<D3D.SHADER_MACRO>(); 

            var method_type_name = typeof(MethodType).Name.ToLower();

            var values = Enum.GetValues(typeof(MethodType));
            foreach (var method in values)
            {
                var method_name = method.ToString().ToLower();

                macros.Add(new D3D.SHADER_MACRO { Name = $"category_{method_type_name}_option_{method_name}", Definition = ((int)method).ToString() });
            }

            return macros;
        }
    }
}
