# Third-party notices

Lucid's own code is MIT (`LICENSE`); its original text, briefs, specs and
generated shell textures are CC-BY-4.0. Everything below belongs to someone else
and keeps its own licence.

Per-cube art assets are **not** listed here. Each cube carries its own ledger at
`Lucid/Assets/_Lucid/Packs/<Pack>/Cubes/<Cube>/assets/LICENSES.md`, checked by
`tools/check-licenses.py`. Only CC0 and CC-BY assets are committed; anything else
is fetched from `assets.manifest.json` and never redistributed here.

## Unity packages

Used under the [Unity Companion License](https://unity.com/legal/licenses/unity-companion-license)
unless the package states otherwise. Versions are pinned in
`Lucid/Packages/manifest.json`.

| Package | Version |
|---|---|
| com.unity.render-pipelines.universal | 17.3.0 |
| com.unity.netcode.gameobjects | 2.13.1 |
| com.unity.addressables | 2.11.2 |
| com.unity.inputsystem | 1.19.0 |
| com.unity.ai.navigation | 2.0.11 |
| com.unity.test-framework | 1.6.0 |
| com.unity.nuget.newtonsoft-json | 3.2.2 (transitive) |

## Development tooling

| Component | Licence | Notes |
|---|---|---|
| [MCP for Unity](https://github.com/CoplayDev/unity-mcp) (`com.coplaydev.unity-mcp`), pinned to `v10.1.2` | See the upstream repository | Editor-side bridge, development only. Not required to build, test or play Lucid — `tools/run-tests.sh` and `tools/build-cube.sh` drive Unity in batch mode without it. See `docs/DECISIONS.md`. |

## Planned

Facepunch.Steamworks and a Steam transport for Netcode for GameObjects arrive in
M1 (`docs/WORKPLAN.md` §5). They will be listed here with their licences when
they land.
