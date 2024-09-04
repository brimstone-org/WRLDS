
ENUMS AS FLAGS BUTTONS
=============================

An editor script by Martin Nerurkar 

- Visit my site at www.playful.systems
- Visit my company at www.sharkbombs.com
- Asset Store Page: https://www.assetstore.unity3d.com/en/#!/content/74356


Description
-----------------------------
This simple script allows you to use the [EnumFlags] attribute on your 
enums to visualize them in the editor. With this plugin they simply 
appear as convenient rows of toggleable buttons, that allow you to 
quickly see and switch the various flags. 

The script offers simple customization options and contains a basic 
example that explains its usage. 

Please note that this is mostly Donationware. A simpler version of 
this script is available for free on our blog: 

http://www.sharkbombs.com/2015/02/17/unity-editor-enum-flags-as-toggle-buttons


Usage
-----------------------------
All you need to do is create a new enum with the [System.Flags]
attribute. Then when you add fields of that enum type in that are
visible in the inspector, give them the [EnumFlags] attribute and
they will be properly displayed in the editor.

Note that the numeric values of your enum need to be configured
properly to work: 

- No enum name has the value 0
- The first name starts with a value of 1
- Every other enum doubles the previous value (2, 4, 8, 16, 32)
- You can skip powers of two and the script will still function
- Do not add non-power-of-two values, this will create weird button behavior
- Do not add negative values, this will also break a lot of things

To set the values from code you can use bitwise operators to do so.

- &   Binary AND Operator copies a bit if it exists in both operands.
- |   Binary OR Operator copies a bit if it exists in either operand
- ^   Binary XOR Operator copies the bit if it is set in one operand but not both.
- ~   Binary Ones Complement Operator is unary and has the effect of 'flipping' bits.
- <<  Binary Left Shift Operator. The left value is moved left by the number of bits specified by the right operand.
- >>  Binary Right Shift Operator. The left value is moved right by the number of bits specified by the right operand.

To check for existing flags you can either use bitwise operators

- (proficency & WeaponType.Sword != 0) to see if the flag is set.

Or you can use the extension method supplied with the assets

- proficency.HasFlag(WeaponType.Sword)


Further information on enum flags can be found here:
- https://www.dotnetperls.com/enum-flags


Example
-----------------------------

[System.Flags]
public enum WeaponType { 
	Sword = 1, 
	Axe = 2, 
	Hammer = 4, 
	Spear = 8 
}

[EnumFlags]
public WeaponType proficency;



Contact & Support
-----------------------------
If you have any questions about the asset, please don't hesitate to
reach out to me:

	E-Mail: 	support@playful.systems
	Twitter: 	@mnerurkar

And if you find time, I'd love a review on the asset store.


