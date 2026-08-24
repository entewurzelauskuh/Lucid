# LUCID — Decisions

ADR-lite. One entry per decision taken during implementation that promotes a **[D]** to **[S]**, deviates from `SPEC.md`, adds a package, or settles an open question. Newest first. Edit the spec in the same pull request.

Format:

```
## YYYY-MM-DD — short title
Context: what forced the decision (issue #, playtest, spike).
Decision: what was decided.
Reason: why this over the alternatives.
Spec: sections changed.
```

## 2026-08-24 — MCP for Unity bridge added, pinned to v10.1.2

Context: issue #29. Driving the editor only through `-batchmode` makes small round trips (reading the console, running one test) cost a full editor launch. Spec §17 already allows "a Unity MCP bridge if editor round trips get tedious".
Decision: `com.coplaydev.unity-mcp` from `https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity`, pinned to the **v10.1.2** tag.
Reason: pinned to a tag rather than the project's `#beta` default branch, so the dependency cannot move under us — the same reasoning spec §14 applies to the community Facepunch transport. A dev-only tool does ship in the public manifest as a result; the alternative, an embedded copy under `Packages/`, would vendor several thousand lines of third-party source into the repository, which is worse. Batch mode stays the contract: `tools/run-tests.sh` and `tools/build-cube.sh` must keep working without the bridge, so contributors are never required to install it.
Spec: none — §17 already anticipates the bridge.

## 2026-08-24 — Netcode for GameObjects and Addressables pinned

Context: M0.1. The Unity 6 URP project was generated from the 3D cross-platform template, whose manifest carries neither package, but spec §14 requires both.
Decision: `com.unity.netcode.gameobjects` at **2.13.1** and `com.unity.addressables` at **2.11.2** in `Lucid/Packages/manifest.json`.
Reason: both declare `unity=6000.0` and so are compatible with the pinned 6000.3.11f1. NGO 2.13.1 is the registry's `latest` and satisfies §14's "2.x in host mode". For Addressables the registry's `latest` dist-tag is 2.11.2 even though 3.0–4.0.1 are published; taking the tagged release is what Package Manager offers by default and avoids adopting the newer build pipeline before anything depends on it. Revisit if a pack ever needs 4.x.
Spec: none — §14 already names both packages; this only pins versions.
