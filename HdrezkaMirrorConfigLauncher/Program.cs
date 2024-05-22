using HdrezkaMirrorSite;
using YamlDotNet.RepresentationModel;

YamlMappingNode yaml = ReadingYaml();

MirrorSiteOpener mirrorSiteOpener;

if (yaml["EmailIsSorted"].ToString() == "yes")
{
    mirrorSiteOpener = new(yaml["Login"].ToString(), yaml["Password"].ToString());
}
else
{
    mirrorSiteOpener = new MirrorSiteOpenerUnsorted(yaml["Login"].ToString(), yaml["Password"].ToString());
}

await mirrorSiteOpener.Open();

await Task.Delay(1000);

Environment.Exit(0);

static YamlMappingNode ReadingYaml()
{
    using StreamReader reader = new("config.yml");
    YamlStream yaml = [];
    yaml.Load(reader);
    YamlMappingNode root = (YamlMappingNode)yaml.Documents[0].RootNode;
    return root;
}