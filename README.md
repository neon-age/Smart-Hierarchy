# Smart-Hierarchy
Human readable hierarchy with icons and folders for Unity.\
Helps you to organize your scene properly, without getting lost in objects mess.

![Smart Hierarchy](https://i.imgur.com/oebJPv9.png)

## Test status:
* Made in Unity 2019.4
* Not tested with Unity 2020.1
* Not tested with sub-scenes
* Not tested with other workflow extensions like Peek


## How folder works?
Folder disables transform and detaches it's children on scene process. That's it

## How did you set icons in hierarchy?
Directly into hierarchy tree view items.\
It doesn't affect objects actual icon (setted in inspector).

### And what it shows me instead of cube?
It smartly decides which component is main and shows you it's icon to help you navigate easily.\
You'll always see where's all you lights, enemies, particles and cameras.


## Known issues:
* Folder inspector is ugly
* First folder icon jittering on collapse
* First folder getting expanded after exiting playmode
* Playmode hierarchy depth does not account sub-folders
* Playmode reordering doesn't update view depth

## Not implemented yet:
* Folding in playmode
* Warning about components on folder object
* Create folder context menu

## Todo:
* Full customization
* Select all child components of type in folder inspector
* Object components on hover
* Object preview icon
