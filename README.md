# Smart Hierarchy
Better hierarchy for Unity, made by community.\
Organize your scene properly, without getting lost in objects mess.

### Icons
![Icons](https://i.imgur.com/V12LjiY.png)

They help you to navigate visually around hierarchy by always showing the most important information.

Note: Icons are not attached to Prefabs root. This behaviour can be changed in Preferences.

### Folders
![Folders](https://i.imgur.com/SRm9pdB.png)

Folder is a special Editor-only object that has no components and no transform.\
It detaches it's children on scene process, so there is no overhead at runtime.\
[Best Practices: Transforms Optimization](https://unity.com/ru/how-to/best-practices-performance-optimization-unity#transforms) \
[Best Practices: Optimizing the Hierarchy](https://blogs.unity3d.com/ru/2017/06/29/best-practices-from-the-spotlight-team-optimizing-the-hierarchy/)

**Issue:** Currently folders are not displayed in Playmode.\
There is an option (in Preferences) to keep them in Editor, but beware of the overhead.

### Activation Toggle
![Activation Toggle](https://i.imgur.com/nv7aPE5.png)

No more need to select game object in order to (de)activate it.

## In progress:
* Components bar
* Double-click expanding
* Folder inspector

## Under investigation:
* Nesting branches
* Depth context
* Decorator/Fake folders
* Object preview
* Scene highlight
* Filtering

## Test status:
* Made in Unity 2019.4
* Tested with Unity 2020.1
* Not tested with sub-scenes
* Not tested with multiple hierarchy windows
* Not tested with other workflow extensions like Peek