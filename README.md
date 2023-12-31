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
	- [x] Investigate Adversarial Self-Play
	- [ ] Cooperative Multi-Agent Play
	- [ ] Competitive Multi-Agent
	- [ ] Ecosystem
- [ ] Explore Marathon Envs & Mujoco
- [x] Instancing Agents

## Documentation

### __Multiple Brain: Reading & Setting Models during runtime from a local directory__
You can find this method in `SwapModel.cs`. 

### __Extracting & Displaying training information__
![c_Monitor](https://github.com/Caileannn/ml-agents-simone/assets/25906839/06d45f96-3425-4daa-9fcf-52756017f5c8)

By using the `Monitor.cs` script, you can easily display information about the agent. I also added a display for the swapping of the models within a local directory.

### Random Actuations
![c_RandomActuations](https://github.com/Caileannn/ml-agents-simone/assets/25906839/6882c0bb-54fb-44a5-b479-42c8ca97cbe2)

When looking at what information to extract, the actions performed by the agent at each step is a float value between -1.0, 1.0. For this example, I added a small amount of noise to each action. I almost feel sad for the agent.. an odd form of computational torture, controlled by forces ubeknownest to it.

### Instancing Agents
![c_basespawn](https://github.com/Caileannn/ml-agents-simone/assets/25906839/4b826e3a-a1f0-4a8c-8c0f-93a5d66897f7)

Experiments with instancing in Unity for Agents. The instance spawner allows for unqiue agents to spawn into a scene. There was a ton of issues with instancing agents which inherits the Agent class, as it would initialise twice, meaning double the body parts in the joint dictionary, and leading to the number of observations increasing - which is never good.. The reason why I built this was because it might be a nice way to visualise past evolutions/models.

![c_rapidspawn](https://github.com/Caileannn/ml-agents-simone/assets/25906839/cd8afa1f-fe85-4b47-b7e2-cdc8662cafb9)



