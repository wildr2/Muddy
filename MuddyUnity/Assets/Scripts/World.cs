using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : WorldBuilder
{
    protected override void Build()
    {
        begin()

        .path("road")
            .desc("The road.")
            .node("bay_doors", los: nearish)
                .door("road_workshop")
                .desc("They're shut. Light creeps between them.")
            .node("workshop", los: med)
                .desc("A crooked wooden barn.")
            .node("oak_tree", los: med)
                .desc("An oak tree.")
            .node("bridge", med, los: med)
            .node("stream", los: med)
            .node("jump", los: med)
                .desc("That seems dangerous, it's a long way down.")
                .cond((state) => { return state["parachute_taken"]; })
                    .door("road_stream")
                    .desc("Ready the parachute.")
                .end_cond()
            .node("gate", 0, 0, los: med)
                .cond((state) => { return !state["gate_opened"]; })
                    .desc("A sturdy iron gate. It's locked.")
                    .wall()
                    .act(action: (state) => { state["gate_opened"] = state["key_taken"]; })
                .end_cond()
                .cond((state) => { return state["gate_opened"]; })
                    .desc("A sturdy iron gate. It's open.")
                .end_cond()
            .node("tree", med, los: med)
                .desc("A tree.")
            .node("oak tree", los: med)
                .cond((state) => { return !state["cave_exists"]; })
                    .desc("~We used to collect sticks here to build campfires in the *cave* by the stream.~")
                    .act(action: (state) => { state["cave_exists"] = 1; })
                .end_cond()
                .cond((state) => { return state["cave_exists"]; })
                    .desc("We used to collect sticks here to build campfires in the *cave* by the stream.")
                .end_cond()
            .node("tree#2", los: med)
                .desc("A tree.")
            .node("stream#2", los: med)
                .door("road_stream2")
            .node("gate#2", 0, 0, los: nearish)
                .cond((state) => { return !state["gate2_opened"]; })
                    .desc("A sturdy iron gate. It's locked.")
                    .wall()
                    .act(action: (state) => { state["gate2_opened"] = state["key_taken"]; })
                .end_cond()
                .cond((state) => { return state["gate2_opened"]; })
                    .desc("A sturdy iron gate. It's open.")
                .end_cond()
            .node("tree#3", 60, los: med)
            .node("tower", los: far)
                .desc("The white tower peaks through distant clouds.", dist: -1)
                .desc("The white tower looms.", dist: med)
                .desc("YOU WIN!!!", dist: nearish)
            .node("tree#4", los: med)
        .path("stream")
            .desc("The stream.")
            .node("smooth_pebble", los: near)
            .node("red_fish", los: near)
            .node("babbling", rmargin: 0, los: near)
            .node("", 0, 0, los: med)
                .door("road_stream")
            .node("bridge", lmargin: 0, los: med)
                .desc("I can't easily climb up to the bridge from here.")
            .cond((state) => { return state["cave_exists"]; })
                .node("cave", med, los: med)
                    .door("stream_cave")
            .end_cond()
            .node("the_road", lmargin: nearish, los: med)
                .door("road_stream2")
        .path("cave")
            .desc("The cave.")
            .node("dripping", los: near)
            .cond((state) => { return !state["key_taken"]; })
                .node("key", los: vnear)
                    .desc("The skeleton key!")
                    .act(action: (state) => { state["key_taken"] = 1; })
            .end_cond()
            .node("darkness", los: near)
            .node("outside", los: med)
                .door("stream_cave")
        .path("workshop")
            .desc("Sunlight filters through the workshop's many grimy windows, dappling scattered wooden benches and overbearing spider plants.")
            .node("glass_jar", los: near)
                .desc("A small empty glass jar sits atop the workbench.")
            .node("ladder", los: med)
                .desc("Looks like it goes up to the attic.")
                .door("workshop_attic")
            .node("workbench", los: nearish)
                .desc("It's a sturdy wooden bench.")
            .node("backpack", los: near)
                .desc("The professor must have left his pack in the workshop.")
            .node("bay_doors", los: med)
                .desc("They're shut. Light creeps between them.")
                .door("road_workshop")
            .node("kayak", los: med)
                .desc("A simple red boat, seems sky-worthy enough.")
        .path("attic")
            .desc("It's a small attic.")
            .node("ladder", los: med)
                .desc("The ladder.")
                .door("workshop_attic")
            .node("crate", los: nearish)
                .desc("A dusty wooden crate.")
                .act(action: (state) => { state["crate_opened"] = 1; })

            .cond((state) => { return state["crate_opened"] && !state["parachute_taken"]; })
                .node("parachute", los: nearish)
                    .desc("A parachute.")
                    .act(action: (state) => { state["parachute_taken"] = 1; })
            .end_cond()

        .end();
    }
}
