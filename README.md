## Introduction
SaveUtility is an open-source save plugin for Unity, designed to save entire scenes. 
SaveUtility was influenced by [UnitySerializer ](http://whydoidoit.com/unityserializer) which unfortunately hasn’t received any recent updates.

**Compatible with Windows 7/8 Desktop, Windows 8 Store, Mac OSX and Linux. Requires the latest version of Unity.
The package comes with examples and scripts that show you how to use the API.**

## Known Issues
- You can't use *Ctrl(CMD) + D* to duplicate game-objects with a *GameObjectSerializer* or *UniqueIdentifier* component because you'll end up with duplicate IDs. You have to use the **Duplicate** button located in the *GameObjectSerializer/UniqueIdentifier* inspector.
- If you have a prefab with a *GameObjectSerializer* or a *UniqueIdentifier* component you can't use the default **Apply** button to apply instance changes to the prefab. You'll need to use the custom **Apply Changes** button located in the *GameObjectSerializer/UniqueIdentifier* inspector.
- If you have a prefab with a *GameObjectSerializer* or a *UniqueIdentifier* component you can't use the default **Revert** button to revert the instance to prefab state. You'll need to use the custom **Revert Changes** button located in the *GameObjectSerializer/UniqueIdentifier* inspector.

## Getting Started
Before you start using SaveUtility I recommend you read the quick start guide [here](https://docs.google.com/document/d/1iLQm2USVTSERdJEx-rLWS9Kfh4-EyiBoFBqnOk9QZqA/edit).

## License
SaveUtility is released under the [MIT license](http://opensource.org/licenses/MIT). You can find a copy of the license in the LICENSE file included in the SaveUtility source distribution.