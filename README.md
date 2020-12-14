# Smart Hierarchy v0.2.9
[![ko-fi](https://www.ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/L4L02M51R)

Collection of QoL improvements for Unity Hierarchy.\
Designed with a native-feel and intuitiveness in mind.

**Note:** Extension is in early development stage, so feel free to bug-hunt, provide feedback and ideas!\
Every contribution is highly appreciated!

## Installation
![Installation](https://i.imgur.com/2e4baUn.png)

No additional setup required!\
Customize plugin behaviour in "Preferences > Workflow > Smart Hierarchy".

## Features
### Icons
![Icons](https://i.imgur.com/aVIzuCy.png)

Last object component is shown by default. You can define "Types Priority" in Preferences.\
Default icon is shown only when object is "Empty", i.e. it has no components and transform is not changed.

### Collections
![Collections](https://i.imgur.com/F2Z8lVz.png)

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

Hold modification key to preview hovered object.\
Bindable keys: Alt / Shift / Ctrl.\
**Very Experimental!** Can be enabled in Preferences.

## In progress:
* Components bar
* Single-click expanding
* Folder inspector
* "Cut" shortcut

## Under investigation:
* Nesting branches
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