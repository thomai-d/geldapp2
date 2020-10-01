using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Run);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

    private string _nugetVersionV2;
    private string _fullSemVer;
    private string _versionDate;

    Target GitVersion => _ => _
        .OnlyWhenStatic(() => IsLocalBuild)
        .Executes(() =>
        {
            var gitv = GitVersionTasks.GitVersion().Result;
            _nugetVersionV2 = gitv.NuGetVersionV2;
            _fullSemVer = gitv.FullSemVer;

            Logger.Info($"NuGetVersion: {_nugetVersionV2}");
            Logger.Info($"FullSemVer:   {_fullSemVer}");
        });

    Target GitVersionWorkaround => _ => _
        .OnlyWhenStatic(() => !IsLocalBuild)
        .DependsOn(GitVersion)
        .Executes(() =>
        {
            // HACK: The Gitversion NuGet package currently seems broken for linux. This is a workaround for my build server using the dotnet tool of gitversion"
            var p = ProcessTasks.StartProcess("dotnet-gitversion", $"{SourceDirectory.Parent}");
            p.WaitForExit();
            var fullText = string.Join("\n", p.Output.Select(o => o.Text));

            _versionDate = DateTime.Now.ToString("yyyy-MM-dd HH:MM");

            var gitversion = JObject.Parse(fullText);
            _nugetVersionV2 = gitversion.SelectToken("NuGetVersionV2").Value<string>();
            if (string.IsNullOrEmpty(_nugetVersionV2))
            {
                throw new Exception($"Can't find NuGetVersionV2 in gitversion output: {fullText}");
            }

            _fullSemVer = gitversion.SelectToken("FullSemVer").Value<string>();
            if (string.IsNullOrEmpty(_fullSemVer))
            {
                throw new Exception($"Can't find FullSemVer in gitversion output: {fullText}");
            }

            Logger.Info($"NuGetVersion: {_nugetVersionV2}");
            Logger.Info($"FullSemVer:   {_fullSemVer}");
            Logger.Info($"Date:         {_versionDate}");
        });

    Target UpdateCsProjVersions => _ => _
        .DependsOn(GitVersionWorkaround)
        .Executes(() =>
        {
            foreach (var csproj in GlobFiles(SourceDirectory, "**/*.csproj"))
            {
                var doc = XElement.Load(csproj);
                var version = doc.XPathSelectElement("/PropertyGroup/Version");
                if (version != null)
                {
                    Logger.Info($"Updating {csproj} version to {_fullSemVer}...");
                    version.SetValue(_fullSemVer);
                    doc.Save(csproj);
                }
                else
                {
                    throw new Exception($"Can't update {csproj} version, since there's no /PropertyGroup/Version tag");
                }
            }
        });

    Target UpdateClientAppVersions => _ => _
        .DependsOn(UpdateCsProjVersions)
        .Executes(() =>
        {
            // Replace json files
            ReplaceJson(SourceDirectory / @"GeldApp2\ClientApp\package.json", "version", _nugetVersionV2);

            var jsonVersionStr = $"version: '{_fullSemVer}'";
            var releaseDateStr = $"versionDate: '{_versionDate}'";
            ReplaceRegex(SourceDirectory / @"GeldApp2\ClientApp\src\environments\environment.ts", "version: '.*?'", jsonVersionStr);
            ReplaceRegex(SourceDirectory / @"GeldApp2\ClientApp\src\environments\environment.prod.ts", "version: '.*?'", jsonVersionStr);
            ReplaceRegex(SourceDirectory / @"GeldApp2\ClientApp\src\environments\environment.ts", "versionDate: '.*?'", releaseDateStr);
            ReplaceRegex(SourceDirectory / @"GeldApp2\ClientApp\src\environments\environment.prod.ts", "versionDate: '.*?'", releaseDateStr);

            // Application title
            ReplaceRegex(SourceDirectory / @"GeldApp2\ClientApp\src\index.html", "<title>.*?</title>", $"<title>GeldApp - v{_fullSemVer}</title>");
        });

    Target Run => _ => _
        .DependsOn(UpdateClientAppVersions)
        .Executes(() =>
        {

        });

    private void ReplaceRegex(string path, string rx, string value)
    {
        Logger.Info($"Updating {path}...");
        var content = File.ReadAllText(path);
        content = Regex.Replace(content, rx, value);
        File.WriteAllText(path, content);
    }

    private void ReplaceJson(string path, string selector, string value)
    {
        Logger.Info($"Updating {path}...");

        JObject packageJson;
        using (var pjs = new StreamReader(path))
        using (var jts = new JsonTextReader(pjs))
            packageJson = JObject.Load(jts);

        packageJson.SelectToken(selector).Replace(value);
        using (var jsw = new StreamWriter(path))
        using (var jtw = new JsonTextWriter(jsw))
        {
            jtw.Formatting = Formatting.Indented;
            packageJson.WriteTo(jtw);
        }
    }
}
