﻿using System;
using System.IO;
using BenchmarkDotNet.Characteristics;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.DotNetCli;
using System.Reflection;

namespace BenchmarkDotNet.Toolchains.CsProj
{
    public class CsProjGenerator : DotNetCliGenerator
    {
        public CsProjGenerator(string targetFrameworkMoniker, Func<Platform, string> platformProvider) 
            : base(targetFrameworkMoniker, null, platformProvider, null, null)
        {
        }

        protected override string GetProjectFilePath(string binariesDirectoryPath)
            => Path.Combine(binariesDirectoryPath, "BenchmarkDotNet.Autogenerated.csproj");

        protected override void GenerateProject(Benchmark benchmark, ArtifactsPaths artifactsPaths, IResolver resolver)
        {
            string template = ResourceHelper.LoadTemplate("CsProj.txt");

            string content = SetPlatform(template, PlatformProvider(benchmark.Job.ResolveValue(EnvMode.PlatformCharacteristic, resolver)));
            content = SetCodeFileName(content, Path.GetFileName(artifactsPaths.ProgramCodePath));
            content = SetDependencyToExecutingAssembly(content, benchmark.Target.Type);
            content = SetTargetFrameworkMoniker(content, TargetFrameworkMoniker);
            //content = SetGcMode(content, benchmark.Job.Env.Gc, resolver); todo: implement

            File.WriteAllText(artifactsPaths.ProjectFilePath, content);
        }

        protected override string SetDependencyToExecutingAssembly(string template, Type benchmarkTarget)
        {
            var assemblyName = benchmarkTarget.GetTypeInfo().Assembly.GetName();

            // todo: support custom path .csprojs, now it's <ProjectReference Include="..\$EXECUTINGASSEMBLY$\$EXECUTINGASSEMBLY$.csproj" />
            return template.Replace("$EXECUTINGASSEMBLY$", assemblyName.Name);
        }
    }
}