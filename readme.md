# gtag ac

**gtag ac** — lightweight, server authoritative anti cheat for unity vr games with physics based gorilla locomotion, arm climbing, grappling, momentum swings.

plain c sharp, zero dependencies, no obfuscation, no reflection in runtime checks, no linq, no per frame allocations, no playerprefs for bans.

```
namespace gtagac
```

---

## features

| check | class | flags | what it does |
|---|---|---|---|
| speed | `gacspd` | 1 | sustained horizontal speed above limit, measured over a sliding window, burst tolerance for hand swings |
| teleport | `gactp` | 3 | impossible displacement between samples, physics burst aware, needs confirmation |
| jump | `gacjmp` | 1 | impossible vertical impulse, separated from normal momentum |
| fly | `gacfly` | 1 | long air time with no physical cause, no rise, no fall, no travel |
| gravity | `gacgrav` | 1 | missing gravity, velocity deviates from the ballistic model, never uses raw position y |
| velocity | `gacvel` | 2 | horizontal velocity, vertical velocity and velocity deltas validated separately |
| arm | `gacarm` | 1 | vr hand speed and hand teleport, only active when hand transforms are provided |
| position | `gacpos` | 2 | history buffer analysis, spikes against the local mean, no single frame verdict |
| rate | `gacrat` | 2 | movement update flooding and packet spam protection |
| env | `gacenv` | 4 | time scale tampering, suspicious delta time, local movement parameter tampering |

- flag system with per check weights, decay and confidence aggregation
- punishments, warning, kick, temporary ban, permanent ban, each independently switchable
- whitelist, development builds, admins, trusted players
- events, `onflag`, `onwarning`, `onkick`, `onban`
- custom checks through `gaccheck` and `gacc`, one class plus one `gac.reg()` line
- networking abstraction through `igacnet`, core is framework agnostic
- ban storage abstraction `igacbanstore`, in memory store for tests, file store for the server
- debug overlay and debug logging, silent in release builds

---

## installation

unity package manager, add package from git url:

```
https://github.com/youraccount/gtagac.git?path=/
```

or copy the folder `runtime` into your project, for example `assets/plugins/gtagac`.

no packages, no manifest changes, no post process steps.

---

## setup

```csharp
using gtagac;

public class boot : MonoBehaviour
{
    void Start()
    {
        gacp.maxspeed = 12f;
        gacp.maxvel = 20f;
        gacp.tpd = 3f;
        gacp.fth = 10;

        gac.init(transform);
    }

    void Update()  { gac.tick(); }
    void FixedUpdate() { gac.phys(); }
}
```

full setup with vr hands, debug overlay and event handlers:

```csharp
gacs.onflag    = e => Debug.Log("flag    " + e.id + " " + e.chk + " " + e.rsn + " " + e.cf + " " + e.cfl);
gacs.onwarning = e => SendWarning(e.id, e.rsn);
gacs.onkick    = e => KickPlayer(e.id, e.rsn);
gacs.onban     = e => BanPlayer(e.id, e.rsn);

gac.init(playerRoot, leftHand, rightHand, playerBody);

gameObject.AddComponent<gacgui>();
gameObject.AddComponent<gacdw>();
```

`gac.tick()` runs in `Update` and steps time, `gac.phys()` runs in `FixedUpdate` and does the ground probe. checks themselves run on the fixed interval `gacp.ivl`, twenty hertz by default.

zero code option, add the `gacm` component to the player object and fill `root`, `hl`, `hr`, `body`.

---

## configuration

everything lives in `gacp`.

| field | default | meaning |
|---|---|---|
| `maxspeed` | 12 | max horizontal player speed, m over s |
| `maxvel` | 20 | max velocity magnitude, m over s |
| `tpd` | 3 | max teleport distance per check interval, m |
| `maxjmp` | 9 | max jump impulse, m over s |
| `maxair` | 8 | max air time before the fly check reacts, s |
| `maxhand` | 14 | max hand speed, m over s |
| `maxupd` | 60 | max movement updates per second |
| `fth` | 10 | flag threshold, warning |
| `kth` | 25 | kick threshold |
| `bth` | 45 | ban threshold |
| `bcf` | 0.9 | ban confidence threshold |
| `maxwarn` | 3 | warnings before silence |
| `decay` | 0.6 | flag decay, points per second |
| `ivl` | 0.05 | check interval, s |
| `burst` | 1.35 | burst tolerance multiplier |
| `smp` | 3 | samples above the limit required in a window |
| `tps` | 2 | teleport confirmations required |
| `grav` | 9.81 | gravity used by the ballistic model |
| `maxacc` | 200 | max velocity change per second |
| `gtol` | 2 | allowed gravity deviation, m over s |
| `gmin` | 0.2 | grace before the gravity check |
| `hov` | 0.5 | vertical speed considered hover |
| `swgsp` | 2 | hand speed considered swinging |
| `grdchk` | true | ground probe in `FixedUpdate` |
| `grdoff` `grdlen` `grdmask` | 0.35, 1.2, all | ground probe parameters |
| `cs` `ct` `cj` `cf` `cg` `cv` `ca` `cp` `cr` `ce` | true | per check enable |
| `pun` `pwn` `pkk` `pkb` `pbm` | true | punishment switches |
| `bper` | false | ban permanent instead of temporary |
| `dbg` `dbgf` `dbgi` | false, false, 0.25 | debug output, force debug in release, debug interval |
| `devby` | true | bypass in development builds |
| `wl` `wlst` | empty, true | whitelist ids |
| `loc` | true | run client side checks |
| `allowsc` | false | allow time scale changes |
| `btmp` | 1440 | temporary ban minutes |

```csharp
gacp.sw("speed");
gac.sync();
```

or directly:

```csharp
gacp.maxspeed = 14f;
gac.chk(0).on = false;
```

---

## events

```csharp
public struct gace
{
    public string id;
    public string chk;
    public string rsn;
    public float cf;
    public float cfl;
    public float tm;
}
```

```csharp
gacs.onflag = e => { };
gacs.onwarning = e => { };
gacs.onban = e => { };
gacs.onkick = e => { };
```

---

## custom checks

```csharp
using gtagac;

public sealed class gacnoclip : gacc
{
    public gacnoclip()
    {
        nm = "noclip";
        weight = 3f;
        cooldown = 0.7f;
    }

    public override void tick(gacd d, float dt)
    {
        if (d.h.n < 2) return;

        float dy = d.h.now.y - d.h.at(1).y;

        if (dy < gacp.tpd * -0.5f) return;

        flg(d, 0.8f, "noclip");
    }
}

gac.init(transform);
gac.reg(new gacnoclip());
gacr.wgt.set("noclip", 3f);
```

`gacc` gives `nm`, `on`, `weight`, `cooldown`, `flg()`. `gacr.wgt.set(name, weight)` overrides the weight at runtime. this is the whole extension mechanism, core is never edited.

---

## server integration

server authoritative, the client never decides that its position is valid.

client sends its state:

```csharp
gac.upd(playerId, position, velocity, isGrounded);
```

server receives and validates:

```csharp
gacsrv.init();

gacsrv.join(id);
gacsrv.up(id, position, velocity, isGrounded, clientTime);
gacsrv.leave(id);
```

`gac.upd` validates position, velocity, movement delta, timestamps and player state on the receiving side, then `gacr` picks the punishment. kicked and banned players are rejected through `gac.st(id)`.

server ban storage:

```csharp
gacban ban = new gacban("serverdata/gtagacbans.txt");
ban.load();
gac.store = ban;
```

permanent bans live on the server side only, never in `playerprefs`. `gacbm` is the in memory store for tests.

client side fallback when no server is available:

```csharp
gac.init(playerRoot, leftHand, rightHand, playerBody);
```

client checks are weaker by nature, treat them as telemetry only.

### network adapters

core references no networking framework. implement `igacnet`:

```csharp
public interface igacnet
{
    string id { get; }
    bool srv { get; }
    Vector3 pos { get; }
    Vector3 vel { get; }
    float t { get; }
    bool grnd { get; }
    void kick(string r);
    void ban(string r, float m);
}
```

photon, fusion, normcore, netcode for gameobjects and mirror all fit behind this one interface. see `samples~/gacnetex.cs`.

---

## false positive prevention

- every check needs several consecutive samples, never a single frame verdict
- burst tolerance `gacp.burst` for gorilla locomotion swings and rope physics
- `gacgrav` uses the ballistic model plus velocity, never raw `position.y`
- `gacfly` ignores rising, falling and travelling states
- gravity, speed, velocity and position checks suspend themselves while grabbing, climbing or swinging
- `gacarm` is active only when hand transforms are provided
- grace window `gac.grace(id, 0.5f)` for respawn, portals, ropes, vehicles, cutscenes
- per check cooldown stops flag spam
- `gacp.devby` bypasses everything in development builds
- whitelist for admins and trusted players

report game state and the checks adapt to it:

```csharp
gac.me.st(character.IsGrounded, character.JumpPressed, character.Climbing, character.InWater, character.Grabbing);
```

---

## performance

- checks run on the fixed interval `gacp.ivl`, twenty hertz by default, not per frame
- `Update` only steps time, `FixedUpdate` only does the ground probe
- ring buffers with pre allocated arrays, zero heap allocation in the check path
- no linq, no reflection, no exceptions inside checks
- `Vector3` math only, no allocations for distance or damping
- one ground `Physics.CheckSphere` per interval, not per frame
- events and debug output are opt in

---

## debug

```csharp
gacp.dbg = true;
gacp.dbgf = Debug.isDebugBuild;
gameObject.AddComponent<gacgui>();
gameObject.AddComponent<gacdw>();
```

overlay and logs show speed, velocity, flags, confidence and active checks. `gacp.dbgf` forces debug output inside a release build, `gacp.dbg` alone stays silent outside development builds. press `gacp.dbgkey`, `F1` by default.

---

## repository layout

```
gtagac/
  readme.md
  license
  changelog.md
  package.json
  runtime/
    gtagac.asmdef
    core/        gac.cs gacp.cs gacf.cs gacper.cs gacr.cs gacev.cs gacm.cs gacgui.cs gacdw.cs
    checks/      gaccheck.cs gacspd.cs gactp.cs gacjmp.cs gacfly.cs
                 gacgrav.cs gacvel.cs gacarm.cs gacpos.cs gacrat.cs gacenv.cs
    data/        gacd.cs gacv.cs
    util/        gaclog.cs gactime.cs gacmath.cs
    net/         igacnet.cs
    ban/         gacbm.cs gacban.cs
    server/      gacsrv.cs
  samples~/
    gacboot.cs gacnetex.cs gtagac.smp.asmdef
```

---

## license

mit, see `license`.

## roadmap

- physics consistency check, predicted position versus reported position
- rotation and angular velocity check
- anim and rig integrity signals from the networking layer
- per check statistical thresholds learned from the server baseline
- matchmaking trust score
- official photon and fusion adapters