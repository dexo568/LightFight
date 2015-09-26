====================================================================

PLEASE READ (2) BELOW FIRST, IF YOU ARE USING UNITY3D INDIE VERSION!

====================================================================


(1) INCLUDED SCENE:

[W] accelerate;
[SHIFT] + [W] accelerate faster;

[S] reverse accelerate;
[SHIFT] + [S] reverse accelerate faster;

[MOUSE MOVE] rotate camera;
[A] or [D] + [MOUSE MOVE] rotate differently (roll locked);

[SPACE] speed decrease until stop;

[ENTER] drops objects into the terrain (please enabled Physics in the Planet Inspector before using this key)

*** Physics is slow in high tesselation, especially if your videocard does not have Physx acceleration.
*** The included sample does not collide the camera with the terrain (but that is possible).
*** Futurely there will be an alternative faster collision detection that won't require physics to be enabled.


Thanks for supporting the extension!


--------

(2) SWITCHING BETWEEN UNITY INDIE AND UNITY PRO BUILDS:

There are two folders within Engine directory called

 UnityIndie and UnityPro

Inside these folders there are two files

 CNoiseFactory and CQuadtree

For now, if you are running Unity Indie you need to edit CNoiseFactory.cs and CQuadtree.cs and put false in their #if's

like

#if false

 ... code for Unity Pro ...

#endif

in both CNoiseFactory and CQuadtree

then put true in the #if's of CNoiseFactory_Indie and CQuadtree_Indie respectively.

Do the inverse to activate Unity Pro.

Please note that, after switching to Unity Indie, you'll need to readjust parameters to get a proper planet again, as they are implemented differently.

Also, Unity Indie version is still preliminar, and some work will still be done on this version, like proper lighting and overall better visual quality.

