Console.WriteLine("Write dependency version start");

Directory.SetCurrentDirectory("../"); // move to repository root
const string TEX_TRANS_CORE_PACKAGE_DOT_JSON_PATH = "../TexTransCore/package.json";
var texTransCorePackageJson = System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(TEX_TRANS_CORE_PACKAGE_DOT_JSON_PATH));
if (texTransCorePackageJson is null) { throw new NullReferenceException(); }
var ttcVersion = texTransCorePackageJson["version"]!.GetValue<string>();
var ttcCode = texTransCorePackageJson["name"]!.GetValue<string>();


var tttPackageJsonPath = @"package.json";
var tttPackageJson = System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(tttPackageJsonPath));
if (tttPackageJson is null) { throw new NullReferenceException(); }

tttPackageJson["dependencies"]![ttcCode] = ttcVersion;
tttPackageJson["vpmDependencies"]![ttcCode] = "^" + ttcVersion;


var outOpt = new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.General) { WriteIndented = true };
File.WriteAllText(tttPackageJsonPath, tttPackageJson.ToJsonString(outOpt) + "\n");
Console.WriteLine("Write version exit!");
