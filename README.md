# House of Potions Prototype

**Unity Scene for Prototype**: "Project.unity"

**Target Device**: Android Phone Device

**Interaction Techniques:**

  - Hover, click, and move throughout the game with the Raycast Pointer and Joystick
  
  - Start game with Raycast and Joystick button "B" on Android

  - Access Potion Menu and what objects to collect with Joystick button "Menu" on Android

  - Click the Joystick power button to pause the game at anytime

  - When you want to hide from the NPC before getting caught, press Joystick button "X" on Android, the distance text assists on when users should hide from the NPC and the scene is a little darker when the user is in hidden mode and cannot move

  - If you get caught by the NPC, the game will end and you'll have to restart the game by clicking Joystick button "Y" on Android 
  
  - Tilt phone screen to the left for 5 seconds for a hint to appear about what the closest object is and what direction to look in (ahead, behind, left, right)
  
  - Grab objects with Joystick button "B" on Android when raycast hovers over an interactable object 
  
  - Open inventory menu with Joystick button "OK" on Android, use DropAll Ingredients if the Alchemy Pot is in view to drop all objects into it

  - Once potion is complete (all necessary ingredients have been added) Shake phone screen side to side to pour the potion into the bottles
  
  - Store potions in inventory with Joystick button "B" on Android when raycast hovers over an interactable object

  - Once entering a broken room, navigate to inventory potions individually and select potions with Joystick button "B" on Android
  
  - To drop potion in a room, make a nod with head while potion is grabbed (fix Library, Dining Room, Garden)

  - Once all rooms have been fixed, game has been completed

**Advanced Requirements:**

  - **Intelligent NPC**: The NPC is trained using Unity ML Agents, mapped the scene using Nav Mesh, and is trained to walk around the scene and find the user. Once the NPC spots the user based on the set FOV, it will follow the user to catch it and try to make the user lose the game. The NPC interacts dynamically with the player by attempting to catch it and interacts dynamically with the virtual environment by moving throughout the scene. It responds realistically to player actions by trying to find the player, and follow it when the NPC finds the player, as long as the player is not in hidden mode or hiding behind an object.  
  - **Mobile Sensors:** The different mobile sensors that we implemented are the following - 
    - **Hint**: If the user tilts their head to the left for 3 seconds when using the Android devices, the object closest to the user to collect will appear.
    - **Potion** Complete: Once all the ingredients have been added to the pot, for the potions to be transferred from the cauldron to the potion bottles, the user must shake their head left and right for the potions to appear on the table to the side of the pot.
    - **Fixing Rooms**: Once all the potions have been stored in the inventory, users can go to a broken room, access the inventory, select the potion, and nod their head up and down to drop the potion in the room.
  
  We selected the Intelligent NPCs & Mobile Sensors advanced requirements because we thought they would make our project more engaging, increase the difficulty, and fit well with our magical theme. We chose the Intelligent NPC as the knight to go well with our theme and have it "guard" the Broken House of Potions against the player. The motion-based interactions we included give the character a sense of magical capabilities to use when creating/using their potions.

**Github Information**:

  - Users: @AishwaryaSudarshan, @dheepsk, @aksharaganapathi, @sanjeet-v
  - Link: [https://github.com/AishwaryaSudarshan/HouseOfPotions](https://github.com/AishwaryaSudarshan/HouseOfPotions)

**Youtube Link**: 

- Prelim Prototype: [https://youtu.be/Z2CZQeiSFkQ](https://youtu.be/Z2CZQeiSFk)
- Final Demo: [https://youtu.be/poLaI7piNKE](https://youtu.be/poLaI7piNKE)
