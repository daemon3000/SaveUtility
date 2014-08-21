## Introduction
SaveUtility is an open-source save plugin for Unity, designed to save entire scenes. 
SaveUtility was influenced by [UnitySerializer ](http://whydoidoit.com/unityserializer) which unfortunately hasn’t received any recent updates.

**Works only on Windows, Mac OSX and Linux. Requires the latest version of Unity.**

## Installation
- Clone or download the project
- Open the project and export the two folders as a new unity package(you don’t have to export the examples if you don’t want to)
- Import the unity package in your game project

The package comes with examples and scripts that show you how to use the API.

## Known Issues
- You can't duplicate game-objects that have a *GameObjectSerializer/UniqueIdentfier* component. If you do you'll end up with duplicate IDs so you'll need to remove the *GameObjectSerializer/UniqueIdentifier* component from the duplicate and add it again.
- If you have a prefab and the prefab has a *GameObjectSerializer/UniqueIdentifier* component you can't use the **Apply** button to apply instance changes to the prefab. If you do it will override the IDs of all instances of that prefab.

## Getting Started
Before you start using SaveUtility I recommend you read the quick start guide [here](https://docs.google.com/document/d/1iLQm2USVTSERdJEx-rLWS9Kfh4-EyiBoFBqnOk9QZqA/edit).

## License
SaveUtility is released under the [MIT license](http://opensource.org/licenses/MIT). You can find a copy of the license in the LICENSE file included in the SaveUtility source distribution.