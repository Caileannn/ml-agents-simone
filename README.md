## To-do List for 'exp-simone' ##

__13.11 -> 17.11__

- [x] Train the new default chair model on all the different Scenarios
- [x] Add Controller as an Option
- [ ] Toggle different Models based on Proximity or Collision
- [x] Toggle for Projectiles
- [ ] Automatic Getup Threshold configuration
- [ ] Toggle 2D Depth Image (Preview/Full Window)

__20.11 -> 25.11__

- [ ] Train on Stairs
- [ ] Experiment with different Surface Properties
- [x] Fix Controller to make it more intutative


## Notes ##

You can aim projectiles by pressing ``f`` when the Cannon is enabled:

![image](https://github.com/Caileannn/ml-agents-simone/assets/25906839/469e3fe1-cb9b-4929-838f-81444a3bc6c8)

Continous Projectiles can be enabled which spawn at a ``rate``.

All of the .ONNX models can be found under:

![image](https://github.com/Caileannn/ml-agents-simone/assets/25906839/7cd2c250-1ef6-4609-a973-ded4f3348961)

The different types of terrains are stored under here:

![image](https://github.com/Caileannn/ml-agents-simone/assets/25906839/a9542e0d-e031-4243-958b-8e54f363b591)

You can enable whichever one you prefer!

When you wish to try out the trained model, you should switch the ``max steps`` under the Walker Component to zero:

![image](https://github.com/Caileannn/ml-agents-simone/assets/25906839/9106527b-1a0c-47a2-b7a3-55e59dd0c3c1)

For the **Controller** you can use it by enabling it here:

![image](https://github.com/Caileannn/ml-agents-simone/assets/25906839/f8bcd33a-d9b5-4e5e-bd45-467aa68d2d4d)

Along with some extra settings for the Mesh & Material of the cursor.

Use the Up arrow to move the controller forward and the Left & Right to rotate it around the axis of the Chair





