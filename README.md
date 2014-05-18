![PVC Build Engine](http://i.imgur.com/vyROdJJ.png)

The PVC Build Engine started as a port of [gulp](http://gulpjs.com) for .NET projects.

It will take advantage of [NuGet](http://nuget.org) and [scriptcs](http://scriptcs.net) to provide a pluggable architecture that
allows you to run any tasks you need.

PVC will be installed via chocolatey and contain everything needed to execute tasks.

Hit me up on Twitter [@stirno](http://twitter.com/stirno) and let me know what you think!

###Status
You can create arbitrary tasks, load file streams (no globbing yet) and pipe them into plugins like [pvc-less](https://github.com/pvcbuild/pvc-less) or [pvc-sass](https://github.com/pvcbuild/pvc-sass)).

The next task is file globbing then error handling and some more plugins to test the structure.

###Streams
As with gulp, PVC plugins are stream based. Currently a large number of useful packages that will be wrapped as plugins are built around file
access such as SassAndCoffee's `SassCompiler`. To work with these libraries there are some useful helper methods in `PvcUtil` to handle marshalling
an input stream to a temp file and back.

Its not ideal but it helps us move forward with streaming content into plugins. In the future we hope users will assist us in adding stream support to more 3rd party libraries.

###Sample
This sample is what we're working towards completing for our testbed. In the directory of your project, you'd create a file like this:

*pvcfile.csx*
```
pvc.Task("js", () => {
    // browserify or similar
});

pvc.Task("stylesheets", () => {
    pvc.Source("test.less", "test.sass")
       .Pipe("less$", new PvcLess())
       .Pipe("(sass|scss)$", new PvcSass())
       .Save("~/deploy");
}).Requires("sprites");

pvc.Task("sprites", () => {
    // sprite generator
});

pvc.Task("default").Requires("stylesheets", "js");
```

Once the `pvcfile.csx` is created (and pvc installed), you can execute tasks with the following commands
```
# run default task, if exists
pvc

# run stylesheets task
pvc stylesheets
```

###Plugins 

A PVC plugin has a very simple interface that it must implement (by extending `PvcPlugin`):

```
IEnumerable<PvcStream> Execute(IEnumerable<PvcStream> inputStreams)
```

Accept streams in, return streams out.

Plugins can generate arbitrary numbers of streams meaning they can produce more streams than passed into them (or fewer). The specific
motivation for this decision is source map generation.

###Initial plugins include
- LESS [pvc-less](https://github.com/pvcbuild/pvc-less)
- SASS [pvc-sass](https://github.com/pvcbuild/pvc-sass)
