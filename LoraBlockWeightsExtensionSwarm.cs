
using Newtonsoft.Json.Linq;
using SwarmUI.Builtin_ComfyUIBackend;
using SwarmUI.Core;
using SwarmUI.Text2Image;
using SwarmUI.Utils;

namespace SwarmUI.LoraBlockWeightsExtension;

public class LoraBlockWeightsExtension : Extension
{
    public static T2IRegisteredParam<double> StrengthClip, StrengthModel, A, B;

    public static T2IRegisteredParam<string> LoraName, Weights;

    public static T2IParamGroup LoraBlockLoaderGroup;

    public override void OnInit()
    {
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

        StrengthModel = T2IParamTypes.Register<double>(new("[LBW] Clip Strength", "[Lora Loader (Block Weight)]\n. Lora Model Strength",
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
        // CFGScaleMin = T2IParamTypes.Register<double>(new("[DT] CFG Scale Minimum", "[Dynamic Thresholding]\nCFG Scale minimum value (for non-constant CFG mode).",
        //     "0", Min: 0, Max: 100, Group: DynThreshGroup, FeatureFlag: "dynamic_thresholding", OrderPriority: 4,
        //     Examples: ["0", "1", "2", "5"]
        //     ));
        // MimicScaleMode = T2IParamTypes.Register<string>(new("[DT] Mimic Scale Mode", "[Dynamic Thresholding]\nMode for the Mimic Scale scheduler.",
        //     "Constant", Group: DynThreshGroup, FeatureFlag: "dynamic_thresholding", OrderPriority: 5,
        //     GetValues: (_) => ["Constant", "Linear Down", "Half Cosine Down", "Cosine Down", "Linear Up", "Half Cosine Up", "Cosine Up", "Power Up", "Power Down", "Linear Repeating", "Cosine Repeating"]
        //     ));
        // MimicScaleMin = T2IParamTypes.Register<double>(new("[DT] Mimic Scale Minimum", "[Dynamic Thresholding]\nMimic Scale minimum value (for non-constant mimic mode).",
        //     "0", Min: 0, Max: 100, Group: DynThreshGroup, FeatureFlag: "dynamic_thresholding", OrderPriority: 6,
        //     Examples: ["0", "1", "2", "5"]
        //     ));
        // SchedulerValue = T2IParamTypes.Register<double>(new("[DT] Scheduler Value", "[Dynamic Thresholding]\nIf either scale scheduler is 'Power', this is the power factor.\nIf using 'repeating', this is the number of repeats per image. Otherwise, it does nothing.",
        //     "4", Group: DynThreshGroup, FeatureFlag: "dynamic_thresholding", OrderPriority: 7,
        //     Examples: ["2", "4", "8"]
        //     ));
        // SeparateFeatureChannels = T2IParamTypes.Register<bool>(new("[DT] Separate Feature Channels", "[Dynamic Thresholding]\nWhether to separate the feature channels.\nNormally leave this on. I think it should be off for RCFG?",
        //     "true", Group: DynThreshGroup, FeatureFlag: "dynamic_thresholding", OrderPriority: 8
        //     ));
        // ScalingStartpoint = T2IParamTypes.Register<string>(new("[DT] Scaling Startpoint", "[Dynamic Thresholding]\nWhether to scale relative to the mean value or to zero.\nUse 'MEAN' normally. If you want RCFG logic, use 'ZERO'.",
        //     "MEAN", Group: DynThreshGroup, FeatureFlag: "dynamic_thresholding", OrderPriority: 9, GetValues: (_) => ["MEAN", "ZERO"]
        //     ));
        // VariabilityMeasure = T2IParamTypes.Register<string>(new("[DT] Variability Measure", "[Dynamic Thresholding]\nWhether to use standard deviation ('STD') or thresholded absolute values ('AD').\nNormally use 'AD'. Use 'STD' if wanting RCFG logic.",
        //     "AD", Group: DynThreshGroup, FeatureFlag: "dynamic_thresholding", OrderPriority: 10, GetValues: (_) => ["AD", "STD"]
        //     ));
        // InterpolatePhi = T2IParamTypes.Register<double>(new("[DT] Interpolate Phi", "[Dynamic Thresholding]\n'phi' interpolation factor.\nInterpolates between original value and DT value, such that 0.0 = use original, and 1.0 = use DT.\n(This exists because RCFG is bad and so half-removing it un-breaks it - better to just not do RCFG).",
        //     "1", Min: 0, Max: 1, Step: 0.05, Group: DynThreshGroup, FeatureFlag: "dynamic_thresholding", OrderPriority: 11,
        //     Examples: ["0", "0.25", "0.5", "0.75", "1"]
        //     ));
        // The actual implementation: add the node as a step in the workflow generator.
        WorkflowGenerator.AddStep(g =>
        {
            // Only activate if parameters are used - because the whole group toggles as once, any one parameter being set indicates the full set is valid (excluding corrupted API calls)
            if (g.UserInput.TryGet(Weights, out string weights))
            {
                // This shouldn't be possible outside of corrupt API calls, but check just to be safe
                if (!g.Features.Contains("lora_block_weights_loader"))
                {
                    throw new SwarmUserErrorException("Lora Block Weights Loader, but feature isn't installed");
                }
                string newNode = g.CreateNode("LoraLoaderBlockWeight //Inspire", new JObject()
                {
                    ["model"] = g.FinalModel,
                    ["mimic_scale"] = mimicScale,
                    ["threshold_percentile"] = g.UserInput.Get(ThresholdPercentile),
                    ["mimic_mode"] = g.UserInput.Get(MimicScaleMode),
                    ["mimic_scale_min"] = g.UserInput.Get(MimicScaleMin),
                    ["cfg_mode"] = g.UserInput.Get(CFGScaleMode),
                    ["cfg_scale_min"] = g.UserInput.Get(CFGScaleMin),
                    ["sched_val"] = g.UserInput.Get(SchedulerValue),
                    ["separate_feature_channels"] = g.UserInput.Get(SeparateFeatureChannels) ? "enable" : "disable",
                    ["scaling_startpoint"] = g.UserInput.Get(ScalingStartpoint),
                    ["variability_measure"] = g.UserInput.Get(VariabilityMeasure),
                    ["interpolate_phi"] = g.UserInput.Get(InterpolatePhi)
                });
                // Workflow additions generally only do anything if a key passthrough field is updated.
                // In our case, we're replacing the Model node, so update FinalModel to point at our node's output.
                g.FinalModel = [newNode, 0];
            }
            // See WorkflowGeneratorSteps for definition of the priority values.
            // JAKETODO, ask monkey what number i should use here for lora...
            // -5.5 puts this before the main Sampler, but after most other model changes (eg controlnet)
        }, -5.5);
    }
}
