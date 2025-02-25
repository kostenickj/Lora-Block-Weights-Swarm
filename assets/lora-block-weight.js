postParamBuildSteps.push(() => {
    let group = document.getElementById('input_group_loraloaderblockweight');
    if (group) {
        if (!currentBackendFeatureSet.includes('lora_block_weights_loader')) {
            group.append(createDiv(`lora_block_weights_loader_install_button`, 'keep_group_visible', `<button class="basic-button" onclick="installFeatureById('lora_block_weights_loader', 'lora_block_weights_loader_install_button')">Install</button>`));
        }
    }
});