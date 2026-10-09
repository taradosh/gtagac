# changelog

## 1.1.0

movement, network and evidence work, plus a test suite.

**upgrade guide:** see `migration.md`. four breaking changes, three of which are bugs that previously made the library weaker than it appeared.

every existing public entry point keeps its meaning unless listed under breaking changes. `gac.upd` behaves exactly as in 1.0.1.

### movement detection

- new check `gacnc`, detects reported positions that cross solid geometry. needs authoritative world colliders, so it stays silent on a client unless `gacp.ncloc` is set
- `gacpos` rewritten, compares reported position against reported velocity instead of a spike detector
- `gacfly` now requires sustained stationarity, `gacgrav` requires sustained deviation
- `gaccon` skips while grounded, climbing or swimming, and ignores samples with unstable intervals
- `gacjmp` only inspects upward velocity
- `gacvel` gained a downward allowance derived from real air time
- `gacrat` gap limit accounts for the measurement interval
- `swg()` also treats fast horizontal motion as swinging

### network handling

- new `gacsq`, sequence numbers, client timestamp validation, clock offset learning, jitter and loss measurement
- `gac.upd2(id, p, v, grounded, seq, clientTime)`, returns whether the packet was accepted. the original `gac.upd` is unchanged
- large forward sequence gaps are now accepted and counted as loss instead of being rejected. rejecting an honest player after a stall bought no security
- replay, stale and future rejection sit behind `gacp.strict`, which defaults to off
- ping is per player, `gac.ping(id, rtt)`, capped by `gacp.pingmax`. one slow player does not widen everyone else's limits. **new in 1.1.0, nothing to migrate**
- every positional tolerance now scales with measured jitter, loss and ping through `d.slack`
- new `gac.dropped(id)`, `gac.outoforder(id)`

the measured rtt lives in `gacd.pin`, outside the config hash, because it is a measurement and not a setting. `gacp.pin` remains a hashed static floor for a global allowance.

### evidence gated punishments

- new opt in gate, `gacp.evon`. defaults to **off**, existing behaviour is unchanged
- kick and ban can require several **different** checks to agree, `gacp.evmin`. repeated flags of one check never count as independent evidence
- `gacp.evcf` confidence floor, `gacp.evwin` evidence window
- warnings are never gated, they are informational
- a held back punishment is logged at level `gate` and applied once the evidence arrives, so the gate delays a kick but cannot cancel one

### prediction

- new optional `gacpred`, off by default, for rendering remote players
- the server model snaps to the reported position, the visual offset is capped per update and overall, and bleeds to zero
- a stall longer than `gacp.maxid` resets the model instead of snapping
- prediction never feeds the checks, so latency cannot produce a flag
- **`gacpred` is not a defence against a client that teleports.** the caps bound how far a rendered player can be dragged, they do not validate anything. a server that accepts the client's position without an authoritative check still lets the client choose where it goes. `Tests/anticheat.cs` pins this distinction

### logging and diagnostics

- new ring buffered structured log with per player rate limiting, `gaclog`
- warnings, kicks, bans and held back punishments are never rate limited away
- new `gacdiag`, full text report, csv export and file save
- all formatted numbers use the invariant culture, a comma decimal separator no longer corrupts the csv
- new alloc free `igace` sink interface alongside the existing `Action<gace>` events

### the speed limit

`gacp.maxspeed` default raised from 12 to 18 m over s. verified in `Tests/rope.cs`:

- the fastest **energy consistent** pendulum, `L·amp·ω ≤ √(2gL(1−cos amp))`, over ropes of 6, 8, 10, 12 and 16 m with amplitudes up to 1.4 rad, peaks below 18 m over s. `the_default_covers_the_fastest_physical_swing` computes this bound rather than trusting a hand picked case
- every rope length is silent, plus a wide arc, a pumped swing under the burst ceiling, a swinging grapple, hanging still, a rope drop, several rope uses in one session, and a swing over a 20 percent lossy link
- 60 m over s is still caught, still kicks, still bans, and the `speed` check fires even when the client reports a velocity matching the position
- 27 m over s, just above the burst ceiling of 24.3, is caught
- movement below the burst ceiling is **not** caught, and that is the documented tolerance rather than a detection gap

a hand picked "fast swing" can easily be made physically impossible. a 10 m rope with a 2.2 s period peaks at 27 m over s, which would require climbing 38 m on a 10 m rope. the suite derives periods from the energy bound so a false positive cannot be blamed on the library.

### server

- `gacsrv.up(id, p, v, grounded, clientTime)` now returns whether the packet was accepted and honours the timestamp
- new overload `gacsrv.up(..., sequence)` for transports that carry their own sequence numbers
- new `gacsrv.seq(bool)` to enable strict rejection, and `gacsrv.rtt(id, rtt)` for per player ping
- `gacsrv.join` registers into `gac.ply` exactly once, an unannounced id is admitted on its first packet, and a kicked or banned id is refused on every path
- `gacsrv.net.cnt` and the slot cap no longer disagree, the two lists used to drift apart

### breaking changes

three items, verified against the 1.0.1 sources: no public member was removed or renamed, no new dependency appears.

**1. `gacf.add` no longer discards the first flag of a check. detection is now more sensitive.**

a new check entry was created with `n = 0`, so the first flag of every check was never counted and every threshold was reached one flag late.

**detection thresholds are now reached earlier, roughly one flag sooner per check.** this is the intended effect of the fix, not a separate tuning change, and it is pinned by `Tests/thresh.cs`. the same file verifies that the shift introduces no false positive across the legitimate scenarios, a bad wifi link and a frozen stream, and that a sustained speedhack still reaches kick and ban.

raising `fth`, `kth` or `bth` by one restores the previous cadence.

**2. `gacp.hash` no longer always returns zero.**

the FNV step was computed in `float`, which discarded all fractional precision, so the function returned `0.0f` for every configuration. config tampering detection via `gacenv` was therefore dead. rewritten with `uint` arithmetic. games that hardcoded an expected hash value, if any, need to stop.

**3. `gacsrv.up` signature and behaviour.**

in 1.0.1 it was `static void up(string, Vector3, Vector3, bool, float)`. it is now `static bool up(string, Vector3, Vector3, bool, double)` plus an overload taking an explicit sequence number. a `void` call site still compiles. the client timestamp, previously accepted and discarded, is now used, but only when `gacp.strict` is on.

the `float` to `double` change on the timestamp is deliberate: `gac.upd2` takes a `double` client time, and a server that widened its own timestamp to `double` and then passed it here would lose precision.

not breaking, but worth reading:

- `gacp.maxspeed` default is 18 instead of 12
- a new check is registered by default, `gacnc`. it stays silent unless `gacp.srv` is set or `gacp.ncloc` is enabled, so client side behaviour is unchanged
- `gacp.pin` is still hashed, and now has a companion `gacp.pingmax`

### performance

- zero bytes allocated per tick, verified by test across a clean stream, a cheater before the kick, a player after the kick, the full local path, prediction, and a lossy link
- flag reasons are formatted only after `gacr.canflg` accepts the flag, so a rejected flag costs no string. this matters most for `p.sent`, which is sticky
- debug strings are built only inside `if (gacp.dbgo())`
- **breaking fix**: `gacr.get` evicted from the dictionary but not from the lookup list, so the list grew without bound and eviction was wrong
- per packet network logging is behind `gacp.logn`, off by default because it scales with packet loss rather than with cheating

### tests

- 211 tests, nunit is a dev dependency only and never enters the runtime assembly
- `Tests/` runs both in the unity test runner and headless through `dev/tests.csproj`, using a minimal `UnityEngine` shim
- false positive suites assert zero flags across legitimate locomotion, ropes and swings, bad wifi and a frozen stream
- `perf.cs` pins the zero allocation guarantee with `GC.GetAllocatedBytesForCurrentThread`
- the legitimate scenario set lives in one place, `tst.legit()`

## 1.0.1

hardening

- new check `gaccon`, acceleration derived from reported positions, catches position writes with a clean velocity
- `gacp.hash` rewritten as an FNV style rolling hash over every config field, previously a fixed polynomial that could be reproduced by hand
- confidence aggregation changed from a mean to a saturating sum, so several simultaneous violations compound instead of averaging out

## 1.0.0

initial release

- core manager `gac`, config `gacp`, flags `gacf`, violations `gacr`
- checks, speed, teleport, jump, fly, gravity, velocity, arm, position, rate, env
- server authoritative mode through `igacnet` and `gac.upd`
- ban storage abstraction `igacbanstore`, memory store `gacbm`, file store `gacban`
- events `onflag`, `onwarning`, `onkick`, `onban`
- confidence aggregation, flag decay, per check cooldown
- whitelist for development builds, admins, trusted players
- debug overlay `gacgui` and debug logger `gacdw`
- unity package layout with `runtime` and `samples~`