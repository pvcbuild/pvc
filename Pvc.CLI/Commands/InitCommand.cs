using LibGit2Sharp;
using Newtonsoft.Json;
using Pvc.CLI.Commands.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Edokan.KaiZen.Colors;
using PvcCore;
using System.Net;

namespace Pvc.CLI.Commands
{
    public class InitCommand : CommandBase
    {
        internal override bool IsTopLevel
        {
            get { return true; }
        }

        internal override string[] Names
        {
            get { return new[] { "init" }; }
        }

        internal override string Description
        {
            get { return "Initialize project in this folder with the specified template"; }
        }

        internal override void Execute(string[] args, Dictionary<string, string> flags)
        {
            string initTemplate;
            if (args.Length == 2)
            {
                initTemplate = args[1];
            }
            else
            {
                Console.WriteLine("Template name not provided. Usage:");
                Console.WriteLine("   pvc init template-name");
                return;
            }

            string templateJson;
            if (File.Exists(initTemplate))
            {
                templateJson = File.ReadAllText(initTemplate);
            }
            else
            {
                Uri initTemplateUri;
                var isUri = Uri.TryCreate(initTemplate, UriKind.RelativeOrAbsolute, out initTemplateUri);
                var pvcInitTemplatesUri = new Uri("https://github.com/pvcbuild/pvc-init-templates/blob/master");

                if (isUri && !initTemplateUri.IsAbsoluteUri)
                {
                    initTemplateUri = new Uri(new Uri(initTemplate.Contains('/') ? "https://github.com" : "https://raw.githubusercontent.com/pvcbuild/pvc-init-templates/master/", UriKind.Absolute), initTemplateUri);
                }
                else
                {
                    Console.WriteLine("Unable to fetch the requested template. Must be a valid URI.");
                    return;
                }
                
                using (var client = new WebClient())
                {
                    var downloadPath = initTemplateUri.ToString();
                    if (!downloadPath.EndsWith(".json"))
                        downloadPath += ".json";

                    Console.WriteLine("Download template ...");
                    Console.WriteLine("   ({0})", downloadPath);
                    Console.Write(Environment.NewLine);

                    templateJson = client.DownloadString(downloadPath);
                }
            }

            var initContext = JsonConvert.DeserializeObject<PvcInitContext>(templateJson);

            var folderNameKey = "$$pvcFolderName";
            this.ShowPrompt(initContext, new KeyValuePair<string, PvcInitPrompt>(
                "Target Directory: ",
                new PvcInitPrompt()
                {
                    Field = folderNameKey
                }
            ));

            Console.WriteLine("Fetch template repository ...");
            Console.WriteLine("  ({0})", initContext.TemplateRepo.Cyan());
            Console.WriteLine("into directory ...");
            Console.WriteLine("  ({0})", Path.GetFullPath(initContext.Config[folderNameKey]));
            Console.Write(Environment.NewLine);

            string gitRepo;
            try
            {
                gitRepo = this.CloneGitRepo(initContext.TemplateRepo, initContext.Config[folderNameKey]);
                initContext.Config.Remove(folderNameKey);
            }
            catch (NameConflictException)
            {
                Console.WriteLine("The requested folder ({0}) is not empty. Sorry!", initContext.Config[folderNameKey]);
                Console.WriteLine();
                return;
            }

            var repoFiles = Directory.EnumerateFiles(gitRepo, "*", SearchOption.AllDirectories);
            var filteredFiles = repoFiles.Where(
                x => initContext.ReplacementExtensions.Any(
                    y =>
                    {
                        var extension = Path.GetExtension(x);
                        if (extension == string.Empty)
                        {
                            return Path.GetFileName(x).StartsWith(y);
                        }
                        else
                        {
                            return y == extension;
                        }
                    }
                )
            )
            .Select(x => new StringBuilder(x));

            this.InteractWithUser(initContext, repoFiles);
            this.InitTemplate(filteredFiles, gitRepo, initContext.Config);

            // delete the .git repo
            string[] allFileNames = System.IO.Directory.GetFiles(Path.Combine(gitRepo, ".git"), "*.*", System.IO.SearchOption.AllDirectories);
            foreach (string filename in allFileNames)
            {
                FileAttributes attr = File.GetAttributes(filename);
                File.SetAttributes(filename, attr & ~FileAttributes.ReadOnly);
            }

            Directory.Delete(Path.Combine(gitRepo, ".git"), true);
        }

        private void InteractWithUser(PvcInitContext ctx, IEnumerable<string> repoFiles)
        {
            foreach (var prompt in ctx.Prompts)
            {
                this.ShowPrompt(ctx, prompt);

                // handle 'choose'
                if (!string.IsNullOrEmpty(prompt.Value.Choose))
                {
                    var chooseDirs = repoFiles.Where(x => Path.GetFileName(x).StartsWith(prompt.Value.Choose)).GroupBy(y => Path.GetDirectoryName(y));
                    var removedFiles = new List<string>();
                    foreach (var chooseDir in chooseDirs)
                    {
                        if (chooseDir.Count() > 1)
                        {
                            var matchFiles = chooseDir.Where(x => x.EndsWith(ctx.Config[prompt.Value.Field]));
                            chooseDir.Except(matchFiles).ToList().ForEach(x => File.Delete(x));

                            var matchFile = matchFiles.First();
                            var newFileDest = Path.Combine(Path.GetDirectoryName(matchFile), prompt.Value.Choose);
                            File.Move(matchFile, newFileDest);
                            removedFiles.Add(matchFile);
                        }
                    }

                    repoFiles = repoFiles.Except(removedFiles);
                }
            }
        }

        private void ShowPrompt(PvcInitContext ctx, KeyValuePair<string, PvcInitPrompt> prompt)
        {
            Console.WriteLine(prompt.Key);

            if (prompt.Value.Options != null)
            {
                var values = new List<string>();
                foreach (var option in prompt.Value.Options.Where(x => x.Length > 0))
                {
                    if (option.Length == 2)
                        values.Add(option[1]);
                    else
                        values.Add(option[0]);

                    Console.WriteLine(" [{0}] {1}", values.Count, option[0]);
                }
                ctx.Config[prompt.Value.Field] = values[this.ReadOption(values.Count) - 1];
            }
            else
            {
                ctx.Config[prompt.Value.Field] = this.ReadValue(ctx.Config.ContainsKey(prompt.Value.Field) && ctx.Config[prompt.Value.Field] == null);
            }

            Console.WriteLine();
        }

        private int ReadOption(int maxValue)
        {
            Console.Write("{0} {1}>> ", PvcConsole.Tag, PvcConsole.TaskOutputTag);
            var value = 1;
            var response = Console.ReadKey();
            var isInt = Int32.TryParse(response.KeyChar.ToString(), out value);
            Console.WriteLine();

            if (!isInt || value < 1 || value > maxValue)
            {
                Console.WriteLine("Please select a valid option.");
                return ReadOption(maxValue);
            }

            return value;
        }

        private string ReadValue(bool isRequired = false)
        {
            Console.Write("{0} {1}>> ", PvcConsole.Tag, PvcConsole.TaskOutputTag);
            var response = Console.ReadLine().Trim();

            if (isRequired && response == string.Empty)
            {
                Console.WriteLine("Please enter a valid value.");
                return ReadValue(isRequired);
            }

            return response;
        }
        
        private string CloneGitRepo(string repoUri, string repoName)
        {
            return Path.Combine(Repository.Clone(repoUri, Path.Combine(Directory.GetCurrentDirectory(), repoName)), "..");
        }

        private void InitTemplate(IEnumerable<StringBuilder> allFiles, string baseDirectory, Dictionary<string, string> variables)
        {
            var replaceFormat = "$${0}$$";
            var extraReplaceFormat = "$${0}.{1}$$";

            var fileEnumeration = allFiles;
            foreach (var fileNameStringBuilder in fileEnumeration)
            {
                // handle tokens in filenames
                var origName = fileNameStringBuilder.ToString();
                foreach (var variable in variables)
                {
                    fileNameStringBuilder.Replace(string.Format(replaceFormat, variable.Key), variable.Value);
                }

                var fileName = fileNameStringBuilder.ToString();
                if (origName != fileName)
                    File.Move(origName, fileName);

                // handle tokens in files
                if (variables.Count == 0)
                    return;

                var tmpFile = Path.GetTempFileName();
                using (var tmpWriter = new StreamWriter(tmpFile))
                using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var sr = new StreamReader(fs))
                {
                    string line;
                    StringBuilder builder;

                    while ((line = sr.ReadLine()) != null)
                    {
                        builder = new StringBuilder(line);

                        foreach (var variable in variables)
                        {
                            builder.Replace(string.Format(replaceFormat, variable.Key), variable.Value);
                            builder.Replace(string.Format(extraReplaceFormat, variable.Key, "toLower"), variable.Value.ToLower());
                            builder.Replace(string.Format(extraReplaceFormat, variable.Key, "toUpper"), variable.Value.ToUpper());
                            builder.Replace(string.Format(extraReplaceFormat, variable.Key, "ToLower"), variable.Value.ToLower());
                            builder.Replace(string.Format(extraReplaceFormat, variable.Key, "ToUpper"), variable.Value.ToUpper());
                        }

                        tmpWriter.WriteLine(builder.ToString());
                    }

                    tmpWriter.Flush();
                }

                File.Delete(fileName);
                File.Move(tmpFile, fileName);
            }
        }
    }

    namespace Internal
    {
        public class PvcInitPrompt
        {
            public string Field { get; set; }

            public string[][] Options { get; set; }

            public string SelectedValue { get; set; }

            public string Choose { get; set; }

            public string Validator { get; set; }
        }

        public class PvcInitContext
        {
            public string TemplateRepo { get; set; }

            public string[] ReplacementExtensions { get; set; }

            public Dictionary<string, string> Config = new Dictionary<string, string>();

            public Dictionary<string, PvcInitPrompt> Prompts = new Dictionary<string, PvcInitPrompt>();
        }
    }
}
