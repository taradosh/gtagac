# 1.1.0

movement detection, network awareness, optional evidence gate, and a test suite.

fixes three things that made the library weaker than it looked, and adds the network handling that a server authoritative design was missing.

---

## fixes

**the config hash was always `0.0f`.** the FNV step was computed in `float`, and a `float` holds integers exactly only up to 16,777,216, while the intermediate product reaches 3.6e16. the fractional part was discarded on the very first mix, so `gacp.hash()` returned the same value for every configuration. the `gacenv` check that compares this hash every tick could therefore never detect config tampering. rewritten with `uint` arithmetic.

**the first flag of every check was thrown away.** `gacf.add` created a new entry with `n = 0`, so a check had to fire twice before it counted at all, and every threshold was reached one flag late. fixed. **this makes detection more sensitive, see breaking changes.**

**latency measurement was impossible to do safely.** writing an rtt into a hashed config field makes the game look like it is tampering with itself. the measured rtt now lives on the player, outside the hash.

**`gac.upd` accepted an argument it did not use.** `gacsrv.up(id, p, v, grounded, clientTime)` threw the timestamp away. it is now validated, behind an explicit opt in.

**the per player table leaked.** `gacr.get` evicted from its dictionary but not from its lookup list, so the list grew without bound and eviction picked the wrong entry.

**diagnostics wrote locale dependent numbers.** a comma decimal separator corrupted the csv export. everything is now invariant culture.

---

## breaking changes

three. no public member was removed or renamed, no new dependency appears, and a `void` call site still compiles.

**1. detection is more sensitive.** thresholds are now reached roughly one flag sooner per check, as a direct consequence of the `gacf.add` fix.

if you had false punishment problems and want the old cadence back:

```csharp
gacp.fth = 11f;   // was 10
gacp.kth = 26f;   // was 25
gacp.bth = 46f;   // was 45
```

this is verified not to introduce a false positive across legitimate locomotion, ropes and swings, a bad wifi link and a frozen stream, and to still catch a sustained speedhack through kick and ban.

**2. `gacp.hash()` no longer returns zero.** if you compared it against a hardcoded value, remove the comparison.

**3. `gacsrv.up` returns `bool` and uses the timestamp.**

```csharp
// 1.0.1
public static void up(string, Vector3, Vector3, bool, float)
// 1.1.0
public static bool up(string, Vector3, Vector3, bool, double)
public static bool up(string, Vector3, Vector3, bool, double, int sequence)
```

the `float` to `double` change on the timestamp matches `gac.upd2`. a `float` still compiles, it is widened.

not breaking, but worth reading:

- `gacp.maxspeed` default is 18 instead of 12. the old value flagged real swings, see below
- a new check is registered by default, `gacnc`. it stays silent unless `gacp.srv` is true or you set `gacp.ncloc`, so client side behaviour is unchanged

---

## what was added

### a new check for wall crossing

`gacnc` casts between two consecutive reported positions and flags a crossing of solid geometry. it discards hits very close to the starting point, extends slightly past the end so fast movement is not missed, and ignores back faces, which is the signature of a ray leaving geometry rather than hitting it.

**it requires authoritative world colliders.** on a dedicated server that is normal. on a client the colliders belong to the cheater, so the check stays silent unless you set `gacp.ncloc`, and even then client side evidence is weak.

### sequence numbers, timestamps, learned latency

```csharp
bool ok = gac.upd2(id, position, velocity, isGrounded, sequence, clientTime);
gac.ping(id, rtt);
gacp.strict = true;
```

`upd2` rejects replayed, stale and out of order packets, learns the client's clock offset from the first packet, and measures jitter and loss. every positional tolerance then scales with those measurements, so players on bad wifi stop producing false positives without loosening the limits for everyone else.

a large **forward** sequence gap is still accepted and counted as loss. rejecting an honest player after a stall buys no security, and this is the one case where a hostile packet is deliberately not rejected.

ping is per player, so one slow player does not widen everyone else's limits.

### optional evidence gate

```csharp
gacp.evon = true;
gacp.evmin = 2;
```

off by default. when on, a kick or a ban waits for several **different** checks to agree. repeated flags of one check never count as independent evidence.

warnings are never gated, they are informational and blocking them would only hide a detection from you. a held back punishment is logged at level `gate` and applied as soon as the evidence arrives, so the gate can delay a kick but never cancel one.

a consistent speedhack trips both `speed` and `velocity`, so the common exploit is punished normally. a genuinely single check exploit is flagged and warned but not kicked.

### optional prediction for remote players

```csharp
gacp.pr = true;
transform.position = gacpred.pos(id);
```

off by default. the server model always snaps to the reported position, the visual offset is capped per update and overall, and bleeds to zero, so a correction is spread over several frames instead of snapping.

a stall longer than `gacp.maxid` resets the model rather than snapping the player to it.

### structured logging and diagnostics

```csharp
string report = gacdiag.txt();
string csv = gacdiag.csv();
gacdiag.save("diag.txt");
```

a ring buffered structured log, rate limited per player. warnings, kicks, bans and held back punishments are never rate limited away, and one flooder cannot starve the rest. per packet network logging is behind `gacp.logn`, off by default because it scales with packet loss rather than with cheating.

an alloc free `igace` sink interface was added next to the existing events, for when `Action<gace>` boxing is not acceptable.

### zero allocation per tick

not a claim, a test. `GC.GetAllocatedBytesForCurrentThread` across 2000 updates requires exactly zero bytes for a clean stream, a cheater before the kick, a player after the kick, the full local path, prediction, and a lossy link.

flag reasons are now formatted only after the flag is accepted, so a rejected flag costs no string at all. that matters most for a player who has already been punished, where the early return is sticky for the rest of the session.

---

## the speed limit

`gacp.maxspeed` default raised from 12 to 18 m over s.

a pendulum on a rope of length `L` swung through amplitude `amp` can only reach `√(2gL(1−cos amp))`, which works out to 16 m over s on the longest plausible VR rope. the old limit of 12, multiplied by the burst tolerance, was inside that envelope, so real swings got flagged.

the tests derive swing periods from the energy bound rather than picking them by hand, so a false positive cannot be blamed on the library. a hand picked "fast swing" is trivially impossible to believe: a 10 m rope with a 2.2 s period peaks at 27 m over s, which would require climbing 38 m on a 10 m rope.

verified: every rope length from 6 to 16 m is silent, plus a wide arc, a pumped swing, a swinging grapple, hanging still, a rope drop, several ropes in one session, and a swing over a 20 percent lossy link. and 60 m over s is still caught, still kicks, still bans.

---

## limitations

be clear about what this library is and is not.

**`gacpred` is not a defence against a client that teleports.** the caps bound how far a rendered remote player can be dragged from what the server last received. they do not validate anything. if your server accepts the client's position without an authoritative check, the client can still choose where it goes and prediction will smoothly render the jump.

for a server that actually resists a teleporting client you need all three of:

1. server side validation running, `gacp.srv = true` plus `gac.upd2` or `gacsrv.up`
2. authoritative world colliders on the server, otherwise `gacnc` cannot see a wall crossing
3. your own authority decision, for example a swept shape cast that resolves the player against the server world and sends back a corrected position

without those, treat the server checks as telemetry, exactly like the client side ones. `gtagac` is a physics plausibility layer, not an authoritative simulation.

**other honest limits:**

- client side checks can be bypassed by an attacker who patches the assembly. they are telemetry, not enforcement
- the client controls its own timestamp, so with `strict` off a client can lie about time. `strict` plus sequence numbers is the mitigation, and it costs you transport compatibility
- rotations and angular velocity are not checked
- `gacnc` needs the moving platform layers removed from `ncmask`, or a grace window at anchors. a platform sweeping into a player looks a lot like noclip

---

## upgrading

full guide in `migration.md`. the short version: replace `runtime/`, run `cd dev && dotnet test`, and decide whether to enable the three new opt in features.

the code compiles as is.

---

## tests

211, nunit as a dev dependency only, never in the runtime assembly. they run in the unity test runner and headless.

two suites exist on purpose. one asserts **zero** flags across legitimate locomotion, bad wifi and a frozen stream. the other asserts that real exploits are still caught. the suite fails both if a check becomes timid and if it becomes noisy.

`perf.cs` pins the zero allocation guarantee. `thresh.cs` pins the sensitivity change. `rope.cs` pins the speed limit.