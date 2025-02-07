# runtime-interface-injection-test

Proof-of-concept interface injection for types at runtime.

What works?

- interfaces are injected into the MT at runtime,
- `(IInterface)(object)myObj` for an injected interface evaluates to `true`,
- `myObj is IInterface` for non-sealed types evaluates to `true`,
- injections are retroactively applied to already-instatiated types because they just modify the MT (shared between instances).

What doesn't work?

- JIT-optimizable expressions (e.g. `myObj is IInterface` for sealed types) evaluate to `false` :(,
  - but `(IInterface)myObj` still evaluates to `true`!
- VTable injection doesn't work yet
  - trying to figure out an ideal and sensible API
    - need to handle multiple methods (can't assume idx 0) and how to define them.

