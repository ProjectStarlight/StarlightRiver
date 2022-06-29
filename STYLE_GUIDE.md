=========================================================================================================================================================

Section 0: Preface

0.1: Introduction

	0.1.1: Welcome
		Welcome to the official Starlight River style guide written in beautiful plaintext. The goal of this document
		is to provide an outline for structuring source code for the Mod which will be consistent with the rest of the
		project, in order to ensure everything is consistent and readable at a glance.

	0.1.2: Contact
		If you find an issue in this guide or wish to make a suggestion, please contact me on Discord (ScalarVector#9106)
		or via github.

=========================================================================================================================================================

Section 1: File and Class structure

1.1: File structure

	1.1.1: CS source files
		.cs Source files for content should be placed in a path starting with the Content directory,followed by it's
		content type, followed by its artistic theme or set identity. The file name shoud consist of a prefix 
		specifying the specific content type (if applicable), the name of the primary class of the file (see below
		for specifications for classes with multiple files), and if a partial class, a suffix describing the
		function of the specific methods of that part of the class.

		the file should then appear as the form of:

		Content/[ContentType]/[Theme]/{SubType.}[InternalName]{.MethodDescriptor}

		with substrings in brackets being optional based on context.

		For example, a source file for the Vitric Pickaxe would be: "Content/Items/Vitric/Tools.VitricPickaxe"

		.cs Source files for larger systems or loaders should be placed in a path starting the the Core directory,
		with loaders being placed in the loader subdirectory. source files should have a descriptive name of the
		system they manage and the type of content it effects.
	
		the StarlightRiver.cs source file should exist in the top level directory as required by TML.

	1.1.2: FX source files and XNB compiled shader binaries
		.fx Shader source files should be placed in the Effects directory directly, along with the compiled .xnb
		binaries. This is a requirement of the shader autoloader. You should always include .fx source files with
		every shader, never just the compiled binary.

	1.1.3: texture assets
		All texture assets should be of .png format, and within the Assets directory. Texture assets used only by
		a single source file or used as the primary texture of a piece of content in a source file (for example,
		the Item texture for the vitric pickaxe) should be placed in a directory directly mirroring that of the
		source file, changing the top level Content directory for the Assets directory. Texture assets which are
		re-used should be placed in the lowest possible directory in which it is accessed. For example, if both
		a file in the Content/Items/Vitric and Content/Items/Overgrow directory use a texture, it should be placed
		in the Assets/Items directory.

		In rare cases additional subdirectories may be created within the assets directory for large quantities of
		textures which fulfill a specific purpose, such as gore or textures with absurd quantites of variants

		Texture asset names should be descriptive of their application, or general if they experience heavy re-use.
		for example, the sprite for the Vitric Pickaxe should be VitricPickaxe.png, but a noise map which is used
		by many shaders would likely better fit with something like "PerlinNoise1".

		icon.png should exist in the top level directory as required by TML.

	1.1.4: sound files
		All sound files must exist in the top-level Sounds directory as required by TML. All music must exist in
		Sounds/Music as required by TML. Sound effects are sorted thematically within the Sounds folder.

	1.1.5: StructureHelper structure files
		StructureHelper structure files should be located in the Structures directory, with an appropriate name for
		the structure saved in it, if the file contains a multistructure it should be of plural form, else it should
		be of singular form. For example: ForestTemple VS ForestTemples

1.2: Class placement in source files

	1.2.1: When should multiple classes be placed in one source file
		Multiple classes should only be placed in the same source file if:
		A: The content they describe is tightly couples. IE, a weapon and it's specialized Projectile
		B: The length of the file does not exceed 500 lines
		C: The secondary classes could reasonably be found under the primary classes name
		This is to ensure it remains easy to find content for the sake of maintaining it while also keeping tightly
		bound content together so that it is intuitive to glean the full functionality of one piece of content from
		it's source file.

		When content must be seperated in order to maintain the readability of a file, ensure the class names and thus
		file names are named alike, IE: VitricSpear and VitricSpearProjectile to clearly communicate functionality is
		in a seperate file and where it is.

	1.2.2: Partial classes
		As described above, partial classes should be named alike, with a prefix describing the functionality of the
		specific methods defined in that partial section. In the rare case a partial class must be used outside of the 
		same directory as it's other parts, you should note in a comment where the rest of the class is. 

=========================================================================================================================================================

Section 2: Class/Interface/Struct structure

2.1: Ordering

	2.1.1: Class member order
		Class member order should follow the form:

		Constants
		Public variables
		Nonpublic variables
		Public properties
		Nonpublic properties
		Property overrides
		Arrow notation methods
		Nested enum definitions
		Constructors
		Load methods
		SetDefaults methods
		All other methods

2.2: Formatting

	2.2.1: General Formatting
		Each of the subcategories of class members listed above should have an empty line between them. Additionally every
		method should have an empty line between it and the next. This includes arrow notation methods. If multiple classes
		are present in one file they should be seperated by a single line break. Newline (Allman) bracketing style is used
		for all classes and methods.

	2.2.2: Region usage
		Regions may be used in larger files to keep methods thematically or functionally organized. Usage of regions is largely
		left to judgement, but generally should not be used in files <200 lines and should have some sort of reason for grouping
		members together.

2.3: Non-class structure

	2.3.1: Interfaces
		Interfaces should follow standard language naming convention ("I[X]able") and be structured with their methods in the
		desired implementation order. Interfaces should be implemented in the order they are defined.

	2.3.2: Structs
		Structs should follow the same structure as classes for all applicable member types.

2.4: Naming
	
	2.4.1: Type names
		Type names should be descriptive and easily identifiable if it is the primary type of a file. It should follow these conventions:

		Class name: UpperCamelCase
		Struct name: UpperCamelCase
		Interface name: see 2.3.1
		
	2.4.2: Member names
		Member names should be descriptive and follow these conventions:

		Constants: ALLCAPS
		Variables: lowerCamelCase
		Properties: UpperCamelCase
		Methods: UpperCamelCase
	 
=========================================================================================================================================================

Section 3: Method structure 

3.1: Control structures

	3.1.1: Conditionals
		If statements with a single line predicate should use next-line inlining. Example:

		if(ThisStyleGuideIsCool)
			ReadTheStyleGuide();

		If statements with multi-line predicates should use Allman bracketing. Example:

		if(ThisStyleGuideIsLame)
		{
			ReadTheStyleGuide();
			DeleteTheStyleGuide();
			styleGuide.coolness = 0;
		}

		Conditionals should have a linebreak between them, with the exception of else and else if statements. Example:

		if(This)
			That();

		if(This2)
		{
			That2();
			That3();
		}
		else if (This3)
		{
			That4();
			That5();
		}
		else
			That6();

	3.1.2: Loops
		Loops should always use Allman style bracketing. There should be a linebreak between loops. Example:

		for(int k = 0; k < 10; k++)
		{
			myArray[k] += 2;
		}

		for(int k = 0; k < 20; k++)
		{
			myArray2[k] += 2;
			myArray3[k] += 3;
		}

	3.1.3: Switch cases
		All switch cases should use Allman style bracketing, and have a linebreak between them.
		Switch cases with 1-line predicates should be placed all on the same line. Switch cases with multi-line
		predicates should have the case and break statements on seperate lines. If any predicates are
		multi-line, use the multi-line format. Example:
		
		switch(k)
		{
			case 1: DoThingOne(); break;
			case 2: DoThingTwo(); break;
			default: DoThingDefault(); break;
		}

		switch(n)
		{
			case 1:
				DoSomeThings();
				DoSomeOtherThings();
				break;
			case 2:
				DoSomeThings2();
				DoSomeOtherThings2();
				break;
			default:
				DoDefaultThing();
				break;
		}

3.2: Method calls and Locals

	3.2.1: Local variables
		Local variables should be named with specificity relevant to their usage. For example: if there is only
		one texture used throughout the entire method and its purpose is obvious and documented, "tex" would be
		an appropriate name. Otherwise, a more specific name is ideal. IE: use "texBody" and "texHead" over
		"tex1" and "tex2". Local variables that are to be used together or are tightly related should be defined
		consecutively, but if they are vastly different in use or identity place a linebreak between their definition.

	3.2.2: Method calls
		Method calls should attempt to be kept on the same line if possible, using local variables to pass complex
		expressions as parameters. If absolutely neccisary, split a method call so that the line ends with a comma
		and is indendted, or so that each line is a seperate parameter Example:

		spriteBatch.Draw(MyCoolTexture, MyCoolVector,
			MyCoolRectangle, MyCoolColor);

		or

		spriteBatch.Draw(
			MyCoolTexture,
			MyCoolVector,
			MyCoolRectangle,
			MyCoolColor);

		Similar to local variable definition, method calls which are tightly related to each other should be grouped
		together with no linebreak, but unrelated method calls should have a linebreak between them.

=========================================================================================================================================================

Section 4: Usage of StarlightPlayer, StarlightNPC, and similar classes

4.1: Accessory/Item/Similar behavior
	
	4.1.1: Accessories
		Changes to these classes directly should be avoided in all cases for accessory functionality. You should instead
		subscribe to the approriate event for the hook you want (names will mirror those of ModPlayer/GlobalNPC/etc.) from
		a load method within your Item class and define behavior there. All Accessories should descend from SmartAccessory,
		which has helper methods for you to check within the methods you subscribe to these hooks with for if the Item is
		equipped and getting the instance which is equipped. Variables required for the functionality should be stored in the
		Item class.

	4.2.2: Items
		Similar to accessories, Items which would require ModPlayer/GlobalNPC/etc. functionality should instead subscribe
		to the event hooks and check against the Player's held Item, with neccisary variables being stored on the Item instance.

	4.2.3: Buffs
		Similar to accessories, a SmartBuff class exists which has helper methods for checking if it is inflicted or not.
		Buff effects requiring ModPlayer/ModNPC/etc. hooks should again subscribe to the approrpiate events and check for
		the buffs infliction in the methods being subscribed with. Unfortunately as buffs are not instanced in the base game
		ModPlayer or ModNPC classes will be required to track variables on buffs. This may change in the future with newer
		solutions to this.

4.2: Save Data

	4.2.1: Saving
		Data which needs to be saved to the Player should be placed in ModPlayer classes, same to world data and ModSystem.
		Ideally flags are saved using BitFlag enums to reduce the size of save files and save/load times. 

	4.2.2: Loading
		All data which was saved should be loaded, any required resetting or similar effects should also be placed in the
		relevant load hook, but should be done seperately from loading save data.

4.3: Usage for Systems

	4.3.1: When to Use
		For fully seperate Player/world/NPC systems, having an independant ModPlayer/ModNPC/etc. class is reccomended.
		To identify if you should use this, ask yourself these questions:
			Is this functionality tightly bound to a single piece or small subset of content?
			Would this be applicable to many Players/NPCs/etc. at some time?
			Would implementing this in another way simply result in a class that only hooks a bunch of ModPlayer methods?
		if you're able to answer yes to most or all of these its likely a good idea to create a seperate class of the
		appropriate type to manage that system. An example of a system which exists in the Mod that fits this description would
		be the Barrier system.

	4.3.2: How to Implement
		When implementing a system like this, try to keep the interactions and functionality described in it as general as possible.
		for example, it would not be ideal to add a variable to this class to track something for a single accessory which interacts
		with the system. That would better be tracked in that accessorie's Item class. 

		Beyond this, implementation should follow standard TML implementations of these class types. Though do try to keep all
		behavior defined in these classes focused on the default functionality of the system overall, and not when it is effected
		by specific events or pieces of content.

=========================================================================================================================================================

Section 5: Drawing and Draw hooks

5.1: What to put in draw hooks
	
	5.1.1: Things to exclude
		In general, draw hooks should contain drawing and ONLY drawing. They may occasionally also contain draw-cycle bound timers,
		but it is important to note these do not sync with game timers. notable things that usually get included that should not are:
			Lighting calls
			Dust spawning

	5.1.2: Resetting the spriteBatch
		This should be avoided as much as possible as each reset impacts performance, but it is sometimes a necessary evil for creating
		visual effects. When you reset the spritebatch, you should place a linebreak between when you end it, no linebreak before beginning it,
		and a linebreak after beginning it again. Example:

		DrawThing();

		spriteBatch.End();
		spriteBatch.Begin();

		DrawThing2();

		spriteBatch.End();
		spriteBatch.Begin();

		DrawThing3();

	5.1.3: Shaders and Parameterization
		Shader effects should always be stored as a local variable. All shaders are autoloaded by the Mod, and can be accessed by their
		file name. For example, if you have a file named CoolDistortion.xnb, you can access it from Filters.Scene["CoolDistortion"].
		Parameter set method calls should all be grouped.

=========================================================================================================================================================
		
Section 6: Conclusion

	6.1: Closing thoughts
		Thanks for reading through this entire mess of a document. I'm hoping this will help ensure that the codebase remains consistent
		and maintainable into the future. Ofcourse not every single fringe case can be covered in something like this, but my hope is that
		the examples and precedents here, along with existing examples and productive discussion can ensure that those cases comply to
		a maintainable standard, and those standards can be appended here if the need arises.

	6.2: Ammendment policy
		If there is a case which arises in which one of these standards should be ammended or a new one should be appended, It should be done
		with a 3/4 vote AND final approval of a project lead. The secondary intent of this guide is to minimize conflict over style and
		standards, and as such a strict ammendment policy is needed as to not have minor conflicts be able to topple the entire standard.

=========================================================================================================================================================