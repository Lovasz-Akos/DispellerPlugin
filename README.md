<p align="center">
  <img src="images/icon.png" alt="Dispeller" width="128">
</p>

<h1 align="center">Dispeller</h1>

<p align="center">A Dalamud plugin that finds items in your Glamour Dresser that share the same model, so you can clear out the duplicates, aswell as the items that can be moved to the Armoire! Happy space-saving 🤍 </p>

## Install

\o/ Officially included in Dalamud! Just search for `Dispeller` and there it is!

Alternatively,

Add this URL in Dalamud under **Settings → Experimental → Custom Plugin Repositories**:

```
https://raw.githubusercontent.com/Lovasz-Akos/DispellerPlugin/master/repo.json
```

Then install **Dispeller** from the plugin installer.

## Use

`/dispeller` opens the window, `/dispeller config` opens the settings.

Open your Glamour Dresser at least once, then hit **Scan**. Items are grouped by
equipment slot, with matching models next to each other, and anything storable in
the Armoire is flagged.

Two items count as duplicates when their mesh matches — a recolour of a garment
is still a redundant glamour. Turn off **Count recolors as duplicates** in the
settings to require the colour variant to match too.

## Disclaimer

Third-party plugins are against the FINAL FANTASY XIV Terms of Service, and
using them may get your account banned. Use at your own risk.

## Credits

Fork of [pupwife/DispellerPlugin](https://github.com/pupwife/DispellerPlugin).
Color variant setting inspired by [Dryness's fork](https://github.com/Dryness/DispellerPlugin).
Built on [Dalamud](https://github.com/goatcorp/Dalamud) and
[FFXIVClientStructs](https://github.com/aers/FFXIVClientStructs).
