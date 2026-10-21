/*************************************************************************
 * What Work Type Is This?, a RimWorld mod by JalapenoLabs.
 *
 * Adds a "Work type" row to every building's info card, naming the work type
 * (and so the Work tab column and skill) that colonists use to operate it.
 *
 * Nothing is hand-mapped. Every job in the game is a WorkGiverDef that already
 * declares which things it works on, either as a workbench list
 * (fixedBillGiverDefs) or as the ThingRequest its scanner searches the map
 * for. Reading those declarations makes modded buildings and modded jobs show
 * up with no compatibility patches.
 *
 * Creative Commons License Attribution-ShareAlike 4.0 International
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using RimWorld;
using Verse;

namespace WhatWorkType;

[StaticConstructorOnStartup]
public static class WhatWorkTypeStartup {
    static WhatWorkTypeStartup() {
        new Harmony("jalapenolabs.rimworld.whatworktype").PatchAll();
    }
}

/// Appends the "Work type" row to building info cards. ThingDef.SpecialDisplayStats feeds both the def card
/// (architect menu) and the placed-building card (the "i" button), so one patch covers both.
[HarmonyPatch(typeof(ThingDef), nameof(ThingDef.SpecialDisplayStats))]
public static class ThingDef_SpecialDisplayStats_Patch {
    /// Sits above "Paintable" (6000), the highest vanilla entry in the Building section.
    const int DisplayPriority = 6500;

    public static IEnumerable<StatDrawEntry> Postfix(IEnumerable<StatDrawEntry> entries, ThingDef __instance) {
        foreach (StatDrawEntry entry in entries) {
            yield return entry;
        }

        // Pawn, item and plant cards would list every pawn-targeting job (tend, rescue, feed...), which is noise.
        if (__instance.category != ThingCategory.Building) {
            yield break;
        }

        // Most buildings (walls, doors, furniture) have no job of their own; showing nothing is the correct answer.
        List<WorkGiverDef> jobs = WorkGiverMatcher.JobsOperating(__instance);
        if (jobs.Count == 0) {
            yield break;
        }

        yield return new StatDrawEntry(
            StatCategoryDefOf.Building,
            "WhatWorkType.Label".Translate(),
            DescribeWorkTypes(jobs),
            DescribeJobs(jobs),
            DisplayPriority
        );
    }

    /// Distinct work types, in the Work tab's left-to-right column order.
    static string DescribeWorkTypes(List<WorkGiverDef> jobs) {
        return jobs
            .Select((job) => job.workType)
            .Distinct()
            .OrderByDescending((workType) => workType.naturalPriority)
            .Select((workType) => workType.labelShort.CapitalizeFirst())
            .ToCommaList();
    }

    /// One line per job with its skill and its rank among the jobs in its work type, since that rank decides
    /// which job a colonist picks first when several share the same Work tab priority.
    static string DescribeJobs(List<WorkGiverDef> jobs) {
        StringBuilder report = new StringBuilder("WhatWorkType.Desc".Translate());
        report.AppendLine();

        foreach (WorkGiverDef job in jobs) {
            WorkTypeDef workType = job.workType;
            string skills = workType.relevantSkills
                .Select((skill) => skill.LabelCap.ToString())
                .ToCommaList();
            if (skills.NullOrEmpty()) {
                skills = "WhatWorkType.NoSkill".Translate();
            }

            int rank = workType.workGiversByPriority.IndexOf(job) + 1;
            report.AppendLine();
            report.Append("WhatWorkType.Job".Translate(
                job.LabelCap,
                workType.labelShort.CapitalizeFirst(),
                skills,
                rank,
                workType.workGiversByPriority.Count
            ));
        }

        return report.ToString();
    }
}

/// Decides which WorkGiverDefs operate a building, using only what each job declares about its targets.
public static class WorkGiverMatcher {
    /// Groups that sweep in nearly every building, so membership says nothing about what the building is for.
    /// BuildingArtificial is how Repair and Fix Broken Down find targets; PotentialBillGiver is the fallback for
    /// bill jobs, which are matched precisely through fixedBillGiverDefs instead.
    static readonly HashSet<ThingRequestGroup> genericGroups = new HashSet<ThingRequestGroup> {
        ThingRequestGroup.Everything,
        ThingRequestGroup.BuildingArtificial,
        ThingRequestGroup.PotentialBillGiver,
        ThingRequestGroup.HaulableEver,
        ThingRequestGroup.HaulableAlways,
        ThingRequestGroup.HaulableEverOrMinifiable,
    };

    /// Jobs whose request depends on the current game state, mapped to the request they make while active.
    /// Research asks for nothing until a project is selected, which would hide research benches.
    static readonly Dictionary<Type, ThingRequest> requestByStatefulJob = new Dictionary<Type, ThingRequest> {
        [typeof(WorkGiver_Researcher)] = ThingRequest.ForGroup(ThingRequestGroup.ResearchBench),
    };

    /// Jobs whose worker threw while being asked for its request. Remembered so the warning is logged once,
    /// not every time an info card opens.
    static readonly HashSet<WorkGiverDef> brokenJobs = new HashSet<WorkGiverDef>();

    /// Computed per call instead of cached: a few hundred cheap checks per info card open, and no cache means
    /// no stale answers if a mod adds or changes defs at runtime.
    public static List<WorkGiverDef> JobsOperating(ThingDef building) {
        List<WorkGiverDef> jobs = new List<WorkGiverDef>();

        foreach (WorkGiverDef job in DefDatabase<WorkGiverDef>.AllDefsListForReading) {
            // Think-tree-only jobs (no work type) have no Work tab column to point the player at.
            if (job.workType != null && Operates(job, building)) {
                jobs.Add(job);
            }
        }

        return jobs;
    }

    static bool Operates(WorkGiverDef job, ThingDef building) {
        // Bill jobs degrade to the broad PotentialBillGiver group when they list several workbenches,
        // so their explicit workbench list is the precise answer.
        if (job.fixedBillGiverDefs?.Contains(building) == true) {
            return true;
        }

        if (!TryGetRequest(job, out ThingRequest request)) {
            return false;
        }

        if (request.singleDef != null) {
            return request.singleDef == building;
        }

        return !genericGroups.Contains(request.group) && request.group.Includes(building);
    }

    static bool TryGetRequest(WorkGiverDef job, out ThingRequest request) {
        for (Type giverClass = job.giverClass; giverClass != null; giverClass = giverClass.BaseType) {
            if (requestByStatefulJob.TryGetValue(giverClass, out request)) {
                return true;
            }
        }

        request = default;
        if (brokenJobs.Contains(job)) {
            return false;
        }

        try {
            // Non-scanner jobs (think-tree driven, cell-based) never target a building through a request.
            if (job.Worker is not WorkGiver_Scanner scanner) {
                return false;
            }

            request = scanner.PotentialWorkThingRequest;
            return true;
        }
        catch (Exception exception) {
            brokenJobs.Add(job);
            Log.Warning(
                $"[What Work Type] Skipping {job.defName} ({job.giverClass?.FullName}): "
                + $"asking it for its targets threw {exception.GetType().Name}: {exception.Message}"
            );
            return false;
        }
    }
}
