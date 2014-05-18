pvc.Task("default", () => {
	System.Console.WriteLine("Turned off alarm. " + DateTime.Now.Millisecond);
}).Requires("make-coffee");

pvc.Task("make-coffee", (done) => {
	System.Console.WriteLine("Made coffee. " + DateTime.Now.Millisecond);
	throw new Exception("halp twalf");
	done();
});

pvc.Task("sync-test", () => System.Console.WriteLine("sync test"));
pvc.Task("dont-run-me", () => System.Console.WriteLine("wat"));

pvc.Task("1default", () => {
	pvc.Source(@"C:\Projects\FluentAutomation\FluentAutomation.sln")
	   .Pipe(new PvcMSBuild("Clean;Build", "Debug"));

	//System.Threading.Thread.Sleep(100000);
});