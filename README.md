# Smart Hierarchy for Unity
Brings intuitive features and important visual elements in the right place.

• [View Changelog](CHANGELOG.md) • [🗺 Roadmap](https://github.com/neon-age/Smart-Hierarchy/projects/1) • [💬 Discussions](https://github.com/neon-age/Smart-Hierarchy/discussions) •

## Installation
Requires Unity **2019.4** or higher.
<details>
<summary>Add from <a href="https://openupm.com/packages/com.av.smart-hierarchy/">OpenUPM</a> <em>| via scoped registry or <a href="https://openupm.com/packages/com.av.smart-hierarchy/#modal-packageinstaller">package installer</a>, recommended</em></summary>
  
&emsp;To add a package via scoped registry:
  
- Open `Edit/Project Settings/Package Manager`
- Add a new Scoped Registry:
  ```
  Name: OpenUPM
  URL:  https://package.openupm.com/
  Scope(s): com.av
  ```
- Open `Window/Package Manager`
- Click <kbd>+</kbd>
- <kbd>Add from Git URL</kbd>
- `com.av.smart-hierarchy` <kbd>Add</kbd>
</details>

<details>
<summary>Add from GitHub | <em>not recommended, no updates </em></summary>
  
- Open `Window/Package Manager`
- Click <kbd>+</kbd>
- <kbd>Add from Git URL</kbd>
- `https://github.com/neon-age/Smart-Hierarchy.git` `#branch-name` <kbd>Add</kbd>

&emsp;Note that you won't be able to receive updates through Package Manager this way, you'll have to update manually.
</details>

Customization is available in `Preferences > Workflow > Smart Hierarchy`.

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

\#ProTip: Swipe to (de)activate multiple objects!

### Hover Preview
![Hover Preview](.github/images/HoverPreview.png)

Bindable keys: Alt / Shift / Ctrl.\
**Very Experimental!** Can be enabled in Preferences.

## Participation
Every contribution is highly appreciated — feel free to bug-hunt, provide feedback and ideas!\
Savvy enough to pull a new feature? I'll be happy to review your code and design UX together!

## Sharing is caring
I want to share my tools with everyone, without hiding behind a paywall.\
If you like my work and feel generous — consider supporting me financially!

[![ko-fi](https://www.ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/L4L02M51R)
[![patreon](.github/become-a-patron.png)](https://www.patreon.com/neonage?fan_landing=true)

![ko-fi](.github/coffee%20cup.png)
