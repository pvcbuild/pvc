![PVC Build Engine](http://i.imgur.com/vyROdJJ.png)

###What

The PVC Build Engine started as a port of [gulp](http://gulpjs.com) for .NET projects but we are now evolving on our own
towards the goal of being the easiest to manage, most comprehensive task runner available.

###Getting Started

PVC is distributed via [Chocolatey](http://chocolatey.org) and plugins are available as [NuGet](http://nuget.org) packages. We provide a pluggable architecture that
allows you to run any tasks you need. Build execution is done via [scriptcs](http://scriptcs.net)'s wonderful hosting API.

```
cinst pvc
```

We'll install our plugins using nuget (pvc install command coming soon):

```
nuget install -o pvc-packages Pvc.AzureBlob
nuget install -o pvc-packages Pvc.Browserify
```

We use NuGet for packaging but to avoid conflicts with other .NET applications, we have our own packages folder and configuration file at:

```
./pvc-packages/
./pvc-packages/pvc-packages.config
```

We're going to create a 'false fork' of any active plugins so that our GitHub organization page can be used to locate plugins for now: [PVC Build](http://github.com/pvcbuild)

Hit me up on Twitter [@stirno](http://twitter.com/stirno) and let me know what you think!

###Sample
Sample time! In the directory of your project, you'd create a file like this:

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

###Runtimes

PVC will package additional runtimes to help plugin developers get access to other ecosystems that may not have been ported to .NET or won't be, such as Browserify.

These runtimes should not be directly referenced by PVC users but plugins may rely on them to provide consistent access to the underlying environment from our packages.

**Active runtimes:**
```
Pvc.Runtime.NodeJs
```

###Streams
As with gulp, PVC plugins are stream based. Currently a large number of useful packages that will be wrapped as plugins are built around file
access such as SassAndCoffee's `SassCompiler`. To work with these libraries there are some useful helper methods in `PvcUtil` to handle marshalling
an input stream to a temp file and back.

Its not ideal but it helps us move forward with streaming content into plugins. In the future we hope users will assist us in adding stream support to more 3rd party libraries.

###Current plugins include
- Browserify [pvc-browserify](https://github.com/pvcbuild/pvc-browserify)
- LESS [pvc-less](https://github.com/pvcbuild/pvc-less)
- SASS [pvc-sass](https://github.com/pvcbuild/pvc-sass)
- MSBuild [pvc-msbuild](https://github.com/pvcbuild/pvc-msbuild)
- NuGet [pvc-nuget](https://github.com/pvcbuild/pvc-nuget)
- HtmlCompressor [pvc-htmlcompressor](https://github.com/pauljz/pvc-htmlcompressor)
- AzureBlob [pvc-azureblob](https://github.com/pauljz/pvc-azureblob)