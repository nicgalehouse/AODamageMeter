AO Damage Meter
---------------

Real-time graphical damage meter for the sci-fi MMORPG Anarchy Online.

Latest release [here](https://github.com/nicgalehouse/AODamageMeter/releases/tag/v1.1.1) (built for W10, but I think it works on W7 too).

Video demonstration [here](https://youtu.be/K4iU7KronOg).

Forum post [here](https://forums.anarchy-online.com/showthread.php?616901-Try-out-the-new-AO-Damage-Meter-(like-those-used-in-WoW)).

Features
--------

+ Automatically configures combat log windows/files. If you have one already it'll find it, otherwise it'll create one for you.
+ Pulls in player bios from http://people.anarchy-online.com/ on the fly so you get nice tooltips, icons, and color coding.
+ Aggregates pets with masters using a naming convention (a pet named "Reimagine's robo" is by convention a pet of the player "Reimagine"), but exposes individualized pet/master detail rows when the master's row is expanded.
+ The interface looks pretty similar to AO's.
+ And the basics like pausing fights, viewing historical fights, scripts, and detailed stats for pretty much everything.
+ Update 1.1: Supports dynamically adding pets to yourself or others.
+ Update 1.1: Includes special handling to make the damage done by/taken from crat charms more accurate.

Instructions
------------
Left-clicking on a row navigates down a level into its sub-rows and right-clicking anywhere navigates in reverse.
Left-clicking on row icons is used to expand detail rows, like the individualized pet/master rows I mentioned above, or a preview of the rows that would be revealed by left-clicking on the row itself (in certain contexts).

Hover over the left text or the right text of any row to reveal relevant informational tooltips.
All data is exposable through a single script, /aodm.
To populate the script with data, ctrl+left-click on any left tooltip, the middle part of a row (if it has sub-rows), or any right tooltip.
The corresponding data is copied to your clipboard and into the /aodm script.
Just type "/aodm" in-game or create a macro like "/macro aodm /aodm" to share the data.

Ctrl+right-click on a row when in the Damage Done view to add that character as your pet.
Ctrl+shift+right-click a row, then ctrl+shift+right-click a second row to add the first character as a pet of the second character.
Ctrl+right-click on any pet detail row to unassign the pet.
