# ML-Agents Experiments 🧠🤖

This repository is for *experimenting* with ML Agents & Unity.

## Requirements

You can follow the setup instructions for Windows [here](https://github.com/Unity-Technologies/ml-agents/blob/develop/docs/Installation.md). I would recommend goign through the **advanced local installtion** for development. This project also requires the experimental build of MLAgents which can be found [here](https://docs.unity3d.com/Packages/com.unity.ml-agents@2.3/manual/index.html).

## Todo

- [x] Explore Curiosity
- [x] Cirriculum Learning for tasks which ramp up in complexity 
- [x] Conditional Behaviours: Goal Signaling
- [x] Working w/ Multiple Brains
	- [x] Read & Wrtie .ONNX files
	- [x] Set .ONNX files to Agent during runtime
- [x] Extract training information
	- [x] Display that information in Unity (GUI)
- [ ] Flexible Training Scenarios
	- [ ] Investigate Adversarial Self-Play
	- [ ] Cooperative Multi-Agent Play
	- [ ] Competitive Multi-Agent
	- [ ] Ecosystem
- [ ] Explore Marathon Envs & Mujoco

## Documentation

### _Multiple Brain: Reading & Setting Models during runtime from a local directory_
You can find this method in `SwapModel.cs`. 
### _Extracting & Displaying trainging information_
![c_Monitor](https://github.com/Caileannn/ml-agents-simone/assets/25906839/06d45f96-3425-4daa-9fcf-52756017f5c8)

By using the `Monitor.cs` script, you can easily display information about the agent. I also added a display for the swapping of the models within a local directory.
