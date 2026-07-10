# NetworkRefactorV2 Plan

## Current priority

1. Match the original `SGF.Network` behavior first.
2. Keep the implementation isolated under `NetworkRefactorV2`.
3. Treat multi-socket support as a capability of the new runtime, not as an integration step yet.
4. Do not modify `AppMain`, Lua, UI, or the old network layer until V2 behavior is proven.

## Original network behavior now covered

- 6-byte packet header: 3-byte little-endian payload-plus-command length, 1-byte serialize type, 2-byte little-endian command.
- Legacy encryption key and rotate/xor algorithm.
- RPC envelope `[14][cmd:2][len:2][payload]`.
- Verify packet is sent immediately after TCP connect when a verify packet factory is provided.
- Business sends are rejected before verify succeeds.
- Verify failure maps to `Kicked` and returns login UI intent.
- Receive failure before verify maps to `Kicked`, not reconnect.
- Verify timeout is separate from TCP connect timeout.
- Heartbeat packets are sent by injected factory and interval gate.
- Weak ping thresholds match the original values, including total weak limit `1000` -> return login.
- Reconnect cancels the old receive loop, applies attempt limit, and delays by default `0.5s`.
- RPC subscriptions support entity-specific dispatch and group dispatch.
- Immediate command hook supports original-style receive-thread handling for heartbeat/time sync without referencing old concrete handlers.
- PacketReader uses a byte buffer with cursors instead of `List<byte>.RemoveRange`.

## Verification status

- V2 EditMode test assembly compile: passed with `EDITMODE_COMPILE_EXIT=0`.
- Standalone self-check: passed with `NetworkRefactorV2 self-check: 24 passed, 0 failed`.
- Unity Editor batch command exits with `UNITY_EXIT=0`, but does not currently write the requested `-testResults` XML or a test summary in the log. Treat this as a Unity CLI result-output gap, not as proof that EditMode tests ran.
- Encoding: V2 `.cs` and `.asmdef` files are UTF-8 without BOM and CRLF.
- Boundary check: V2 code does not directly reference `SGF.Network`, `SocketItem`, `SocketUtils`, `MsgEncode`, `NetworkManager`, `GameManager`, or UI classes.

## Unity test blocker

Unity Editor batch test was previously blocked because another Unity instance had the same project open:

```text
Multiple Unity instances cannot open the same project.
Project: H:/Star/Stars_Project/StarsProject_Client/trunk/Stars
```

After closing the open Unity Editor instance, the batch command starts and exits with code `0`, but no XML result file is produced. Re-run with this command when investigating Unity Test Runner output:

```powershell
& 'H:/DevelopFold/Unity Editor/2022.3.42f1c1/Editor/Unity.exe' -batchmode -nographics -quit -projectPath 'H:/Star/Stars_Project/StarsProject_Client/trunk/Stars' -runTests -testPlatform EditMode -assemblyNames SGF.NetworkRefactorV2.Tests -logFile 'H:/Star/Stars_Project/StarsProject_Client/trunk/Stars/Temp/NetworkRefactorV2-EditMode.log'
```

## Remaining before old-layer integration

1. Investigate why Unity CLI exits `0` without producing `-testResults` XML or a test summary, then rerun real EditMode tests.
2. Add a direct old-layer comparison harness only if it can be compiled without pulling old game/UI dependencies into V2 tests.
3. Define the adapter boundary for constructing the real `ClientVerifyReq` and heartbeat payload from current game session data.
4. Define command IDs for immediate heartbeat/time-sync handling at the integration boundary.
5. Only after the above, plan multi-socket registration for `game` and any future `battle` socket.
