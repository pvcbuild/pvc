pvc.Task("nuget-push", () => {
	// configure NuGet
	PvcNuGet.ApiKey = Environment.GetEnvironmentVariable("NugetApiKey");

	pvc.Source("Pvc.Core.csproj")
	   .Pipe(new PvcNuGetPack(
			createSymbolsPackage: true
	   ))
	   .Pipe(new PvcNuGetPush());
});