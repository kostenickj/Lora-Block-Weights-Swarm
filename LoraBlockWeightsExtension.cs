
using Newtonsoft.Json.Linq;
using SwarmUI.Builtin_ComfyUIBackend;
using SwarmUI.Core;
using SwarmUI.Text2Image;
using SwarmUI.Utils;

namespace Kostenickj.Extensions.LoraBlockWeightsExtension;

public class LoraBlockWeightsExtension : Extension
{
    public static T2IRegisteredParam<double> StrengthClip, StrengthModel, A, B;

    public static T2IRegisteredParam<string> LoraName, Weights;

    public static T2IParamGroup LoraBlockLoaderGroup;

    public override void OnInit()
    {
        Logs.Info("LoraBlockWeightsExtension has started.");
        // Add the installable info for the comfy node backend
        InstallableFeatures.RegisterInstallableFeature(new("Lora Loader (Block Weight)", "lora_block_weights_loader", "https://github.com/ltdrdata/ComfyUI-Inspire-Pack", "ltdrdata", "This will install Comfy UI Inspire pack by ltdrdata.\nDo you wish to install?"));
        // Add the JS file, which manages the install button for the comfy node
        ScriptFiles.Add("assets/lora-block-weight.js");
        // Track a feature ID that will automatically be enabled for a backend if the comfy node is installed
        ComfyUIBackendExtension.NodeToFeatureMap["LoraLoaderBlockWeight //Inspire"] = "lora_block_weights_loader";
        // Build the actual parameters list for inside SwarmUI, note the usage of the feature flag defined above
        LoraBlockLoaderGroup = new("Lora Loader (Block Weight)", Toggles: true, Open: false, IsAdvanced: true);

        StrengthClip = T2IParamTypes.Register<double>(new("[LBW] Clip Strength", "[Lora Loader (Block Weight)]\n. Lora Clip Strength",
            "1", Group: LoraBlockLoaderGroup, FeatureFlag: "lora_block_weights_loader", OrderPriority: 1,
            Examples: ["0", "1", ".75"]
            ));

        StrengthModel = T2IParamTypes.Register<double>(new("[LBW] Model Strength", "[Lora Loader (Block Weight)]\n. Lora Model Strength",
             "1", Group: LoraBlockLoaderGroup, FeatureFlag: "lora_block_weights_loader", OrderPriority: 1,
             Examples: ["0", "1", ".75"]
         ));

        A = T2IParamTypes.Register<double>(new("[LBW] A", "[Lora Loader (Block Weight)]\n. A Value",
                 "4", Group: LoraBlockLoaderGroup, FeatureFlag: "lora_block_weights_loader", OrderPriority: 1,
                 Examples: ["0", "4", ".75"]
             ));

        B = T2IParamTypes.Register<double>(new("[LBW] B", "[Lora Loader (Block Weight)]\n. B Value",
            "4", Group: LoraBlockLoaderGroup, FeatureFlag: "lora_block_weights_loader", OrderPriority: 1,
            Examples: ["0", "4", ".75"]
        ));

        LoraName = T2IParamTypes.Register<string>(new("[LBW] Lora Name", "[Lora Loader (Block Weight)]\n. Lora Name",
            "", Group: LoraBlockLoaderGroup, FeatureFlag: "lora_block_weights_loader", OrderPriority: 3,
            GetValues: (sesh) => T2IParamTypes.CleanModelList(Program.T2IModelSets["LoRA"].ListModelNamesFor(sesh)), Clean: (_, s) => T2IParamTypes.CleanModelNameList(s)
            ));


        Weights = T2IParamTypes.Register<string>(new("[LBW] Block Weights", "[Lora Loader (Block Weight)]\n. Block Weights",
            "1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1", Group: LoraBlockLoaderGroup, FeatureFlag: "lora_block_weights_loader",
            OrderPriority: 3
      ));

        // TODO, not sure if this needs to be a model gen step or regular step....
        WorkflowGenerator.AddStep(g =>
        {
            // Only activate if parameters are used - because the whole group toggles as once, any one parameter being set indicates the full set is valid (excluding corrupted API calls)
            if (g.UserInput.TryGet(LoraName, out string loraName))
            {
                // This shouldn't be possible outside of corrupt API calls, but check just to be safe
                if (!g.Features.Contains("lora_block_weights_loader"))
                {
                    throw new SwarmUserErrorException("Lora Block Weights Loader, but feature isn't installed");
                }

                if (String.IsNullOrEmpty(loraName))
                {
                    throw new SwarmUserErrorException("Lora Block Weights Loader: loraName is required");
                }

                Logs.Info("Lora Name" + loraName);
                var loraNameSafetensors = loraName + ".safetensors";

                string newNode = g.CreateNode("LoraLoaderBlockWeight //Inspire", new JObject()
                {
                    ["model"] = g.FinalModel,
                    ["clip"] = g.FinalClip,
                    ["lora_name"] = loraNameSafetensors,
                    ["strength_model"] = g.UserInput.Get(StrengthModel),
                    ["strength_clip"] = g.UserInput.Get(StrengthClip),
                    ["A"] = g.UserInput.Get(A),
                    ["B"] = g.UserInput.Get(B),
                    ["block_vector"] = g.UserInput.Get(Weights),
                    ["inverse"] = false,
                    ["seed"] = 0,
                    ["preset"] = "FLUX-DBL-ALL:1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1"
                });
                // Workflow additions generally only do anything if a key passthrough field is updated.
                // In our case, we're replacing the Model node, so update FinalModel to point at our node's output.
                g.FinalModel = [newNode, 0];
            }
            // See WorkflowGeneratorSteps for definition of the priority values.
        }, -14.5);
    }
}
