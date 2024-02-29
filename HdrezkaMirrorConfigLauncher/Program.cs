using System.IO;
using HdrezkaMirrorSite;
using YamlDotNet.RepresentationModel;

namespace HdrezkaMirrorConfigLauncher
{
    class Program
    {
        static void Main()
        {
            YamlMappingNode yaml = ReadingYaml();
            if (yaml["EmailIsSorted"].ToString() == "yes")
            {
                new MirrorSiteOpener(yaml["Login"].ToString(), yaml["Password"].ToString());
            }
            else
            {
                new MirrorSiteOpenerUnsorted(yaml["Login"].ToString(), yaml["Password"].ToString());
            }
        }

        static YamlMappingNode ReadingYaml()
        {
            using (StreamReader reader = new StreamReader("config.yml"))
            {
                YamlStream yaml = new YamlStream();
                yaml.Load(reader);
                YamlMappingNode root = (YamlMappingNode)yaml.Documents[0].RootNode;
                return root;
            }
        }
    }
}