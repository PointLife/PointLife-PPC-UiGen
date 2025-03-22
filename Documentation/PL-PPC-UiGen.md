1. Import unitypackage
	![[PL_PPC_UiGen.unitypackage]]
2. Wait for Unity to Recompile
3. .uxml files need to be in Format [PluginName]\_[Category/Folder]\_[MenuName], eg `BetterBP_Estate_ManageSettings`
	This is to make sure that no names of elements conflict with other Menus or Plugins.

4. UI Objects Name needs to be set to the Variable Name
	![[Pasted image 20250321232252.png]]
5. If you want a Close Button it needs to be called "CloseMenu"
	![[Pasted image 20250321232407.png]]
6. Open PointLife-PPC-UiGen -> Generate UI Code
	![[Pasted image 20250321231514.png]]
	![[Pasted image 20250321232517.png]]
7. Select the Export Path ( A Folder in your Visual Studio Solution for your Plugin )
8. Enable DISABLE TEXT to show the names of the Fields
9. Accepted ( or change ) the suggested Changes to fix the Issues
	![[Pasted image 20250321232904.png]]
10. Press Build and Export Files
11. Add 0PPC-UiGen-BaseType.dll Refrence to your Plugin and into the Plugins Folder or Managed Folder
	![[0PPC-UiGen-BaseType.dll]]
```cs
<Reference Include="0PPC-UiGen-BaseType">
	<HintPath>$(BPDIR)\Plugins\0PPC-UiGen-BaseType.dll</HintPath>
</Reference>
```
12. If using SDK-type .csproj the exported Folder will be automatically incldued.
13. Make a new Class and add `using PointLife.UiGen.Scaffold;` and inherit from `scaffold_`
	![[Pasted image 20250321233648.png]]
14. Use Visual Studio Alt+Enter to Generate the Class
    ![[Pasted image 20250321234033.png]]
	Choose to override Open and Close ( if needed )
	![[Pasted image 20250321234313.png]]

You can now add Code
 ```cs
using BPEssentials.ExtensionMethods;
using BrokeProtocol.Entities;
using PointLife.UiGen.Scaffold;

namespace BPPlasmaGangs.Menus.UiToolkit.Menus
{
	public class ManageSettings : scaffold_BetterBP_Estate_ManageSettings
	{
		public ManageSettings(ShPlayer _player) : base(_player)
		{
		}

		public override void Open()
		{
			Fields.Text_Password.UpdateTo("123456789");     // Write last Password
			Fields.Toggle_AllowVisitors.UpdateTo(true);     // Write last Toggle

			base.Open();
		}

		public override void On_Save_Pressed()
		{
			Fields.UpdateAll(player).Then(() =>             // Get all Fields from the Client
			{
				player.TS("settings_saved");

				player.TS("setting_toggle", Fields.Toggle_AllowVisitors.CheckboxValue);
				player.TS("setting_password", Fields.Text_Password.Text);

				Close();    // Close Menu
			});
		}
	}
}

```

In a Command it can then be used like:
```cs
new ManageSettings(player).Open();
```