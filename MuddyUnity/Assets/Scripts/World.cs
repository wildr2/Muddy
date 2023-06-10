using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : WorldBuilder
{
    private void Awake()
    {
        world1();
    }

    private void world1()
    {
        begin()

        .path("road")
            .desc("It's hot and windy outside the workshop.")
            .node("bay_doors", los: nearish)
                .door("road_workshop")
                .desc("They're shut. Light creeps between them.")
            .node("workshop", los: med)
                .desc("A crooked wooden barn.")
            .node("oak_tree", los: med)
                .desc("An oak tree.")
            .node("tower", 70, los: far)
                .desc("From a distance, the white tower of Schist peaks through distant clouds.")
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