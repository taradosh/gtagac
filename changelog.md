# changelog

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