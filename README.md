# TilemapEditor
TilemapEditor made with MonoGame. Original implementation is from Dante3085/EVCMonoGame project.

# How to create Visual Studio project
1. Clone repository somewhere on your hard drive.
2. Create a Visual Studio project with "MonoGame Windows Project" template inside the cloned folder.
   When creating the project check "Place solution and project in the same directory"
3. Copy everything from the project folder into the folder one level up (the cloned folder)
   Don't replace the Content.mgcb file!
4. In the opened Visual Studio project under the Solution Explorer -> References -> Add Reference -> Assemblies
   add System.Windows.Forms.
5. Right click on the Project name in the Solution Explorer -> Properties -> Application: Set Target framework
   to 4.7.2
6. Right click on the Project name in the Solution Explorer -> Manage Nuget Packages: Browse "System.Text.Json"
   and add it with version 4.7.2.
7. At the top of the solution explorer click the button "Shwo all files" and by right clicking they now visible
   but greyed out folder "src" include it in the project.
8. Remove the .cs files "Game1.cs" and "Program.cs" outside of the src folder.
9. Open Content-Pipeline-Tool with the existing Content.mgcb and add the missing files to the folders like in the tool.

# TilemapEditor Demo
![](TilemapEditor.gif)
