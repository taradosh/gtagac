# migrating 1.0.1 to 1.1.0

the upgrade is mechanical for almost everyone. the code compiles as is, no public member was removed or renamed, and no new dependency appears. what changes is how *sensitive* the library is, and that two mechanisms you may have relied on were weaker than they looked.

three breaking changes, all verified against the 1.0.1 sources:

| # | change | affects you if |
|---|---|---|
| 1 | the first flag of a check is no longer discarded, thresholds arrive one flag sooner | you want the old timing back |
| 2 | `gacp.hash()` no longer always returns `0.0f` | you compared it against a value |
| 3 | `gacsrv.up` returns `bool` and honours the client timestamp | you read the return value |

do this in order: swap the files, run the tests, then decide on the three optional changes at the end.

---

## 1. swap and build

```
runtime/    replace
Tests/      new, optional
dev/        new, optional
```

nothing is renamed, nothing moved, no new dependency. `gac.init`, `gac.tick`, `gac.phys`, `gac.upd` and every `gacp` field keep their meaning.

verified compiling clean against unity 2021.3.21f1, 6000.3.6f1 and 6000.5.0f1, zero errors and zero warnings.

---

## 2. the one thing that changes behaviour out of the box

**detection now happens roughly one flag sooner per check.**

in 1.0.1 `gacf.add` created a new check entry with `n = 0`. the first flag of every check was silently discarded, so every threshold was reached one flag late. that was a bug and it is fixed.

### do i need to do anything?

only if your players were getting punished and you want the old cadence back:

```csharp
gacp.fth = 11f;    // was 10
gacp.kth = 26f;    // was 25
gacp.bth = 46f;    // was 45
```

one point restores the previous timing exactly.

if you have **not** had false punishment problems, do nothing. the more sensitive setting is the intended one, and `Tests/thresh.cs` verifies it does not introduce a false positive across legitimate locomotion, ropes and swings, a bad wifi link and a frozen stream.

### other defaults that changed

| field | 1.0.1 | 1.1.0 | note |
|---|---|---|---|
| `maxspeed` | 12 | 18 | the old value flagged real swings, see `readme.md` |

a new check is registered by default, `gacnc`, the wall crossing detector. it stays silent unless `gacp.srv` is true or you set `gacp.ncloc`, so client side behaviour is unchanged. on a server, read the section on colliders below.

---

## 3. new: latency awareness, nothing to migrate

`gac.ping` **did not exist in 1.0.1**, so there is nothing to migrate. it is new in 1.1.0 together with automatic tolerance widening.

```csharp
gac.ping(id, rtt);
```

tell the server the measured round trip and the tolerances widen for that player, per player. one slow player no longer widens everyone else's limits.

if you want a static allowance for everyone, set the config field, which is hashed like every other limit:

```csharp
gacp.pin = 0.05f;    // global floor, part of the config hash
gacp.pingmax = 2f;   // cap on a measured rtt, 2 seconds by default
gac.ping(id, rtt);   // per player measurement, not hashed
```

the split matters: the rtt lives in `gacd.pin`, outside the hash, because it is a measurement and not a setting. writing a measured value into a hashed field is what made an earlier draft of this release report itself as config tampering.

---

## 4. if you use `gacsrv`

the signature changed, in a way that still compiles:

```csharp
// before, 1.0.1
gacsrv.up(id, position, velocity, isGrounded, clientTime);   // void, timestamp discarded

// after, 1.1.0
bool ok = gacsrv.up(id, position, velocity, isGrounded, clientTime);
```

the client timestamp was previously accepted and thrown away. it is now used, but only when strict mode is on. turn it on explicitly, because the server is not the client's time source:

```csharp
gacsrv.seq(true);
```

without that, replay and out of order packets are still accepted, exactly as in 1.0.1.

the timestamp parameter changed from `float` to `double`, matching `gac.upd2`. passing a `float` still compiles, it is widened. if your server keeps client time in a `double`, nothing is lost now.

if your transport already has sequence numbers, pass them instead of relying on the built in counter:

```csharp
gacsrv.up(id, position, velocity, isGrounded, clientTime, clientSequence);
```

other server changes worth knowing:

- `gacsrv.join(id)` is now optional. an unannounced id is admitted on its first packet, which is what a real server sees while a handshake is in flight
- a kicked or banned id is refused at every entry point, including `join` and `up`
- `gacsrv.rtt(id, rtt)` feeds the per player ping
- `gacsrv.net.cnt` and the slot cap now agree, they used to drift apart

---

## 5. if you had a hardcoded config hash

in 1.0.1 `gacp.hash()` **always returned `0.0f`**. the FNV step was computed in `float`, which threw away the fractional part of the product.

```csharp
if (gacp.hash() == 0f) { ... }   // 1.0.1, always true
```

the `gacenv` config tampering check was effectively dead, because the value never changed. it is now a real `uint` based hash.

if you wrote anything against that value, remove it. if you relied on `gacenv` catching config changes, it works now for the first time.

---

## 6. optional: enable the new features

all off by default, none of them change existing behaviour.

### evidence gate

require several **different** checks to agree before a kick or a ban:

```csharp
gacp.evon = true;
gacp.evmin = 2;      // two distinct checks
gacp.evcf = 0.35;    // confidence floor
gacp.evwin = 30f;    // evidence window, seconds
```

what to expect:

- warnings still fire. they are informational and gating them would only hide a detection from you
- a held back punishment is logged at level `gate`, so you can see it was considered
- it is never cancelled. the moment the second check fires, the kick happens
- a consistent speedhack trips both `speed` and `velocity`, so it passes the gate and is punished normally
- a genuinely single check exploit, for example only wall crossing, is flagged and warned but not kicked

### sequence numbers and timestamps

`gac.upd2` is new, the old `gac.upd` still works and is unchanged:

```csharp
gac.upd(id, position, velocity, isGrounded);                       // 1.0.1 behaviour, verbatim
bool ok = gac.upd2(id, position, velocity, isGrounded, seq, t);    // new
```

replay, stale timestamps and out of order packets are rejected when `gacp.strict` is on:

```csharp
gacp.strict = true;
```

a **forward** gap is still accepted and counted as loss, because muting an honest player after a stall buys no security. that is the one case where a malicious packet is deliberately not rejected, and it is a deliberate trade.

tolerances widen automatically with measured ping, jitter and loss, so you should not need to loosen limits for players on bad wifi.

### prediction, for rendering remote players

```csharp
gacp.pr = true;

transform.position = gacpred.pos(id);
```

read this before enabling it: **`gacpred` is not a defence against a client that teleports.** the caps bound how far a rendered player can be dragged from what the server last received. they do not validate anything. if your server accepts the client's position without an authoritative check, the client can still choose where it goes and `gacpred` will smoothly render the jump.

for `gacpred` to mean anything you need all three:

1. server side validation actually running, `gacp.srv = true` and `gac.upd2` or `gacsrv.up`
2. authoritative world colliders on the server, otherwise `gacnc` cannot see a wall crossing
3. your own authority decision, for example a swept shape cast that resolves the player against the server world and sends back a corrected position. `gacpred` renders that decision, it does not make it

if you have none of these, treat the server checks as telemetry only, same as the client ones.

### structured log

```csharp
string report = gacdiag.txt();
string csv = gacdiag.csv();
gacdiag.save("diag.txt");
```

rate limited per player, and warnings, kicks, bans and held back punishments are never dropped. `gacp.logn` additionally logs per packet network events, off by default because it scales with packet loss rather than with cheating.

---

## 7. verifying the upgrade

the test suite runs headless and in unity:

```
cd dev
dotnet test
```

211 tests. the ones that matter for you:

| suite | what it proves |
|---|---|
| `thresh` | the new sensitivity adds no false positive, and real exploits still get punished |
| `rope` | every rope length and swing style stays silent at the new limit, and speedhacks still get caught |
| `legit`, `netfp`, `noclipfp` | zero flags across legitimate locomotion, bad wifi and a frozen stream |
| `anticheat` | `gacpred` does not hide a reported teleport from the checks |
| `perf` | still zero bytes allocated per tick |

if you changed any threshold, run them before shipping.

---

## 8. if something is wrong

| symptom | likely cause |
|---|---|
| kicks you did not expect | the one flag earlier threshold. raise `kth` by one |
| `env` flags mentioning `cfg` | something is writing to a hashed field after init. `gacenv` compares `gacp.hash()` every tick, so any runtime write to a `gacp.*` field is tampering by definition. use `gac.ping(id, rtt)` for latency, it writes to `gacd.pin` |
| noclip false positives on a server | moving platforms, vehicles or doors in `ncmask`, or use `gac.grace(id, 0.5f)` at anchors and respawns |
| ropes or swings flag on a server | confirm the server sees real rope physics rather than a networked approximation, and that the player is reported as `grabbing` or `climbing` through `gac.me.st(...)` |
| prediction looks wrong | it renders, it does not validate. see section 6 |