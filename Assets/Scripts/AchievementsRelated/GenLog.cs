using System;
using System.IO;
using System.Text;
using UnityEngine;

public sealed class DesktopDiagnosticNote : MonoBehaviour
{
    private const string FileName = "Terminal_Diagnostic_Note.txt";

    // 这里就是你要落到桌面的“假报错报告”
    private const string ReportText =
@"================================================================================
 VIRTUAL DESKTOP // INCIDENT ARTIFACT
--------------------------------------------------------------------------------
 ReportId: VD-INC-SEQ-0xC3E1-7A90
 Timestamp: 2026-01-05T18:07:33.418-05:00
 Host: DESKTOP-SIM(virtual)
 User: player0
 Session: pid=4216, tid=1904, frame=188421
 Module: RedeemService.SequenceGate
 Build: 0.3.8-dev (commit 7f2c1b9, dirty)
 Severity: ERROR
 ErrorCode: SEQ-VALIDATE-FINGERPRINT-MISMATCH
 Storage: desktop://Artifacts/
================================================================================

[0] SUMMARY
    Redeem sequence submission failed STRICT validation.
    Privacy mode is enabled. No raw input or user-derived fingerprints were persisted.
    Artifact includes contract metadata and subsystem diagnostics captured at failure time.

[1] PROCESS / RUNTIME
    Runtime.Engine: Unity 2022.x (simulated)
    ScriptingBackend: Mono
    DomainReload: disabled
    TargetFrameRate: 120
    Time.Unscaled: true
    Locale: zh-CN
    Charset: UTF-8
    CurrentScene: DesktopScene
    ActiveWindowCount: 5
    FocusWindow: redeem_window

[2] MEMORY / GC SNAPSHOT
    Managed.UsedMB: 173.4
    Managed.ReservedMB: 512.0
    GC.Collections: gen0=3, gen1=1, gen2=0
    GC.LastCollectionAgo: 00:00:12
    ObjectCount.Est: 48213

[3] WINDOW MANAGER STATE
    WindowManager.Mode: desktop
    ZOrder.Top: redeem_window
    ZOrder.Stack: [redeem_window, achievements_window, notes_window, settings_window, explorer_window]
    DragState: idle
    LastOpenAction: DesktopIconButton -> OpenOrFocus(""redeem_window"")
    LastFocusReason: pointer_down

[4] UI / LAYOUT DIAGNOSTICS
    Canvas.ScaleFactor: 1.00
    Canvas.PixelPerfect: false
    EventSystem: enabled
    GraphicRaycaster: enabled
    Layout.RebuildsFrame: 2
    Layout.TotalTimeMs: 4.12
    TMP.AtlasRebuildsFrame: 0
    TMP.FallbacksFrame: 1

[5] REQUEST ENVELOPE
    RequestId: SEQREQ-0000000000002C07
    Route: /redeem/sequence/submit
    Context: desktop
    Window: redeem
    Flags: [strict, privacy, emit-artifact]
    Payload.SizeBytes: 59
    Payload.Hash: sha1: 1a0d9b7f3c1e4d2b9f2a0c1e0b9d8a7c6e5f4321
    CorrelationId: 9b0d3b12-5c2a-4e9a-8b3f-2d67a8a3d5ff
    RetryCount: 0

[6] INPUT POLICY
    RawInputRetention: disabled
    DerivedInputLogging: disabled
    Canonicalization: DIGITS_ONLY, separators stripped
    NormalizationRules:
      - trim whitespace
      - drop delimiters [' ', '-', '_', '.', ':', '/']
      - reject any non-digit codepoint
      - preserve order exactly
    FingerprintMethod: CRC32 (IEEE 802.3, reflected)

[7] TOKEN CONTRACT
    ContractKind: FIXED_LENGTH_DIGITS_ONLY
    AllowedAlphabet: ['0'..'9']
    MinLength: 7
    MaxLength: 7
    StrictMode: true

[8] VALIDATION RESULT
    Validator: SequenceValidatorV4
    Constraint: EXACT_FINGERPRINT_MATCH
    Result: FAIL
    FailureReason: FINGERPRINT_MISMATCH

    Notes:
      - submission satisfied format constraints
      - mismatch indicates wrong answer under current contract

[9] SECURITY / PRIVACY
    PrivacyMode: enabled
    RawInputLogging: disabled
    DerivedInputLogging: disabled
    ArtifactPolicy: expected-only (no user-derived material)
    DataClassification: internal-debug

[10] SECONDARY DIAGNOSTICS
    - INFO  UI: Layout pass 4.12ms (threshold 5.00ms)
    - WARN  Audio: buffer underrun (recoverable)
    - INFO  WindowManager: focus switch x2 within 110ms
    - WARN  FontAtlas: fallback glyph invoked for 1 codepoint
    - INFO  Net: offline mode (expected)
    - INFO  FS: artifact committed to desktop://Artifacts/

[11] MODULE INVENTORY
    Core:
      - WindowManager
      - UIWindow
      - UIDragHandle
      - DesktopIconButton
    Achievements:
      - AchievementsManager
      - AchievementsCatalog
      - AchievementPanel
      - AchievementCard
      - AchievementToastManager
      - AchievementHoverTooltip
    Redeem:
      - AchievementSequenceRedeemWindow
      - AchievementSequenceTable
      - SequenceFeedbackItem
    Diagnostics:
      - CrashReporter
      - DesktopFS

[12] CONFIG SNAPSHOT (SELECTED)
    SequenceGate.Strict= true
    SequenceGate.ContractKind= FIXED_LENGTH_DIGITS_ONLY
    SequenceGate.Length= 7
    SequenceGate.Canonicalization= DIGITS_ONLY_SEPARATORS_STRIPPED
    SequenceGate.FingerprintAlg= CRC32_IEEE_802_3_REFLECTED
    SequenceGate.ExpectedFingerprint= CRC32:0xFA8E8EA9
    CrashReporter.EmitArtifactOnFail= true
    CrashReporter.RetentionFiles= 3
    DesktopFS.ArtifactsPath= desktop://Artifacts/

[13] CRC PROFILE
    ProfileName: IEEE_802_3_REFLECTED
    poly: 0x04C11DB7
    init: 0xFFFFFFFF
    refin: true
    refout: true
    xorout: 0xFFFFFFFF
    encoding: ASCII

[14] REMARKS
    CRC32 is a checksum designed to detect transmission/storage errors, not to hide data.
    It maps arbitrary-length input to a 32-bit value, so different inputs can share the same CRC32.
    CRC32 is not intended to be inverted. Recovering an input from a CRC32 generally requires searching.
    Recovery is only feasible when the input space is constrained by a strict contract.
    Under FIXED_LENGTH_DIGITS_ONLY with length 7, the candidate space is bounded (10,000,000 values).

[15] STACK TRACE (VIRTUALIZED)
    at RedeemService.Submit(SequenceRequest req) in Assets/Scripts/RedeemService.cs:line 141
    at RedeemService.NormalizeAndValidate(String raw) in Assets/Scripts/RedeemService.cs:line 209
    at SequenceGate.Verify(String canonical) in Assets/Scripts/SequenceGate.cs:line 88
    at SequenceValidatorV4.ValidateExpected() in Assets/Scripts/SequenceValidatorV4.cs:line 52
    at CrashReporter.EmitArtifact(String path, String content) in Assets/Scripts/CrashReporter.cs:line 66
    at DesktopFS.CreateFile(String name, String content) in Assets/Scripts/DesktopFS.cs:line 142

[16] USER-FACING MESSAGE
    ""Invalid sequence. A diagnostic note has been synced to your system desktop.""

[17] FREE ACHIEVEMENT
    ""READTHELOG""

================================================================================
 END OF ARTIFACT
================================================================================
";

    // 你在“输入无效”时调用这个就行
    public void EnsureOnDesktop()
    {
        try
        {
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string path = Path.Combine(desktop, FileName);

            if (File.Exists(path))
                return;

            File.WriteAllText(path, ReportText, new UTF8Encoding(false));
        }
        catch (Exception e)
        {
            Debug.LogWarning("[DesktopDiagnosticNote] Failed to write txt to system desktop: " + e.Message);
        }
    }
}
