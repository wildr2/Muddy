using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : WorldBuilder
{
    public int world_id = 1;

    private void Awake()
    {
        switch (world_id)
        {
            case 1: world1(); break;
            case 2: world2(); break;
        }
    }

    private void world1()
    {
        begin()

        .path("road")
            .desc("The road.")
            //.desc("It's hot and windy outside the workshop.")
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
                .door("road_stream")
                .desc("Jump down to the stream under the bridge.")
            .node("tree", med, los: med)
            .node("oak tree", los: med)
            .node("tree", los: med)
            .node("stream", los: med)
                .door("road_stream2")
            .node("tower", 70, los: far)
                .desc("The white tower peaks through distant clouds.", dist: -1)
                .desc("The white tower looms.", dist: med)
        .path("stream")
            .desc("The stream.")
            .node("smooth_pebble", los: near)
            .node("red_fish", los: near)
            .node("babbling", vnear, 0, los: near)
            .node("", 0, 0, los: med)
                .door("road_stream")
            .node("bridge", 0, vnear, los: med)
                .desc("I can't easily climb up to the bridge from here.")
            .node("cave", los: med)
                .door("stream_cave")
            .node("the_road", los: med)
                .door("road_stream2")
        .path("cave")
            .desc("The cave.")
            .node("dripping", los: near)
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

        .end();
    }

    private void world2()
    {
        begin()

        .path("road")
            .node("jump", med, los: nearish)
                .door("jump_river")
            .node("bridge", near, los: farish)
            .node("stairs", near, los: nearish)
                .door("bridge_river")
            .node("tree", med, los: med)
            .node("tower", 70, los: far)
        .path("river")
            .node("fish", near, los: nearish)
            .node("")
                .door("jump_river")
            .node("rock", near, los: nearish)
            .node("cave", near, los: nearish)
            .node("stairs", near, los: nearish)
                .door("bridge_river")

        .end();
    }
}
