using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Wave.Common;

namespace BuildHelper
{
    class Program
    {
        const string ProductName = "Wave Explorer Build Helper";

        const string PlatformOptionsValue = "@@WPPLATFORMOPTIONS@@";
        const string ClientOptionsValue = "@@WPCLIENTOPTIONS@@";

        const string File1 = "BuildConfig.xml";
        const string File2 = "WMAppManifest.xml";
        const string File3 = "AssemblyInfo.cs";

        static Dictionary<string, string> substitutions = new Dictionary<string, string>();
        
        static void Main(string[] args)
        {
            Console.WriteLine(ProductName);
            Console.WriteLine(String.Empty);
            
            if (args.Length > 0)
            {
                string answerFile = Path.GetFullPath(args[0]);

                if (File.Exists(answerFile))
                {
                    Answers ans = null;

                    using (FileStream fs = new FileStream(answerFile, FileMode.Open))
                    {
                        try
                        {
                            ans = XmlHelper.DeserializeObject(fs, typeof(Answers)) as Answers;
                        }
                        catch
                        {
                            ans = null;
                        }
                    }

                    if (ans != null)
                    {
                        // reading answer file
                        PrepareSubstitutions(ans);
                        
                        // preparing folder paths
                        string sourceFolder = null, targetFolder = null;

                        try
                        {
                            if ((args.Length > 1) && !String.IsNullOrWhiteSpace(args[1]))
                                targetFolder = Path.GetFullPath(args[1]);
                            else
                                targetFolder = Path.GetFullPath(@"..\..\Phone\Client\");

                            if ((args.Length > 2) && !String.IsNullOrWhiteSpace(args[2]))
                                sourceFolder = Path.GetFullPath(args[2]);
                            else
                                sourceFolder = Path.GetFullPath(@"..\Templates\");
                        }
                        catch
                        {
                            sourceFolder = null;
                            targetFolder = null;
                        }

                        if (!String.IsNullOrWhiteSpace(sourceFolder) && !String.IsNullOrWhiteSpace(targetFolder) &&
                            Directory.Exists(sourceFolder) && Directory.Exists(targetFolder))
                        {
                            // performing substitutions
                            ApplySubstitutions(Path.Combine(sourceFolder, File1), Path.Combine(targetFolder, File1));
                            ApplySubstitutions(Path.Combine(sourceFolder, File2), Path.Combine(targetFolder, @"Properties\" + File2));
                            ApplySubstitutions(Path.Combine(sourceFolder, File3), Path.Combine(targetFolder, @"Properties\" + File3));
                        }
                        else
                            Console.WriteLine("Error: unable to resolve file locations.");
                    }
                    else
                        Console.WriteLine("Error: unable to read answer file.");
                }
                else
                    Console.WriteLine("Error: answer file not found.");
            }
            else
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("BuildHelper.exe \"path to answer file\" [\"destination folder\"] [\"templates folder\"]");
            }
        }

        static void PrepareSubstitutions(Answers answers)
        {
            if (answers != null)
            {
                if ((answers.Substitutions != null) && (answers.Substitutions.Count > 0))
                {
                    foreach (KeyValuePair<string, string> substitution in answers.Substitutions)
                        substitutions.Add(
                            String.Format("@@{0}@@", substitution.Key), 
                            substitution.Value);
                }

                if ((answers.PlatformOptions != null) && (answers.PlatformOptions.Count > 0))
                {
                    StringBuilder po = new StringBuilder();

                    foreach (string platformOption in answers.PlatformOptions)
                    {
                        po.AppendFormat("    <Option>{0}</Option>", platformOption);
                        po.AppendLine();
                    }

                    substitutions.Add(PlatformOptionsValue, po.ToString());
                }
                else
                    substitutions.Add(PlatformOptionsValue, String.Empty);

                if ((answers.ClientOptions != null) && (answers.ClientOptions.Count > 0))
                {
                    StringBuilder co = new StringBuilder();

                    foreach (KeyValuePair<string, string> clientOption in answers.ClientOptions)
                    {
                        co.AppendFormat("    <Option Key=\"{0}\" Value=\"{1}\" />", clientOption.Key, clientOption.Value);
                        co.AppendLine();
                    }

                    substitutions.Add(ClientOptionsValue, co.ToString());
                }
                else
                    substitutions.Add(ClientOptionsValue, String.Empty);
            }
        }

        static void ApplySubstitutions(string template, string targetLocation)
        {
            Console.WriteLine("Processing: {0}", Path.GetFileName(targetLocation));

            if (File.Exists(template))
            {
                string[] lines = File.ReadAllLines(template);

                if (lines.Length > 0)
                {
                    using (StreamWriter writer = new StreamWriter(targetLocation, false))
                    {
                        foreach (string line in lines)
                            writer.WriteLine(ReplaceStrings(line));
                    }
                }
            }
        }

        static string ReplaceStrings(string input)
        {
            StringBuilder res = new StringBuilder(input);

            foreach (KeyValuePair<string, string> substitution in substitutions)
                res.Replace(substitution.Key, substitution.Value);

            return res.ToString();
        }
    }
}
