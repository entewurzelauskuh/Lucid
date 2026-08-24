# LUCID — M0 Playtest Plan

**Version 1.0 — 2026-08-24 (consolidated).** The note template is `docs/playtests/TEMPLATE.md`. Runs as work plan task M0.10, after M0.9 (round flow) and M0.9c (tuning console and log report).

M0 has corridors only: Straight, Corner, T, Cross and the bedroom. No traps, no mobs, no shooting. So this playtest cannot balance the game and does not try to. It answers one question and settles three knobs.

**The question:** with nothing but corridors, fog doors and the deepest-exit rule, is the race between one Nightmare and one Sleeper already a little fun and completely readable? If not, chicanes will decorate a loop that does not work.

**The knobs:** exit hysteresis (0 or 2), starting budget and trickle, head start.

---

## 1. Hypotheses

Each has a measure and a threshold. The thresholds are deliberately modest; M0 is a loop, not a game.

| # | Hypothesis | Measure | Pass |
|---|---|---|---|
| H1 | The door language is readable without explanation | Sleeper rating "I knew where the exit was" (1–5), and count of "lost" moments (> 10 s in explored cubes with no plan) | mean ≥ 3.5; lost moments ≤ 1 per round by round 5 |
| H2 | Exit moves feel like play, not like cheating | Sleeper rating "the exit moving felt fair" (1–5) | mean ≥ 3 with hysteresis 0, or ≥ 3.5 with hysteresis 2 |
| H3 | Exploring is understood as a weapon | Nightmare rating "hardened doors changed my plans" (1–5); branch kills per round (explored events that solidified ≥ 1 door) | mean ≥ 3; ≥ 2 branch kills per round by round 5 |
| H4 | The Nightmare can build fast enough to matter | Click-to-cube time (median seconds from selecting a card to a confirmed placement); rejections per round by reason | median ≤ 2.0 s; rejections ≤ 3 per round and none rated "didn't understand why" |
| H5 | Corridors alone lose slowly, not instantly | Time to wake with default knobs | median between 45 s and 120 s |
| H6 | The yo-yo exists and has a counter | Yo-yo events per round (an exit move that lengthens the Sleeper's shortest path to any exit by ≥ 4 cubes); whether the Sleeper kills the abandoned branch within 20 s | recorded, no threshold; feeds the hysteresis decision |
| H7 | The wire behaves | Desync notices; placement round trip (ms); wake/place races | 0 desyncs; round trip ≤ 150 ms on LAN; every race resolves as the spec says |
| H8 | It is a little fun | Both players' fun rating (1–5); "would play another round" after rounds 5 and 10 | mean ≥ 3.5; yes from both at round 10 |

## 2. What needs to exist first (M0.9c)

- **Tuning console** (`F8`): head start, round length, starting budget, trickle interval, exit hysteresis, Sleeper speed multiplier, each changeable by the host between rounds and stamped into the `.lucidlog` header. Nothing here ships to players; it is a dev panel.
- **Log report** (`tools/playtest-report.py <file.lucidlog>`): prints the per-round metrics of §5 as a table and appends a row to `docs/playtests/m0-rounds.csv`.
- **Round timer overlay** for the observer: elapsed, cubes placed, exit depth, Sleeper depth.
- A build that a friend can run: `tools/build-dev.sh` producing a zip with the UTP transport; for remote play before Steam exists, a mesh VPN such as Tailscale or ZeroTier makes the host's port reachable with no router work.

## 3. Setup

- **People:** two players minimum; an observer is a luxury. Both players play both roles, five rounds each.
- **Place:** same room if possible, so the observer (or the idle player) can see both screens. Remote works with Discord and screen share; note it, since latency and the lack of peripheral observation change what you see.
- **Machines:** the host is the Nightmare in the first block (placement latency zero, worst case for the Sleeper) and the Sleeper in the second (worst case for the Nightmare). Record which.
- **Recording:** every round writes its `.lucidlog`. A phone on a stand filming the Nightmare's screen is cheap insurance for H4; skip it if it slows things down.
- **Before the session:** the owner runs three rounds in Sandbox and one against a virtual player in Multiplayer Play Mode, so the session is not the first time the build is played.

## 4. Protocol

Ten scored rounds in three blocks, plus a warm-up. Sixty seconds of questions after every round; a five-minute break between blocks.

| Block | Rounds | Knobs | Roles |
|---|---|---|---|
| Warm-up | 1 | defaults | owner as Nightmare |
| A — defaults | 1–4 | head start 30 s, budget 12, trickle 1 / 4 s, hysteresis 0, dawn 5:00 | swap after round 2 |
| B — hysteresis | 5–7 | as A, hysteresis 2 | swap after round 6 |
| C — tuned | 8–10 | budget and trickle chosen from A and B by the rules in §7 | swap after round 9 |

Standing instructions to players, read aloud before round 1 and never repeated:

- Sleeper: "Grey mist is closed. White light is the way out. The dream can change while you run."
- Nightmare: "You have thirty seconds alone. Cubes cost points. Keep the white door away from them."

Nothing else. If a player asks a rules question during a round, answer after the round and write the question down; every question is an H1 or H3 data point.

## 5. Metrics from the log

`tools/playtest-report.py` computes these per round from `.lucidlog` alone:

- Outcome and time (woke at / consumed at / dawn).
- Cubes placed; placements per minute; budget unspent at the end.
- Median click-to-cube time (from `PlaceRequest` timestamps in the log; the request is logged even when rejected).
- Rejections by reason.
- Exit moves: count; the Sleeper's path distance to the nearest exit before and after each; yo-yo events (Δ ≥ 4 cubes); time from each yo-yo to the branch being killed, if ever.
- Branch kills (explorations that solidified ≥ 1 door) and doors hardened.
- Sleeper backtracking: seconds spent in already-explored cubes.
- Deepest exit depth reached; Sleeper's maximum depth.
- Desync notices; placement round-trip median; wake/place races and their resolution.

The observer adds by hand: "lost" moments (Sleeper), "didn't understand the rejection" moments (Nightmare), and any question asked.

## 6. Questionnaire

Asked aloud right after each round, answered with a number 1–5 and at most one sentence. The idle player writes it down.

Sleeper: knew where the exit was · the exit moving felt fair · I had a plan most of the time · fun.

Nightmare: I could build as fast as I wanted · hardened doors changed my plans · rejections made sense · fun.

After rounds 5 and 10, both: "another round?" (yes / no) and "what would you change?" (one sentence).

## 7. Decision rules

Written up in `docs/DECISIONS.md` the same day, with the numbers.

- **Hysteresis.** Adopt 2 if block A shows ≥ 1 yo-yo per round on average *and* the Sleepers' fairness rating in A is < 3.5 *and* block B improves it by ≥ 0.5. Otherwise keep 0. If yo-yos never happened at all, note that corridors did not provoke them and re-test in M1.13 with chicanes.
- **Click-to-cube.** If the median is above 2.0 s, that is a UI bug, not a tuning question; fix the palette before deciding anything else and re-run block A.
- **Trickle and starting budget.** Corridors should lose the race slowly. Measure the Sleeper's mean corridor traversal time `t_c` (about 1.5 s expected) and the Nightmare's median click-to-cube `t_p`. Set the trickle interval near `2.5 × t_c` (default 4 s stands if `t_c` ≈ 1.5 s) and the starting budget to what a Nightmare actually places in the head start plus a margin of 2 (default 12 stands if 8–10 cubes get placed). Confirm in block C that H5 holds.
- **Head start.** Keep 30 s unless fewer than 6 cubes get placed in it by round 4; then 45 s, and note that the palette is the likelier culprit.
- **Round length.** Not decided in M0; record the times. Chicanes are what stretch a round toward 5 minutes.
- **Go / no-go for M1.** Go if H1, H4, H7 and H8 pass and none of H2, H3 fails by more than 0.5. No-go means a week of thinking, not a week of building: the likely pivots are the door visuals (H1), the palette (H4), the exit rule itself (H2), and Sleeper speed (H5).

## 8. Session note template

Save as `docs/playtests/YYYY-MM-DD-m0.md`.

```markdown
# M0 playtest — YYYY-MM-DD

Build: <commit>   Transport: utp (LAN | Tailscale)   Room: same | remote
Players: A (owner), B   Observer: none | C

## Rounds
| # | Block | Nightmare | Outcome | Time | Cubes | Exit moves | Yo-yos | Branch kills | Rejections | Desyncs |
|---|---|---|---|---|---|---|---|---|---|---|
| 1 | A | A | woke | 1:12 | 14 | 3 | 0 | 2 | 1 (solid) | 0 |
…

## Ratings (mean per block)
| Block | Exit clear | Exit fair | Plan | Build speed | Doors changed plans | Rejections clear | Fun S | Fun N |
|---|---|---|---|---|---|---|---|---|

## Questions asked during rounds
- 

## Lost / confused moments
- 

## What each player would change
- A:
- B:

## Decisions (copied to DECISIONS.md)
- Hysteresis:
- Trickle / starting budget:
- Head start:
- Go / no-go:
```

## 9. What this playtest is not

It does not test balance, difficulty, trap fairness, combat, possession, four Sleepers, Steam, or the look. Those have their own sessions at M1.13, M2 and M3. Anything observed about them goes in the note under "later" and nowhere else.
