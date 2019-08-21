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
	.Does(() =>
{
	var nuGetPackSettings = new NuGetPackSettings
	{
		OutputDirectory = artifactsDirectory
	};
	NuGetPack("./Cmdty.Core.nuspec", nuGetPackSettings);
});	


Task("Default")
	.IsDependentOn("Pack-NuGet");

Task("CI")
	.IsDependentOn("Pack-NuGet");

RunTarget(target);
