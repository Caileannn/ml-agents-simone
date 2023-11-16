# To-do List for 'exp-simone' #

__13.11 -> 17.11__

- [x] Train the new default chair model on all the different Scenarios
- [x] Add Controller as an Option
- [x] Toggle different Models based on Proximity or Collision
- [x] Toggle for Projectiles
- [x] Automatic Getup Threshold configuration
- [x] Toggle 2D Depth Image (Preview/Full Window)

__20.11 -> 25.11__

- [ ] Train on Stairs
- [ ] Experiment with different Surface Properties
- [x] Fix Controller to make it more intutative


# Notes #

## Projectiles ##

You can aim projectiles by pressing ``f`` when the Cannon is enabled:

![image](https://github.com/Caileannn/ml-agents-simone/assets/25906839/469e3fe1-cb9b-4929-838f-81444a3bc6c8)

Continous Projectiles can be enabled which spawn at a ``rate``.

### Location of Models ###

All of the .ONNX models can be found under:

![image](https://github.com/Caileannn/ml-agents-simone/assets/25906839/7cd2c250-1ef6-4609-a973-ded4f3348961)

### Terrain ###

The different types of terrains are stored under here:

![image](https://github.com/Caileannn/ml-agents-simone/assets/25906839/a9542e0d-e031-4243-958b-8e54f363b591)

You can enable whichever one you prefer!

### Agent setup for Inference ###

When you wish to try out the trained model, you should switch the ``max steps`` under the Walker Component to zero:

![image](https://github.com/Caileannn/ml-agents-simone/assets/25906839/9106527b-1a0c-47a2-b7a3-55e59dd0c3c1)

### Controller ###

For the **Controller** you can use it by enabling it here:

![image](https://github.com/Caileannn/ml-agents-simone/assets/25906839/f8bcd33a-d9b5-4e5e-bd45-467aa68d2d4d)

Along with some extra settings for the Mesh & Material of the cursor.

Use the Up arrow to move the controller forward and the Left & Right to rotate it around the axis of the Chair

### Depth Camera ###

I have added a new Depth camera to the scene. By using a shader on a camera depth texture, you will be able to have a higher quality version of the agents input.
This can be found under ```cameras/DepthCamera```. 

![image](https://github.com/Caileannn/ml-agents-simone/assets/25906839/2d7d2566-340e-421f-9aac-ad319b798cbb)

You can also disable the smaller depth window by going to ```ui/RawImage``` and disabling that.

### Model Swapping ###

I have added the ability to be able to swap models during runtime. You can do this by enabling the function ```Model Swap``` in the ```Walker``` script.

![image](https://github.com/Caileannn/ml-agents-simone/assets/25906839/edd60fae-cb9d-448d-b0d2-3ff6a51f78c4)

The list of model is stored in the project under ```Model Swapper/ModelList```, and you can dynamically add new models in there.

You can press ```O``` & ```P``` to cycle through the models during runtime.

I have also set up a function titled ```Switch Model After Falling``` which applies the ```Getup``` model when the agent falls over, and returns to the last model it was using before it fell over.

Lastly, I have added a ```Proximity Swapper```. This is an object that can trigger the agent to swap it model to any model in the ```ModelList``` folder when it enters its collider. Once the agent leaves the collider, it returns to its past model before it entered.

You can duplicate this and place them in different locations.

![image](https://github.com/Caileannn/ml-agents-simone/assets/25906839/b82f8a18-770e-4070-9459-893f9680c889)

![image](https://github.com/Caileannn/ml-agents-simone/assets/25906839/579709c6-e922-4511-8b9d-6c311ca599db)

You can add a tag to the object to state which Model you want the agent to swap to. This list is dynamic, and we can add to it as we wish.







