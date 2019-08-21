var target = Argument<string>("Target", "Default");
var configuration = Argument<string>("Configuration", "Release");

var artifactsDirectory = Directory("./artifacts");
var testResultDir = "./temp/";
var isRunningOnBuildServer = !BuildSystem.IsLocalBuild;

var msBuildSettings = new DotNetCoreMSBuildSettings();


if (HasArgument("BuildNumber"))
{
    msBuildSettings.WithProperty("BuildNumber", Argument<string>("BuildNumber"));
    msBuildSettings.WithProperty("VersionSuffix", "alpha" + Argument<string>("BuildNumber"));
}

if (HasArgument("VersionPrefix"))
{
    msBuildSettings.WithProperty("VersionPrefix", Argument<string>("VersionPrefix"));
}

Task("Clean-Artifacts")
    .Does(() =>
{
    CleanDirectory(artifactsDirectory);
});

Task("Add-NuGetSource")
    .Does(() =>
    {
		if (isRunningOnBuildServer)
		{
			// Get the access token
			string accessToken = EnvironmentVariable("SYSTEM_ACCESSTOKEN");
			if (string.IsNullOrEmpty(accessToken))
			{
				throw new InvalidOperationException("Could not resolve SYSTEM_ACCESSTOKEN.");
			}

			NuGetRemoveSource("Cmdty", "https://pkgs.dev.azure.com/cmdty/_packaging/cmdty/nuget/v3/index.json");

			// Add the authenticated feed source
			NuGetAddSource(
				"Cmdty",
				"https://pkgs.dev.azure.com/cmdty/_packaging/cmdty/nuget/v3/index.json",
				new NuGetSourcesSettings
				{
					UserName = "VSTS",
					Password = accessToken
				});
		}
		else
		{
			Information("Not running on build so no need to add Cmdty NuGet source");
		}
    });

Task("Build")
	.IsDependentOn("Add-NuGetSource")
    .Does(() =>
{
    var dotNetCoreSettings = new DotNetCoreBuildSettings()
            {
                Configuration = configuration,
                MSBuildSettings = msBuildSettings
            };
    DotNetCoreBuild("Cmdty.Core.sln", dotNetCoreSettings);
});

Task("Test-C#")
    .IsDependentOn("Build")
    .Does(() =>
{
    Information("Cleaning test output directory");
    CleanDirectory(testResultDir);

    var projects = GetFiles("./tests/**/*.Test.csproj");
    
    foreach(var project in projects)
    {
        Information("Testing project " + project);
        DotNetCoreTest(
            project.ToString(),
            new DotNetCoreTestSettings()
            {
                ArgumentCustomization = args=>args.Append($"/p:CollectCoverage=true /p:CoverletOutputFormat=cobertura"),
                Logger = "trx",
                ResultsDirectory = testResultDir,
                Configuration = configuration,
                NoBuild = true
            });
    }
});


Task("Pack-NuGet")
	.IsDependentOn("Test-C#")
	.IsDependentOn("Clean-Artifacts")
	.Does(setupContext =>
{
	string workingDirectory = setupContext.Environment.WorkingDirectory.ToString();

	string productVersion = GetAssemblyVersion(configuration, workingDirectory);

	var nuGetPackSettings = new NuGetPackSettings
	{
		OutputDirectory = artifactsDirectory,
		Version =  productVersion,
		Properties = new Dictionary<string, string>
		{
			{ "configuration", configuration }
		}
	};
	NuGetPack("./Cmdty.Core.nuspec", nuGetPackSettings);
});	


using System.Reflection;
private string GetAssemblyVersion(string configuration, string workingDirectory)
{
	// TODO find better way of doing this!
	string assemblyPath = System.IO.Path.Combine(workingDirectory, "src", "Cmdty.Core.Trees", "bin", configuration, "net45", "Cmdty.Core.Trees.dll");
	Assembly assembly = Assembly.LoadFrom(assemblyPath);
	string productVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion;
	return productVersion;
}

Task("Push-NuGetToCmdtyFeed")
    .IsDependentOn("Add-NuGetSource")
    .IsDependentOn("Pack-NuGet")
    .Does(() =>
{
    var nupkgPath = GetFiles(artifactsDirectory.ToString() + "/*.nupkg").Single();
    Information($"Pushing NuGetPackage in {nupkgPath} to Cmdty feed");
    NuGetPush(nupkgPath, new NuGetPushSettings 
    {
        Source = "Cmdty",
        ApiKey = "VSTS"
    });
});


Task("Default")
	.IsDependentOn("Pack-NuGet");

Task("CI")
	.IsDependentOn("Push-NuGetToCmdtyFeed");

RunTarget(target);
