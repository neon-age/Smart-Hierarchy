# Smart Hierarchy v0.2.6
[![ko-fi](https://www.ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/L4L02M51R)

Better hierarchy for Unity, made by community.\
Organize your scene properly, without getting lost in objects mess.

**Note:** Extension is in early development stage, so feel free to bug-hunt, provide feedback and ideas!\
Every contribution is highly appreciated!

### Icons
![Icons](https://i.imgur.com/V12LjiY.png)

They help you to navigate visually around hierarchy by always showing the most important information.

Note: Icons are not attached to Prefabs root. This behaviour can be changed in Preferences.

### Collections
![Collections](https://i.imgur.com/9anU7QQ.png)

*"GameObject / Create Collection".*

Collection is a special Editor-only object that has no components and no transform.\
It detaches it's children on scene process, so there is no overhead at runtime.\
[Best Practices: Transforms Optimization](https://unity.com/ru/how-to/best-practices-performance-optimization-unity#transforms) \
[Best Practices: Optimizing the Hierarchy](https://blogs.unity3d.com/ru/2017/06/29/best-practices-from-the-spotlight-team-optimizing-the-hierarchy/)

**Issue:** Currently they are not displayed in Playmode.\
There is an option (in Preferences) to keep them in Editor, but beware of the overhead.

### Activation Toggle
![Activation Toggle](https://i.imgur.com/nv7aPE5.png)

No more need to select game object in order to (de)activate it.

### Hover Preview
![Hover Preview](https://i.imgur.com/CAN5uKL.png) 

Hold "magnify" key to preview hovered object.\
Bindable keys: Alt / Shift / Ctrl.\
**Very Experimental!** Can be enabled in Preferences.

## In progress:
* Components bar
* Nesting branches
* Double-click expanding
* Folder inspector

## Under investigation:
* Depth context
* Decorator/Fake folders
* Scene highlight
* Filtering

## Test status:
* Made in Unity 2019.4
* Tested with Unity 2020.1
* Not tested with sub-scenes
* Not tested with multiple hierarchy windows
* Not tested with other workflow extensions like Peek