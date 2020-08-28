# ExplorerCleanup

Experimental Space Engineers mod that explores broadcasting player grid violations to create exploration game loops in multiplayer.

Unstable POC, not for production.

Introduces several interesting mechanics including:

- Salavage and exploration with risk / reward trade off
- PVP encounters when engineers contest a location
- Only broadcasts interesting grids
- Notifies owner with grace period and checks for player distance from grid
- Possibility for player pirates to spoof signals in various forms
- Trash collection

Todo:

- Proper logging (not in chat)
- Proximity to owned antenna increases GPS grid details (e.g. name, type, power output, grid size)
- Make it play nice with default trash remover and conceal
- User and Admin chat commands
- Delay GPS based upon distance
- Change settings to realistic values (currently 30 seconds for testing)
