# What it does
``modkit_conflicts.ps1`` can look through a FiveM server's resource folders and tell you of Modkit ID conflicts.

It creates two files wherever it is run: ``modkits.txt`` and ``conflicts.txt``

## How to run it
Download the actual ``modkit_conflicts.ps1 script`` from Releases.


You'll probably need to run it on a local copy of your server.
Move the ``modkit_conflicts.ps1`` file to the ``resources`` folder, right-click on it, "Run with PowerShell". The Powershell will open and warn you about running unsafe scripts. Z to accept and run it, N to not run it.

It will show you its progress and, when finished, you can go through the two files to learn what modkit IDs are conflicting across all your vehicles.
