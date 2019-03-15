// Script for generating a unique but persistent string identifier belonging to this 
// component
//
// We construct the identifier from two parts, the scene name and a guid.
// 
// The guid is guaranteed to be unique across all components loaded at 
// any given time. In practice this means the ID is unique within this scene. We 
// then append the name of the scene to it. This ensures that the identifier will be 
// unique accross all scenes. (as long as your scene names are unique)
// 
// The identifier is serialised ensuring it will remaing the same when the level is 
// reloaded
//
// This code copes with copying the game object we are part of, using prefabs and 
// additive level loading
//
// Final point - After adding this Component to a prefab, you need to open all the 
// scenes that contain instances of this prefab and resave them (to save the newly 
// generated identifier). I recommend manually saving it rather than just closing it
// and waiting for Unity to prompt you to save it, as this automatic mechanism 
// doesn't always seem to know exactly what needs saving and you end up being re-asked
// incessantly
//
// Written by Diarmid Campbell 2017 - feel free to use and ammend as you like
//
using UnityEngine;
using System.Collections.Generic;
using System;

namespace Thovex.WFC
{
    [ExecuteInEditMode]
    public class UniqueId : MonoBehaviour
    {
        public string uniquePrefabId;

    }
}