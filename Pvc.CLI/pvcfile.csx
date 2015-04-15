pvc.Task("less", () => {
	
});

pvc.Task("msbuild", () => {

});

pvc.Task("default", () => {
	var t = pvc;
	t.Task("t", () => {});
    Console.WriteLine("wut");
});