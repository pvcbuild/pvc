pvc.Task("nuget-push", () => {
	// configure NuGet
	PvcNuGet.NuGetExePath = @"C:\Chocolatey\bin\NuGet.bat";
	PvcNuGet.ApiKey = "";

	pvc.Source("Pvc.Core.csproj")
	   .Pipe(new PvcNuGetPack(
			createSymbolsPackage: true
	   ))
	   .Pipe(new PvcNuGetPush());
});

pvc.Task("inline", () => {
	pvc.Source("Pvc.Core.csproj")
	.Pipe((streams) => {
		streams.ToList().ForEach(s => {
			Console.WriteLine(s.OriginalSourcePath);
		});

		return streams;
	});
});