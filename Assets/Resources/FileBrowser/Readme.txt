Please read !

	-- EDITOR

Starting from an empty scene, simply create a new canvas and drag-and-drop the prefab on it. The file browser will be displayed and working, but you the have to setup how to open it.
in the Test scene, if you click on "Cancel", simply press the spacebar to display the menu again. take a look at the "TestInput" script to see the simple setup. 

	-- BUILDING

The script use a System library (System.Drawing) that need to be included when building. For the thumbnails display to be able to work when building, follow these steps :

1) Go to Edit > Project Settings > Player
2) Look under Other Settings > Optimization
3) Change ".NET 2.0 Subset" to ".NET 2.0"

MAC : 
4) After you've build the Application, open the ".app/Contents" directory.
5) Create a "Plugins" directory there and copy the file "libgdiplus.dylib" from the "lib" directory of Mono 2.10.8 for OSX.
6) Now rename the file "libgdiplus.dylib" to "gdiplus.dll.bundle".

	-- Please consider letting a review

Do not hesitate to take a look at the source code and contact me if you have any question.
I tried to make it simple, but... well, that's my first asset, so it may not be as obvious as I think it is.
Good luck and please give me a feedback on the asset. I will gladly take any advice or comment to improve it.