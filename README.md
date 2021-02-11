# Smart Hierarchy v0.2.9

Collection of QoL improvements for Unity Hierarchy.\
Designed with a native-feel and intuitiveness in mind.

**Note:** Extension is at the early development stage, so feel free to bug-hunt, provide feedback and ideas!\
Every contribution is highly appreciated! 😉

[![ko-fi](https://www.ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/L4L02M51R)

## Installation
![Installation](.github/images/Installation.png)

Customize plugin behaviour in "Preferences > Workflow > Smart Hierarchy".

## Features
### Icons
![Icons](.github/images/Icons.png)

Most important components are shown to help you orientate visually.\
Types Priority can be defined in Preferences.

### Collections
![Collections](.github/images/Collections.png)

*"GameObject / Create Collection".*

Collection is used to group objects in Editor without affecting runtime transform hierarchy.\
Children are detached during scene process.

[Best Practices: Transforms Optimization](https://unity.com/ru/how-to/best-practices-performance-optimization-unity#transforms) \
[Best Practices: Optimizing the Hierarchy](https://blogs.unity3d.com/ru/2017/06/29/best-practices-from-the-spotlight-team-optimizing-the-hierarchy/)

**Note:** There is an option in Preferences to keep collections in Playmode.

### Activation Toggle
![Activation Toggle](.github/images/ActivationToggle.png)

No more need to select game object in order to (de)activate it.

### Hover Preview
![Hover Preview](.github/images/HoverPreview.png)

Bindable keys: Alt / Shift / Ctrl.\
**Very Experimental!** Can be enabled in Preferences.

## In progress:
* Debug Log context
* Components Toolbar
* Entities transform optimizations
* Customizable GameObject context-menu
* Better Collection inspector

## Under investigation:
* Multi-Rename
* Nesting Branches
* Depth Context
* Local Hierarchy
* Collection Children Filter
* Scene highlight
* Preview Icons

## Test status:
* Made in Unity 2020.2 
* Tested with Unity 2019.4
* Not tested with sub-scenes
* Not tested with multiple hierarchy windows
* Not tested with other workflow extensions like Peek