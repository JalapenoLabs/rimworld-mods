/*************************************************************************
 * Willing Hands — the belonging tracker.
 *
 * Samples every colony slave on a slow cadence. Sustained comfort and mood
 * build a hidden contentment streak; a lapse erodes it. Once a slave has been
 * content long enough, each sample carries a small chance they ask to join.
 *
 * A MapComponent (rather than a per-pawn hediff) keeps the streak invisible,
 * preserving the pleasant surprise of the request, and auto-scopes the work to
 * spawned slaves.
 *
 * Creative Commons License Attribution-ShareAlike 4.0 International
 *************************************************************************/

using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace WillingHands;

/// Per-slave belonging state, persisted with the map.
public class BelongingRecord : IExposable {
    public int contentTicks;
    public int cooldownUntilTick;

    public void ExposeData() {
        Scribe_Values.Look(ref contentTicks, "contentTicks", 0);
        Scribe_Values.Look(ref cooldownUntilTick, "cooldownUntilTick", 0);
    }
}

public class MapComponent_SlaveBelonging : MapComponent {
    private Dictionary<Pawn, BelongingRecord> records = new Dictionary<Pawn, BelongingRecord>();

    private static readonly List<Pawn> stalePawns = new List<Pawn>();

    public MapComponent_SlaveBelonging(Map map) : base(map) { }

    public override void MapComponentTick() {
        if (Find.TickManager.TicksGame % WillingHandsTuning.CheckIntervalTicks != 0) return;
        SampleSlaves();
    }

    private void SampleSlaves() {
        int now = Find.TickManager.TicksGame;
        int interval = WillingHandsTuning.CheckIntervalTicks;
        List<Pawn> slaves = map.mapPawns.SlavesOfColonySpawned;

        DropStaleRecords(slaves);

        foreach (Pawn slave in slaves) {
            if (slave?.needs?.mood == null) continue;

            if (!records.TryGetValue(slave, out BelongingRecord record)) {
                record = records[slave] = new BelongingRecord();
            }

            if (IsContent(slave)) {
                record.contentTicks = Mathf.Min(record.contentTicks + interval,
                    WillingHandsTuning.RequiredContentTicks + interval);
            } else {
                int decay = Mathf.RoundToInt(interval * WillingHandsTuning.LapseDecayMultiplier);
                record.contentTicks = Mathf.Max(0, record.contentTicks - decay);
            }

            if (record.contentTicks < WillingHandsTuning.RequiredContentTicks) continue;
            if (now < record.cooldownUntilTick) continue;

            if (Rand.Chance(AskChanceThisSample(interval))) {
                SendJoinRequest(slave);
                record.cooldownUntilTick = now + WillingHandsTuning.AskCooldownTicks;
            }
        }
    }

    private static bool IsContent(Pawn slave) {
        float mood = slave.needs.mood.CurLevelPercentage;
        Need_Comfort comfort = slave.needs.TryGetNeed<Need_Comfort>();
        float comfortLevel = comfort?.CurLevel ?? 0f;
        return mood >= WillingHandsTuning.MoodThreshold
            && comfortLevel >= WillingHandsTuning.ComfortThreshold;
    }

    /// Converts the per-day ask chance into the equivalent per-sample chance, so
    /// the sampling cadence never changes how often slaves actually ask.
    private static float AskChanceThisSample(int interval) {
        float samplesPerDay = 60000f / interval;
        return 1f - Mathf.Pow(1f - WillingHandsTuning.AskChancePerDay, 1f / samplesPerDay);
    }

    private void SendJoinRequest(Pawn slave) {
        TaggedString label = "WillingHands.JoinRequest.Label".Translate(slave.Named("PAWN"));
        TaggedString text = "WillingHands.JoinRequest.Text".Translate(slave.Named("PAWN"));

        ChoiceLetter_SlaveJoinRequest letter = (ChoiceLetter_SlaveJoinRequest)LetterMaker.MakeLetter(
            label, text, WillingHandsDefOf.WillingHands_JoinRequest, slave);
        letter.slave = slave;
        Find.LetterStack.ReceiveLetter(letter);
    }

    /// Called from the letter when the player refuses: a long pause, and some
    /// lost ground, before the slave might ask again.
    public void NotifyDeclined(Pawn slave) {
        if (records.TryGetValue(slave, out BelongingRecord record)) {
            record.cooldownUntilTick = Find.TickManager.TicksGame + WillingHandsTuning.DeclineCooldownTicks;
            record.contentTicks = Mathf.Max(0, record.contentTicks - WillingHandsTuning.RequiredContentTicks / 3);
        }
    }

    private void DropStaleRecords(List<Pawn> currentSlaves) {
        stalePawns.Clear();
        foreach (KeyValuePair<Pawn, BelongingRecord> entry in records) {
            if (entry.Key == null || entry.Key.Destroyed || !currentSlaves.Contains(entry.Key)) {
                stalePawns.Add(entry.Key);
            }
        }
        foreach (Pawn stale in stalePawns) {
            records.Remove(stale);
        }
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Collections.Look(ref records, "records", LookMode.Reference, LookMode.Deep);
        if (Scribe.mode == LoadSaveMode.PostLoadInit && records == null) {
            records = new Dictionary<Pawn, BelongingRecord>();
        }
    }
}
